#define GET_BUFFER_INTERNAL

using System.Buffers;
using System.Collections.ObjectModel;
using SilverCraft.CSCore.Codecs.FLAC.SubFrames;

namespace SilverCraft.CSCore.Codecs.FLAC;

/// <summary>
///     Represents a frame inside of an Flac-Stream.
/// </summary>
public sealed partial class FlacFrame : IDisposable
{
    private int[]? _destBuffer;

    private bool _disposed;
    private int[]? _residualBuffer;
    private Stream _stream;
    private FlacMetadataStreamInfo _streamInfo;
    private List<FlacSubFrameData> _subFrameData;
#if FLAC_DEBUG
    private ReadOnlyCollection<FlacSubFrameBase> _subFrames;
#endif

    private FlacFrame(Stream stream, FlacMetadataStreamInfo? streamInfo = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("Stream is not readable");

        _stream = stream;
        _streamInfo = streamInfo;
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

    /// <summary>
    ///     Disposes the <see cref="FlacFrame" /> and releases all associated resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        GC.SuppressFinalize(this);
        Header = null;
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
        //return frame.HasError ? null : frame;
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
        //return frame.HasError ? null : frame;
    }

    /// <summary>
    ///     Tries to read the next flac frame inside of the specified stream and returns a value which indicates whether the
    ///     next flac frame could be successfully read.
    /// </summary>
    /// <returns>True if the next flac frame could be successfully read; false if not.</returns>
    public bool NextFrame()
    {
        Decode(_stream, _streamInfo);
        return !HasError;
    }

    private void Decode(Stream stream, FlacMetadataStreamInfo streamInfo)
    {
        Header = new FlacFrameHeader(stream, streamInfo);
        _stream = stream;
        _streamInfo = streamInfo;
        HasError = Header.HasError;
        if (HasError) return;
        ReadSubFrames();
    }

    private void ReadSubFrames() // Drop unsafe keyword entirely here!
{
    var subFrames = new List<FlacSubFrameBase>();

    var requiredSize = Header.Channels * Header.BlockSize;
    if (_destBuffer == null || _destBuffer.Length < requiredSize)
        _destBuffer = new int[requiredSize];
    if (_residualBuffer == null || _residualBuffer.Length < requiredSize)
        _residualBuffer = new int[requiredSize];

    _subFrameData = [];
    for (var c = 0; c < Header.Channels; c++) 
        _subFrameData.Add(new FlacSubFrameData());

    var minimumSize = 0x20000;
    long calculatedSize = ((_streamInfo.MaxFrameSize * Header.Channels * Header.BitsPerSample * 2) >> 3) -
                          FlacConstant.FrameHeaderSize;
    if (calculatedSize > minimumSize) minimumSize = (int)calculatedSize;

    var buffer = ArrayPool<byte>.Shared.Rent(minimumSize);

    try
    {
        var read = _stream.Read(buffer, 0, (int)Math.Min(buffer.Length, _stream.Length - _stream.Position));

        for (var c = 0; c < Header.Channels; c++)
        {
            _subFrameData[c].DestinationBuffer = _destBuffer.AsMemory(c * Header.BlockSize, Header.BlockSize);
            _subFrameData[c].ResidualBuffer = _residualBuffer.AsMemory(c * Header.BlockSize, Header.BlockSize);
        }

        using var reader = new FlacBitReader(buffer, 0);
        
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

            var subframe = FlacSubFrameBase.GetSubFrame(reader, _subFrameData[c], Header, bitsPerSample);
            subFrames.Add(subframe);
        }

        reader.Flush(); 

        Crc16 = (short)reader.ReadBits(16);

        _stream.Position -= read - reader.Position;

        MapToChannels(_subFrameData);
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }
#if FLAC_DEBUG
    _subFrames = subFrames.AsReadOnly();
#endif
}

    private void MapToChannels(List<FlacSubFrameData> subFrames)
    {
        switch (Header.ChannelAssignment)
        {
            case ChannelAssignment.LeftSide:
            {
                var left = subFrames[0].DestinationSpan;
                var right = subFrames[1].DestinationSpan;

                for (var i = 0; i < Header.BlockSize; i++)
                    right[i] = left[i] - right[i];

                break;
            }
            case ChannelAssignment.RightSide:
            {
                var left = subFrames[0].DestinationSpan;
                var right = subFrames[1].DestinationSpan;

                for (var i = 0; i < Header.BlockSize; i++)
                    left[i] += right[i];

                break;
            }
            case ChannelAssignment.MidSide:
            {
                var midSpan = subFrames[0].DestinationSpan;
                var sideSpan = subFrames[1].DestinationSpan;

                for (var i = 0; i < Header.BlockSize; i++)
                {
                    var mid = midSpan[i] << 1;
                    var side = sideSpan[i];

                    mid |= side & 1;

                    midSpan[i] = (mid + side) >> 1;
                    sideSpan[i] = (mid - side) >> 1;
                }

                break;
            }
        }
    }
}