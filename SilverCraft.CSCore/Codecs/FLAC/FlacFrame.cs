#define GET_BUFFER_INTERNAL

using System.Buffers;
using System.Collections.ObjectModel;
using SilverCraft.CSCore.Codecs.FLAC.SubFrames;
using SilverCraft.CSCore.Utils;

namespace SilverCraft.CSCore.Codecs.FLAC;

/// <summary>
///     Represents a frame inside of an Flac-Stream.
/// </summary>
public sealed partial class FlacFrame : IDisposable
{
    private const int DefaultBufferSize = 65536;
    private const int DefaultMinBufferedBytes = 32768;

    private int[]? _destBuffer;
    private bool _disposed;
    private int[]? _residualBuffer;
    private Stream _stream;
    private FlacMetadataStreamInfo? _streamInfo;
    private readonly List<FlacSubFrameData> _subFrameData;

    private byte[]? _streamBuffer;
    private int _bufOffset;
    private int _bufCount;
    private long _expectedStreamPos;
    private bool _eof;

#if FLAC_DEBUG
    private ReadOnlyCollection<FlacSubFrameBase>? _subFrames;
#endif

    private FlacFrame(Stream stream, FlacMetadataStreamInfo? streamInfo = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("Stream is not readable");

        _stream = stream;
        _streamInfo = streamInfo;
        Header = new FlacFrameHeader();
        _subFrameData = new List<FlacSubFrameData>(8);
        _expectedStreamPos = stream.CanSeek ? stream.Position : 0;
    }

    /// <summary>
    ///     Gets the header of the flac frame.
    /// </summary>
    public FlacFrameHeader Header { get; private set; }

    /// <summary>
    ///     Gets the CRC16-checksum.
    /// </summary>
    public short Crc16 { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the decoder has encountered an error with this frame.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this frame contains an error; otherwise, <c>false</c>.
    /// </value>
    public bool HasError { get; private set; }

    internal bool IsEndOfStream { get; private set; }

    internal void ResetBuffer()
    {
        _bufOffset = 0;
        _bufCount = 0;
        _eof = false;
        _expectedStreamPos = _stream != null && _stream.CanSeek ? _stream.Position : 0;
    }

    /// <summary>
    ///     Disposes the <see cref="FlacFrame" /> and releases all associated resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        GC.SuppressFinalize(this);
        if (_streamBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(_streamBuffer);
            _streamBuffer = null;
        }

        Header = null!;
        _destBuffer = null;
        _residualBuffer = null;
        _stream = null!;
        _streamInfo = null!;
        _disposed = true;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="FlacFrame" /> class based on the specified <paramref name="stream" />.
    /// </summary>
    /// <param name="stream">The stream which contains the flac frame.</param>
    /// <returns>A new instance of the <see cref="FlacFrame" /> class.</returns>
    public static FlacFrame FromStream(Stream stream)
    {
        var frame = new FlacFrame(stream);
        return frame;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="FlacFrame" /> class based on the specified <paramref name="stream" /> and
    ///     some basic stream information.
    /// </summary>
    /// <param name="stream">The stream which contains the flac frame.</param>
    /// <param name="streamInfo">Some basic information about the flac stream.</param>
    /// <returns>A new instance of the <see cref="FlacFrame" /> class.</returns>
    public static FlacFrame FromStream(Stream stream, FlacMetadataStreamInfo streamInfo)
    {
        var frame = new FlacFrame(stream, streamInfo);
        return frame;
    }

    /// <summary>
    ///     Tries to read the next flac frame inside the specified stream and returns a value which indicates whether the
    ///     next flac frame could be successfully read.
    /// </summary>
    /// <returns>True if the next flac frame could be successfully read; false if not.</returns>
    public bool NextFrame()
    {
        Decode(_stream, _streamInfo);
        return !HasError;
    }

    private void EnsureBuffered(int minBytes)
    {
        if (_eof || _bufCount >= minBytes)
            return;

        if (_streamBuffer == null)
        {
            var initialSize = Math.Max(DefaultBufferSize, minBytes);
            _streamBuffer = ArrayPool<byte>.Shared.Rent(initialSize);
        }
        else if (_streamBuffer.Length < minBytes)
        {
            var next = ArrayPool<byte>.Shared.Rent(minBytes);
            if (_bufCount > 0)
                Buffer.BlockCopy(_streamBuffer, _bufOffset, next, 0, _bufCount);
            ArrayPool<byte>.Shared.Return(_streamBuffer);
            _streamBuffer = next;
            _bufOffset = 0;
        }
        else if (_bufOffset > 0)
        {
            if (_bufCount > 0)
                Buffer.BlockCopy(_streamBuffer, _bufOffset, _streamBuffer, 0, _bufCount);
            _bufOffset = 0;
        }

        while (_bufCount < minBytes)
        {
            var read = _stream.Read(_streamBuffer, _bufCount, _streamBuffer.Length - _bufCount);
            if (read == 0)
            {
                _eof = true;
                break;
            }

            _bufCount += read;
            if (_stream.CanSeek)
                _expectedStreamPos += read;
        }
    }

    private void Decode(Stream stream, FlacMetadataStreamInfo? streamInfo)
    {
        _stream = stream;
        _streamInfo = streamInfo;
        IsEndOfStream = false;

        if (_stream.CanSeek)
        {
            var currentPos = _stream.Position;
            if (currentPos != _expectedStreamPos)
            {
                _bufOffset = 0;
                _bufCount = 0;
                _eof = false;
                _expectedStreamPos = currentPos;
            }
        }

        var minBuffered = streamInfo is { MaxFrameSize: > 0 }
            ? Math.Max(FlacConstant.FrameHeaderSize, streamInfo.MaxFrameSize)
            : DefaultMinBufferedBytes;

        EnsureBuffered(minBuffered);

        if (_bufCount == 0 && _eof)
        {
            IsEndOfStream = true;
            HasError = true;
            return;
        }

        var frameStreamPos = _stream.CanSeek ? _expectedStreamPos - _bufCount : -1;
        var headerSliceLen = Math.Min(FlacConstant.FrameHeaderSize, _bufCount);

        if (!Header.TryParseFromSpan(
                _streamBuffer.AsSpan(_bufOffset, headerSliceLen),
                streamInfo,
                doCrc: true,
                out var headerBytes,
                logErrors: true,
                frameStreamPos))
        {
            if (_bufCount > 0)
            {
                _bufOffset++;
                _bufCount--;
            }

            if (_bufCount == 0 && _eof)
                IsEndOfStream = true;

            HasError = true;
            return;
        }

        var worstCaseBytes = headerBytes + (Header.BlockSize * Header.Channels * ((Header.BitsPerSample + 7) / 8)) + 1024;
        if (streamInfo is not { MaxFrameSize: > 0 } && !_eof && _bufCount < worstCaseBytes)
            EnsureBuffered(worstCaseBytes);

        var decoded = TryReadSubFramesWithRetry(headerBytes, worstCaseBytes);
        if (decoded) return;
        if (_bufCount > 0)
        {
            _bufOffset++;
            _bufCount--;
        }

        if (_bufCount == 0 && _eof)
            IsEndOfStream = true;

        HasError = true;
    }

    private bool TryReadSubFramesWithRetry(int headerBytes, int worstCaseBytes)
    {
        if (_eof || _bufCount >= worstCaseBytes) return ReadSubFrames(headerBytes);
        try
        {
            if (ReadSubFrames(headerBytes))
                return true;
        }
        catch (FlacException)
        {
            // A lying/too-small STREAMINFO MaxFrameSize can cause truncation into zero-padding.
            // Re-buffer up to worstCaseBytes and retry before treating it as a genuine frame error.
        }

        EnsureBuffered(worstCaseBytes);

        return ReadSubFrames(headerBytes);
    }

    private bool ReadSubFrames(int headerBytes)
    {
#if FLAC_DEBUG
        var subFrames = new List<FlacSubFrameBase>(Header.Channels);
#endif

        var requiredSize = Header.Channels * Header.BlockSize;
        if (_destBuffer == null || _destBuffer.Length < requiredSize)
            _destBuffer = new int[requiredSize];
        if (_residualBuffer == null || _residualBuffer.Length < requiredSize)
            _residualBuffer = new int[requiredSize];

        while (_subFrameData.Count < Header.Channels)
            _subFrameData.Add(new FlacSubFrameData());

        for (var c = 0; c < Header.Channels; c++)
        {
            _subFrameData[c].DestinationBuffer = _destBuffer.AsMemory(c * Header.BlockSize, Header.BlockSize);
            _subFrameData[c].ResidualBuffer = _residualBuffer.AsMemory(c * Header.BlockSize, Header.BlockSize);
        }

        var payloadSpan = _streamBuffer.AsSpan(_bufOffset + headerBytes, _bufCount - headerBytes);
        var reader = new SpanBitReader(payloadSpan);

        for (var c = 0; c < Header.Channels; c++)
        {
            var bitsPerSample = Header.BitsPerSample;
            switch (Header.ChannelAssignment)
            {
                case ChannelAssignment.MidSide or ChannelAssignment.LeftSide:
                    bitsPerSample += c;
                    break;
                case ChannelAssignment.RightSide:
                    bitsPerSample += 1 - c;
                    break;
            }

#if FLAC_DEBUG
            var subframe = FlacSubFrameBase.GetSubFrame(ref reader, _subFrameData[c], Header, bitsPerSample);
            if (subframe == null)
                return false;
            subFrames.Add(subframe);
#else
            if (!FlacSubFrameBase.Decode(ref reader, _subFrameData[c], Header, bitsPerSample))
                return false;
#endif
        }

        reader.Flush();
        Crc16 = (short)reader.ReadBits(16);

        var totalConsumed = headerBytes + reader.Position;
        if (reader.IsTruncated || totalConsumed > _bufCount)
            return false;

        _bufOffset += totalConsumed;
        _bufCount -= totalConsumed;

        MapToChannels(_subFrameData);
        HasError = false;

#if FLAC_DEBUG
        _subFrames = subFrames.AsReadOnly();
#endif
        return true;
    }

    private void MapToChannels(List<FlacSubFrameData> subFrames)
    {
        if (Header.ChannelAssignment == ChannelAssignment.Independent || Header.Channels < 2)
            return;

        var blockSize = Header.BlockSize;
        var left = subFrames[0].DestinationSpan[..blockSize];
        var right = subFrames[1].DestinationSpan[..blockSize];
        switch (Header.ChannelAssignment)
        {
            case ChannelAssignment.LeftSide:
            {
                for (var i = 0; i < blockSize; i++)
                    right[i] = left[i] - right[i];

                break;
            }
            case ChannelAssignment.RightSide:
            {
                for (var i = 0; i < blockSize; i++)
                    left[i] += right[i];

                break;
            }
            case ChannelAssignment.MidSide:
            {
                for (var i = 0; i < blockSize; i++)
                {
                    var mid = left[i] << 1;
                    var side = right[i];

                    mid |= side & 1;

                    left[i] = (mid + side) >> 1;
                    right[i] = (mid - side) >> 1;
                }
                break;
            }
        }
    }
}