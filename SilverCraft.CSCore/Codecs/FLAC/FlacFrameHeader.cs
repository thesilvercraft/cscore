using Microsoft.Extensions.Logging;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    /// <summary>
    /// Represents the header of a <see cref="FlacFrame"/>.
    /// </summary>
    public sealed partial class FlacFrameHeader
    {
        private int _blocksizeHint; //if bsindex == 6 || 7
        private int _sampleRateHint; //if sampleRateIndex == 12 || 13 || 14


        /// <summary>
        /// Gets number of samples, the frame contains.
        /// </summary>
        /// <value>
        /// The number of samples, the frame contains.
        /// </value>
        public int BlockSize { get; private set; }

        /// <summary>
        /// Gets the sample rate in Hz.
        /// </summary>
        /// <value>
        /// The sample rate in Hz.
        /// </value>
        public int SampleRate { get; private set; }

        /// <summary>
        /// Gets the number of channels.
        /// </summary>
        /// <value>
        /// The number of channels.
        /// </value>
        public int Channels { get; private set; }

        /// <summary>
        /// Gets the channel assignment.
        /// </summary>
        /// <value>
        /// The channel assignment.
        /// </value>
        public ChannelAssignment ChannelAssignment { get; private set; }

        /// <summary>
        /// Gets the bits per sample.
        /// </summary>
        /// <value>
        /// The bits per sample.
        /// </value>
        public int BitsPerSample { get; private set; }

        /// <summary>
        /// Gets a value which indicates whether the frame provides the <see cref="SampleNumber"/> or the <see cref="FrameNumber"/>.
        /// </summary>
        /// <value>
        /// A value which indicates whether the frame provides the <see cref="SampleNumber"/> or the <see cref="FrameNumber"/>.
        /// </value>
        public BlockingStrategy BlockingStrategy { get; private set; }

        /// <summary>
        /// Gets the frame's starting sample number.
        /// </summary>
        /// <value>
        /// The frame's starting sample number.
        /// </value>
        /// <remarks>Only available if the <see cref="BlockingStrategy"/> is set to <see cref="BlockingStrategy.VariableBlockSize"/>.</remarks>
        public long SampleNumber { get; private set; }

        /// <summary>
        /// Gets the frame's number.
        /// </summary>
        /// <value>
        /// The frame's number.
        /// </value>
        /// <remarks>Only available if the <see cref="BlockingStrategy"/> is set to <see cref="BlockingStrategy.FixedBlockSize"/>.</remarks>
        public int FrameNumber { get; private set; }

        /// <summary>
        /// Gets the 8-bit crc checksum of the frame header.
        /// </summary>
        /// <value>
        /// The 8-bit crc checksum of the frame header.
        /// </value>
        public byte Crc8 { get; private set; }

        private bool DoCrc { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has error; otherwise, <c>false</c>.
        /// </value>
        public bool HasError { get; private set; }
        /// <summary>
        /// The number of bytes parsing this header took
        /// Only filled if constructed via byte array
        /// </summary>
        public int BytesConsumed { get; private set; }
        /// <summary>
        /// Gets the stream position.
        /// </summary>
        /// <value>
        /// The stream position.
        /// </value>
        public long StreamPosition { get; private set; }

        private sealed record LoggerCacheEntry(ILoggerFactory Factory, ILogger Logger);

        private static LoggerCacheEntry? s_loggerCache;

        private static ILogger GetSharedLogger()
        {
            var factory = LogLocation.Factory;
            var cached = Volatile.Read(ref s_loggerCache);
            if (cached is not null && ReferenceEquals(cached.Factory, factory))
                return cached.Logger;

            ILogger logger = factory is Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory
                ? Microsoft.Extensions.Logging.Abstractions.NullLogger<FlacFrameHeader>.Instance
                : factory.CreateLogger<FlacFrameHeader>();

            Volatile.Write(ref s_loggerCache, new LoggerCacheEntry(factory, logger));
            return logger;
        }

        private static ILogger _logger => GetSharedLogger();

        internal FlacFrameHeader()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacFrameHeader"/> class.
        /// </summary>
        /// <param name="stream">The underlying stream which contains the <see cref="FlacFrameHeader"/>.</param>
        /// <param name="streamInfo">The stream-info-metadata-block of the flac stream which provides some basic information about the flac framestream. Can be set to null.</param>
        /// <param name="doCrc">A value which indicates whether the crc8 checksum of the <see cref="FlacFrameHeader"/> should be calculated.</param>
        public FlacFrameHeader(Stream stream, FlacMetadataStreamInfo? streamInfo = null, bool doCrc = true)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanRead) throw new ArgumentException("stream is not readable");
            if (!stream.CanSeek)
                throw new ArgumentException("stream must be readable and seekable", nameof(stream));

            HasError = !TryParseFromStream(stream, streamInfo, doCrc);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FlacFrameHeader"/> class.
        /// </summary>
        /// <param name="buffer">The raw byte-data which contains the <see cref="FlacFrameHeader"/>.</param>
        /// <param name="offset">The offset from which data will be read from the buffer</param>
        /// <param name="streamInfo">The stream-info-metadata-block of the flac stream which provides some basic information about the flac framestream. Can be set to null.</param>
        /// <param name="doCrc">A value which indicates whether the crc8 checksum of the <see cref="FlacFrameHeader"/> should be calculated.</param>
        public FlacFrameHeader(
            byte[] buffer,
            int offset,
            FlacMetadataStreamInfo streamInfo,
            bool doCrc)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            DoCrc = doCrc;
            StreamPosition = -1;
            ReadOnlySpan<byte> span = (uint)offset < (uint)buffer.Length
                ? buffer.AsSpan(offset)
                : ReadOnlySpan<byte>.Empty;
            HasError = !TryParseFromSpan(span, streamInfo, doCrc, out var bytesConsumed, logErrors: true);
            BytesConsumed = bytesConsumed;
        }

        internal bool TryParseFromStream(Stream stream, FlacMetadataStreamInfo? streamInfo, bool doCrc)
        {
            DoCrc = doCrc;
            StreamPosition = stream.Position;

            Span<byte> headerBuffer = stackalloc byte[FlacConstant.FrameHeaderSize];
            var totalRead = 0;
            while (totalRead < headerBuffer.Length)
            {
                var n = stream.Read(headerBuffer[totalRead..]);
                if (n == 0)
                    break;
                totalRead += n;
            }

            if (totalRead < headerBuffer.Length)
            {
                if (totalRead == 0)
                {
                    HasError = true;
                    BytesConsumed = 0;
                    return false;
                }

                // Allow parsing short trailing headers if >= 4 bytes, otherwise EOF
                var partialResult = TryParseFromSpan(headerBuffer[..totalRead], streamInfo, doCrc, out var partialConsumed, logErrors: true, StreamPosition);
                if (!partialResult)
                {
                    Log_EOFErrorHeader(_logger);
                    HasError = true;
                    BytesConsumed = 0;
                    return false;
                }

                stream.Position -= totalRead - partialConsumed;
                BytesConsumed = partialConsumed;
                HasError = false;
                return true;
            }

            var result = TryParseFromSpan(headerBuffer, streamInfo, doCrc, out var bytesConsumed, logErrors: true, StreamPosition);
            stream.Position -= totalRead - bytesConsumed;
            BytesConsumed = bytesConsumed;
            HasError = !result;
            return result;
        }

        // https://xiph.org/flac/format.html#FRAME_HEADER
        internal bool TryParseFromSpan(
            ReadOnlySpan<byte> headerBuffer,
            FlacMetadataStreamInfo? streamInfo,
            bool doCrc,
            out int bytesConsumed,
            bool logErrors = true,
            long streamPosition = -1)
        {
            _blocksizeHint = 0;
            _sampleRateHint = 0;
            StreamPosition = streamPosition;
            BlockSize = 0;
            SampleRate = 0;
            Channels = 0;
            BitsPerSample = 0;
            SampleNumber = 0;
            FrameNumber = 0;
            Crc8 = 0;
            DoCrc = doCrc;
            bytesConsumed = 0;

            if (headerBuffer.Length < 4)
                return false;

            if (headerBuffer[0] == 0xFF && headerBuffer[1] >> 1 == 0x7C) //sync bits
            {
                if ((headerBuffer[1] & 0x02) != 0)
                {
                    if (logErrors) Log_EOFErrorReservedBit1(_logger);
                    return false;
                }

                #region blocksize

                int val = headerBuffer[2] >> 4;
                var blocksize = -1;

                switch (val)
                {
                    case 0:
                        if (logErrors) Log_EOFErrorBlocksize0(_logger);
                        return false;
                    case 1:
                        blocksize = 192;
                        break;
                    case >= 2 and <= 5:
                        blocksize = 576 << (val - 2);
                        break;
                    case 6 or 7:
                        _blocksizeHint = val;
                        break;
                    default: //8-15 
                        blocksize = 256 << (val - 8);
                        break;
                }

                BlockSize = blocksize;

                #endregion blocksize

                #region samplerate

                val = headerBuffer[2] & 0x0F;
                var sampleRate = -1;

                switch (val)
                {
                    case 0 when streamInfo != null:
                        sampleRate = streamInfo.SampleRate;
                        break;
                    case 0:
                        if (logErrors) Log_EOFMissingSampleRate(_logger);
                        return false;
                    case >= 1 and <= 11:
                        sampleRate = FlacConstant.SampleRateTable[val];
                        break;
                    case >= 12 and <= 14:
                        _sampleRateHint = val;
                        break;
                    default:
                        if (logErrors) Log_EOFInvalidSampleRate(_logger, val);
                        return false;
                }

                SampleRate = sampleRate;

                #endregion samplerate

                #region channels

                val = headerBuffer[3] >> 4; //cc: unsigned
                int channels;
                if ((val & 8) != 0)
                {
                    channels = 2;
                    if ((val & 7) > 2)
                    {
                        if (logErrors) Log_EOFInvalidChannelAssignment(_logger);
                        return false;
                    }

                    ChannelAssignment = (ChannelAssignment)((val & 7) + 1);
                }
                else
                {
                    channels = val + 1;
                    ChannelAssignment = ChannelAssignment.Independent;
                }

                Channels = channels;

                #endregion channels

                #region bitspersample

                val = (headerBuffer[3] & 0x0E) >> 1;
                int bitsPerSample;
                switch (val)
                {
                    case 0 when streamInfo != null:
                        bitsPerSample = streamInfo.BitsPerSample;
                        break;
                    case 0:
                        if (logErrors) Log_MissingBitsPerSample(_logger);
                        return false;
                    case 3:
                    case >= 7:
                    case < 0:
                        if (logErrors) Log_InvalidBitsPerSampleIndex(_logger);
                        return false;
                    default:
                        bitsPerSample = FlacConstant.BitPerSampleTable[val];
                        break;
                }

                BitsPerSample = bitsPerSample;

                #endregion bitspersample

                if ((headerBuffer[3] & 0x01) != 0) // reserved bit -> 0
                {
                    if (logErrors) Log_InvalidErrorReservedBit1(_logger);
                    return false;
                }

                var reader = new Utils.SpanBitReader(headerBuffer);
                reader.ReadBits(32); //skip the first 4 bytes since they got already processed

                #region utf8

                //variable blocksize
                if ((headerBuffer[1] & 0x01) != 0 ||
                    (streamInfo != null && streamInfo.MinBlockSize != streamInfo.MaxBlockSize))
                {
                    if (reader.ReadUTF8_64(out var samplenumber) && samplenumber != ulong.MaxValue)
                    {
                        BlockingStrategy = BlockingStrategy.VariableBlockSize;
                        SampleNumber = (long)samplenumber;
                    }
                    else
                    {
                        if (logErrors) Log_InvalidUtf8SampleNumberCoding(_logger);
                        return false;
                    }
                }
                else //fixed blocksize
                {
                    if (reader.ReadUTF8_32(out var framenumber) && framenumber != uint.MaxValue)
                    {
                        BlockingStrategy = BlockingStrategy.FixedBlockSize;
                        FrameNumber = (int)framenumber;
                    }
                    else
                    {
                        if (logErrors) Log_InvalidUtf8FrameNumberCoding(_logger);
                        return false;
                    }
                }

                #endregion utf8

                #region read hints

                if (_blocksizeHint != 0)
                {
                    if (reader.Position + (_blocksizeHint == 7 ? 2 : 1) + 1 > headerBuffer.Length)
                        return false;
                    val = (int)reader.ReadBits(8);
                    if (_blocksizeHint == 7)
                    {
                        val = (val << 8) | (int)reader.ReadBits(8);
                    }

                    BlockSize = val + 1;
                }

                if (_sampleRateHint != 0)
                {
                    if (reader.Position + (_sampleRateHint != 12 ? 2 : 1) + 1 > headerBuffer.Length)
                        return false;
                    val = (int)reader.ReadBits(8);
                    if (_sampleRateHint != 12)
                    {
                        val = (val << 8) | (int)reader.ReadBits(8);
                    }

                    SampleRate = _sampleRateHint switch
                    {
                        12 => val * 1000,
                        13 => val,
                        _ => val * 10
                    };
                }

                #endregion read hints

                var crcPayloadLength = reader.Position;
                if (crcPayloadLength >= headerBuffer.Length)
                    return false;

                if (doCrc)
                {
                    var crc8 = Utils.CRC8.Instance.CalcCheckSum(headerBuffer[..crcPayloadLength]);
                    Crc8 = (byte)reader.ReadBits(8);
                    if (Crc8 != crc8)
                    {
                        if (logErrors) Log_ErrorCRC8(_logger);
                        return false;
                    }
                }
                else
                {
                    Crc8 = (byte)reader.ReadBits(8);
                }

                bytesConsumed = reader.Position;
                BytesConsumed = bytesConsumed;
                HasError = false;
                return true;
            }

            if (logErrors) Log_InvalidSyncBits(_logger);
            return false;
        }

        internal static bool TryParseCompact(
            ReadOnlySpan<byte> headerBuffer,
            FlacMetadataStreamInfo? streamInfo,
            out int blockSize,
            out int sampleRate,
            out int channels,
            out int bitsPerSample,
            out BlockingStrategy blockingStrategy,
            out long sampleOrFrameNumber,
            out int bytesConsumed)
        {
            blockSize = 0;
            sampleRate = 0;
            channels = 0;
            bitsPerSample = 0;
            blockingStrategy = default;
            sampleOrFrameNumber = 0;
            bytesConsumed = 0;

            if (headerBuffer.Length < 5)
                return false;
            if (headerBuffer[0] != 0xFF || (headerBuffer[1] >> 1) != 0x7C || (headerBuffer[1] & 0x02) != 0)
                return false;
            if ((headerBuffer[3] & 0x01) != 0)
                return false;

            var bsIndex = headerBuffer[2] >> 4;
            var blocksizeHint = 0;
            switch (bsIndex)
            {
                case 0:
                    return false;
                case 1:
                    blockSize = 192;
                    break;
                case >= 2 and <= 5:
                    blockSize = 576 << (bsIndex - 2);
                    break;
                case 6 or 7:
                    blocksizeHint = bsIndex;
                    break;
                default:
                    blockSize = 256 << (bsIndex - 8);
                    break;
            }

            var srIndex = headerBuffer[2] & 0x0F;
            var sampleRateHint = 0;
            switch (srIndex)
            {
                case 0 when streamInfo != null:
                    sampleRate = streamInfo.SampleRate;
                    break;
                case 0:
                    return false;
                case >= 1 and <= 11:
                    sampleRate = FlacConstant.SampleRateTable[srIndex];
                    break;
                case >= 12 and <= 14:
                    sampleRateHint = srIndex;
                    break;
                default:
                    return false;
            }

            var chVal = headerBuffer[3] >> 4;
            if ((chVal & 8) != 0)
            {
                channels = 2;
                if ((chVal & 7) > 2)
                    return false;
            }
            else
            {
                channels = chVal + 1;
            }

            var bpsIndex = (headerBuffer[3] & 0x0E) >> 1;
            switch (bpsIndex)
            {
                case 0 when streamInfo != null:
                    bitsPerSample = streamInfo.BitsPerSample;
                    break;
                case 0:
                case 3:
                case >= 7:
                    return false;
                default:
                    bitsPerSample = FlacConstant.BitPerSampleTable[bpsIndex];
                    break;
            }

            var reader = new Utils.SpanBitReader(headerBuffer);
            reader.ReadBits(32);

            if ((headerBuffer[1] & 0x01) != 0 ||
                (streamInfo != null && streamInfo.MinBlockSize != streamInfo.MaxBlockSize))
            {
                if (!reader.ReadUTF8_64(out var samplenumber) || samplenumber == ulong.MaxValue)
                    return false;
                blockingStrategy = BlockingStrategy.VariableBlockSize;
                sampleOrFrameNumber = (long)samplenumber;
            }
            else
            {
                if (!reader.ReadUTF8_32(out var framenumber) || framenumber == uint.MaxValue)
                    return false;
                blockingStrategy = BlockingStrategy.FixedBlockSize;
                sampleOrFrameNumber = framenumber;
            }

            if (blocksizeHint != 0)
            {
                if (reader.Position + (blocksizeHint == 7 ? 2 : 1) + 1 > headerBuffer.Length)
                    return false;
                var hintVal = (int)reader.ReadBits(8);
                if (blocksizeHint == 7)
                    hintVal = (hintVal << 8) | (int)reader.ReadBits(8);
                blockSize = hintVal + 1;
            }

            if (sampleRateHint != 0)
            {
                if (reader.Position + (sampleRateHint != 12 ? 2 : 1) + 1 > headerBuffer.Length)
                    return false;
                var hintVal = (int)reader.ReadBits(8);
                if (sampleRateHint != 12)
                    hintVal = (hintVal << 8) | (int)reader.ReadBits(8);
                sampleRate = sampleRateHint switch
                {
                    12 => hintVal * 1000,
                    13 => hintVal,
                    _ => hintVal * 10
                };
            }

            var crcPayloadLength = reader.Position;
            if (crcPayloadLength >= headerBuffer.Length)
                return false;

            var crc8 = Utils.CRC8.Instance.CalcCheckSum(headerBuffer[..crcPayloadLength]);
            var expectedCrc = (byte)reader.ReadBits(8);
            if (expectedCrc != crc8)
                return false;

            bytesConsumed = reader.Position;
            return true;
        }


        /// <summary>
        /// Indicates whether the format of the current <see cref="FlacFrameHeader"/> is equal to the format of another <see cref="FlacFrameHeader"/>.
        /// </summary>
        /// <param name="other">A <see cref="FlacFrameHeader"/> which provides the format to compare with the format of the current <see cref="FlacFrameHeader"/>.</param>
        /// <returns><c>true</c> if the format of the current <see cref="FlacFrameHeader"/> is equal to the format of the <paramref name="other"/> <see cref="FlacFrameHeader"/>.</returns>
        public bool IsFormatEqualTo(FlacFrameHeader other)
        {
            ArgumentNullException.ThrowIfNull(other);
            return (BitsPerSample == other.BitsPerSample &&
                    Channels == other.Channels &&
                    SampleRate == other.SampleRate);
        }
    }
}