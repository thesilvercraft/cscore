using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SilverCraft.CSCore.Tags.ID3;

namespace SilverCraft.CSCore.Codecs.FLAC;

/// <summary>
///     Provides a decoder for decoding flac (Free Lostless Audio Codec) data.
/// </summary>
public class FlacFile : IWaveSource
{
    private readonly object _bufferLock = new();

    private readonly bool _closeStream;
    private readonly FlacPreScan? _scan;
    private readonly Stream _stream;
    private readonly FlacMetadataStreamInfo _streamInfo;
    private readonly FlacMetadataSeekTable? _seekTable;
    private FlacFrame? _frame;
    private readonly long _firstFrameOffset;
    private int _frameIndex;

    //overflow:
    private byte[]? _overflowBuffer;

    private int _overflowCount;
    private int _overflowOffset;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlacFile" /> class.
    /// </summary>
    /// <param name="fileName">Filename which of a flac file which should be decoded.</param>
    public FlacFile(string fileName)
        : this(File.OpenRead(fileName))
    {
        _closeStream = true;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlacFile" /> class.
    /// </summary>
    /// <param name="stream">Stream which contains flac data which should be decoded.</param>
    public FlacFile(Stream stream)
        : this(stream, FlacPreScanMode.Default)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlacFile" /> class.
    /// </summary>
    /// <param name="stream">Stream which contains flac data which should be decoded.</param>
    /// <param name="scanFlag">Scan mode which defines how to scan the flac data for frames.</param>
    public FlacFile(Stream stream, FlacPreScanMode scanFlag)
        : this(stream, scanFlag, null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FlacFile" /> class.
    /// </summary>
    /// <param name="stream">Stream which contains flac data which should be decoded.</param>
    /// <param name="scanFlag">Scan mode which defines how to scan the flac data for frames.</param>
    /// <param name="onscanFinished">
    ///     Callback which gets called when the pre scan processes finished. Should be used if the
    ///     <paramref name="scanFlag" /> argument is set the <see cref="FlacPreScanMode.Async" />.
    ///    If a SeekTable is found scanning will be skipped but the callback will be called with a null.
    /// </param>
    public FlacFile(Stream stream, FlacPreScanMode scanFlag,
        Action<FlacPreScanFinishedEventArgs?>? onscanFinished)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("Stream is not readable.", nameof(stream));

        _stream = stream;
        _closeStream = true;

        while (ID3v2.SkipTag(stream))
        {
            //skip ID3v2
        }

        var beginSync = new byte[4];
        var read = stream.Read(beginSync, 0, beginSync.Length);
        if (read < beginSync.Length)
            throw new EndOfStreamException("Can not read \"fLaC\" sync.");
        if (beginSync[0] != 'f' || beginSync[1] != 'L' || beginSync[2] != 'a' || beginSync[3] != 'C')
            throw new FlacException("Invalid Flac-File. \"fLaC\" Sync not found.", FlacLayer.OutSideOfFrame);

        var metadata = FlacMetadata.ReadAllMetadataFromStream(stream).ToList();
        Metadata = metadata.AsReadOnly();
        if (metadata.Count <= 0)
            throw new FlacException("No Metadata found.", FlacLayer.Metadata);

        if (metadata.First(x => x.MetaDataType == FlacMetaDataType.StreamInfo) is not FlacMetadataStreamInfo
            streamInfo)
            throw new FlacException("No StreamInfo-Metadata found.", FlacLayer.Metadata);
        if (metadata.FirstOrDefault(x => x.MetaDataType == FlacMetaDataType.Seektable) is FlacMetadataSeekTable jackpot)
        {
            Debug.WriteLine("Flac SEEKATABLE FOUND :)");
            _seekTable = jackpot;
        }

        _streamInfo = streamInfo;
        WaveFormat = CreateWaveFormat(streamInfo);
        Debug.WriteLine("Flac StreamInfo found -> WaveFormat: " + WaveFormat);
        Debug.WriteLine("Flac-File-Metadata read.");

        if (_seekTable is { SeekPoints.Length: > 0 })
        {
            Debug.WriteLine("Huh? Oh, wait! We're reading a flac with a SEEKATABLE. Lossless, too - no need to scan.");
            _firstFrameOffset = _stream.Position;
            onscanFinished?.Invoke(null);
            return;
        }

        _firstFrameOffset = _stream.Position;
        //prescan stream
        if (scanFlag == FlacPreScanMode.None) return;
        var scan = new FlacPreScan(stream);
        scan.ScanFinished += (_, e) => { onscanFinished?.Invoke(e); };
        scan.ScanStream(_streamInfo, scanFlag);
        _scan = scan;
    }

    /// <summary>
    ///     Gets a list with all found metadata fields.
    /// </summary>
    public ReadOnlyCollection<FlacMetadata> Metadata { get; protected set; }

    private FlacFrame? Frame => _frame ??= FlacFrame.FromStream(_stream, _streamInfo);

    /// <summary>
    ///     Gets the output <see cref="CSCore.WaveFormat" /> of the decoder.
    /// </summary>
    public WaveFormat WaveFormat { get; }

    /// <summary>
    ///     Gets a value which indicates whether the seeking is supported. True means that seeking is supported; False means
    ///     that seeking is not supported.
    /// </summary>
    public bool CanSeek => _seekTable != null || _scan != null;

    /// <summary>
    ///     Reads a sequence of bytes from the <see cref="FlacFile" /> and advances the position within the stream by the
    ///     number of bytes read.
    /// </summary>
    /// <param name="buffer">
    ///     An array of bytes. When this method returns, the <paramref name="buffer" /> contains the specified
    ///     byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
    ///     <paramref name="count" /> - 1) replaced by the bytes read from the current source.
    /// </param>
    /// <param name="offset">
    ///     The zero-based byte offset in the <paramref name="buffer" /> at which to begin storing the data
    ///     read from the current stream.
    /// </param>
    /// <param name="count">The maximum number of bytes to read from the current source.</param>
    /// <returns>The total number of bytes read into the buffer.</returns>
    public int Read(byte[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var read = 0;
        count -= count % WaveFormat.BlockAlign;

        lock (_bufferLock)
        {
            var destinationSpan = buffer.AsSpan(offset, count);
            var cachedBytesRead = GetOverflows(destinationSpan);
            read += cachedBytesRead;
            destinationSpan = destinationSpan[cachedBytesRead..];
            while (read < count)
            {
                var frame = Frame;
                if (frame == null)
                    return read;
                while (!frame.NextFrame())
                {
                    if (_scan is { Frames.Count: > 0 })
                    {
                        if (++_frameIndex >= _scan.Frames.Count)
                            return read;
                        _stream.Position = _scan.Frames[_frameIndex].StreamOffset;
                    }
                    else if (_stream.Position >= _stream.Length)
                    {
                        return read;
                    }
                }

                _frameIndex++;
                var bufferLength = frame.GetBuffer(ref _overflowBuffer);
                ReadOnlySpan<byte> frameSpan = _overflowBuffer.AsSpan(0, bufferLength);
                var bytesToCopy = Math.Min(destinationSpan.Length, frameSpan.Length);

                if (bytesToCopy > 0)
                {
                    frameSpan[..bytesToCopy].CopyTo(destinationSpan);
                    read += bytesToCopy;
                    destinationSpan = destinationSpan[bytesToCopy..];
                }
                if (bufferLength > bytesToCopy)
                {
                    _overflowCount = bufferLength - bytesToCopy;
                    _overflowOffset = bytesToCopy;
                }
                else
                {
                    _overflowCount = 0;
                    _overflowOffset = 0;
                }
            }
        }
        _position += read;
        return read;
    }

    /// <summary>
    ///     Gets or sets the position of the <see cref="FlacFile" /> in bytes.
    /// </summary>
    public long Position
    {
        get
        {
            if (_disposed)
                return 0;

            lock (_bufferLock)
            {
                return _position;
            }
        }
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!CanSeek)
                return;

            lock (_bufferLock)
            {
                value = Math.Max(Math.Min(value, Length), 0);
                value -= value % WaveFormat.BlockAlign;

                if (_seekTable is { EntryCount: > 0 })
                {
                    var targetSample = value / WaveFormat.BlockAlign;
                    FlacSeekPoint? bestPoint = null;

                    for (var i = 0; i < _seekTable.EntryCount; i++)
                    {
                        var point = _seekTable[i];
                        if (point.SampleNumber == unchecked((long)0xFFFFFFFFFFFFFFFF)) continue; // skip placeholders

                        if (point.SampleNumber > targetSample) continue;
                        if (bestPoint == null || point.SampleNumber > bestPoint.SampleNumber)
                            bestPoint = point;
                    }

                    if (bestPoint != null)
                    {
                        _stream.Position = _firstFrameOffset + bestPoint.Offset;
                        _frame = FlacFrame.FromStream(_stream, _streamInfo);
                        _overflowCount = 0;
                        _overflowOffset = 0;

                        _position = bestPoint.SampleNumber * WaveFormat.BlockAlign;

                        var diff = (int)(value - Position);
                        diff -= diff % WaveFormat.BlockAlign;
                        if (diff > 0) this.ReadBytes(diff);

                        return;
                    }
                }

                if (_scan == null) return;
                for (var i = 0; i < _scan.Frames.Count; i++)
                {
                    if (value / WaveFormat.BlockAlign > _scan.Frames[i].SampleOffset) continue;
                    if (i != 0)
                        i--;

                    _stream.Position = _scan.Frames[i].StreamOffset;
                    _frameIndex = i;
                    if (_stream.Position >= _stream.Length)
                        throw new EndOfStreamException("Stream got EOF.");
                    _position = _scan.Frames[i].SampleOffset * WaveFormat.BlockAlign;
                    _overflowCount = 0;
                    _overflowOffset = 0;

                    var diff = (int)(value - Position);
                    diff -= diff % WaveFormat.BlockAlign;
                    if (diff > 0) this.ReadBytes(diff);

                    break;
                }
            }
        }
    }

    /// <summary>
    ///     Gets the length of the <see cref="FlacFile" /> in bytes.
    /// </summary>
    public long Length
    {
        get
        {
            if (_disposed)
                return 0;
            return _streamInfo.TotalSamples * WaveFormat.BlockAlign;
        }
    }

    /// <summary>
    ///     Disposes the <see cref="FlacFile" /> instance and disposes the underlying stream.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static WaveFormat CreateWaveFormat(FlacMetadataStreamInfo streamInfo)
    {
        if (streamInfo.Channels is <= 2 or > 8)
            return new WaveFormat(streamInfo.SampleRate, streamInfo.BitsPerSample, streamInfo.Channels,
                AudioEncoding.Pcm);
        var channelMask = streamInfo.Channels switch
        {
            3 =>
                //2.1
                ChannelMask.SpeakerFrontLeft | ChannelMask.SpeakerFrontRight | ChannelMask.SpeakerFrontCenter,
            4 =>
                //quadraphonic
                ChannelMask.SpeakerFrontLeft | ChannelMask.SpeakerFrontRight | ChannelMask.SpeakerBackLeft |
                ChannelMask.SpeakerBackRight,
            5 =>
                //5.0
                ChannelMask.SpeakerFrontLeft | ChannelMask.SpeakerFrontRight | ChannelMask.SpeakerFrontCenter |
                ChannelMask.SpeakerSideLeft | ChannelMask.SpeakerSideRight,
            6 =>
                //5.1
                ChannelMask.SpeakerFrontLeft | ChannelMask.SpeakerFrontRight | ChannelMask.SpeakerFrontCenter |
                ChannelMask.SpeakerLowFrequency | ChannelMask.SpeakerSideLeft | ChannelMask.SpeakerSideRight,
            7 =>
                //6.1
                ChannelMask.SpeakerFrontLeft | ChannelMask.SpeakerFrontRight | ChannelMask.SpeakerFrontCenter |
                ChannelMask.SpeakerLowFrequency | ChannelMask.SpeakerSideLeft | ChannelMask.SpeakerSideRight |
                ChannelMask.SpeakerBackCenter,
            8 =>
                //7.1
                ChannelMask.SpeakerFrontLeft | ChannelMask.SpeakerFrontRight | ChannelMask.SpeakerFrontCenter |
                ChannelMask.SpeakerLowFrequency | ChannelMask.SpeakerBackLeft | ChannelMask.SpeakerBackRight |
                ChannelMask.SpeakerSideLeft | ChannelMask.SpeakerSideRight,
            _ => throw new InvalidOperationException("Invalid number of channels. This error should not occur.")
        };
        return new WaveFormatExtensible(streamInfo.SampleRate, streamInfo.BitsPerSample, streamInfo.Channels,
            AudioSubTypes.Pcm, channelMask);
    }

    private int GetOverflows(Span<byte> destinationSpan)
    {
        if (_overflowCount == 0 || _overflowBuffer == null || destinationSpan.IsEmpty)
            return 0;
        var bytesToCopy = Math.Min(destinationSpan.Length, _overflowCount);

        ReadOnlySpan<byte> sourceSpan = _overflowBuffer.AsSpan(_overflowOffset, bytesToCopy);
        sourceSpan.CopyTo(destinationSpan);
        _overflowCount -= bytesToCopy;
        _overflowOffset += bytesToCopy;

        return bytesToCopy;
    }

    /// <summary>
    ///     Disposes the <see cref="FlacFile" /> instance and disposes the underlying stream.
    /// </summary>
    /// <param name="disposing">
    ///     True to release both managed and unmanaged resources; false to release only unmanaged
    ///     resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        lock (_bufferLock)
        {
            if (_disposed) return;
            _frame?.Dispose();
            _frame = null;

            if (_stream != null && !_stream.IsClosed() && _closeStream)
                _stream.Dispose();
            if (_overflowBuffer != null) ArrayPool<byte>.Shared.Return(_overflowBuffer);

            _disposed = true;
        }
    }
    private bool _disposed;
    private long _position;
}