using System;
using System.Diagnostics;
using System.IO;
using Serilog;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    /// <summary>
    /// Represents the header of a <see cref="FlacFrame"/>.
    /// </summary>
    public sealed class FlacFrameHeader
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
        /// Gets the stream position.
        /// </summary>
        /// <value>
        /// The stream position.
        /// </value>
        public long StreamPosition { get; private set; }

        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacFrameHeader"/> class.
        /// </summary>
        /// <param name="stream">The underlying stream which contains the <see cref="FlacFrameHeader"/>.</param>
        /// <param name="streamInfo">The stream-info-metadata-block of the flac stream which provides some basic information about the flac framestream. Can be set to null.</param>
        /// <param name="doCrc">A value which indicates whether the crc8 checksum of the <see cref="FlacFrameHeader"/> should be calculated.</param>
        public FlacFrameHeader(Stream stream, FlacMetadataStreamInfo? streamInfo = null, bool doCrc = true)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (stream.CanRead == false) throw new ArgumentException("stream is not readable");
            if (!stream.CanSeek)
                throw new ArgumentException("stream must be readable and seekable", nameof(stream));

            //streamInfo can be null

            DoCrc = doCrc;
            StreamPosition = stream.Position;
            _logger = LogLocation.GetLogger(typeof(FlacFrameHeader));
            HasError = !ParseHeader(stream,0, streamInfo);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacFrameHeader"/> class.
        /// </summary>
        /// <param name="buffer">The raw byte-data which contains the <see cref="FlacFrameHeader"/>.</param>
        /// <param name="streamInfo">The stream-info-metadata-block of the flac stream which provides some basic information about the flac framestream. Can be set to null.</param>
        /// <param name="doCrc">A value which indicates whether the crc8 checksum of the <see cref="FlacFrameHeader"/> should be calculated.</param>
        public FlacFrameHeader(
            byte[] buffer,
            int offset,
            FlacMetadataStreamInfo streamInfo,
            bool doCrc)
        {
            DoCrc = doCrc;
            StreamPosition = -1;
            _logger = LogLocation.GetLogger(typeof(FlacFrameHeader));

            HasError = !ParseHeader(buffer, offset, streamInfo, out var _);
        }

        private bool ParseHeader(Stream stream, int offset, FlacMetadataStreamInfo? streamInfo)
        {
            var headerBuffer = new byte[FlacConstant.FrameHeaderSize];

            try
            {
                stream.ReadExactly(headerBuffer);
            }
            catch (EndOfStreamException)
            {
                _logger?.Error("Not able to read Flac header - EOF?");
                return false;
            }

            var result = ParseHeader(headerBuffer,offset, streamInfo, out var bytesConsumed);

            stream.Position -= headerBuffer.Length - bytesConsumed;

            return result;
        }

       //https://xiph.org/flac/format.html#FRAME_HEADER
        private  bool ParseHeader( byte[] headerBuffer, int offset,
            FlacMetadataStreamInfo? streamInfo,
            out int bytesConsumed)
        {
            int val;
            bytesConsumed = 0;
            if (offset < 0 || offset >= headerBuffer.Length)
                return false;

            if (headerBuffer.Length - offset < 4)
                return false;
            if (headerBuffer[offset ] == 0xFF && headerBuffer[offset +1] >> 1 == 0x7C) //sync bits
            {
                if ((headerBuffer[offset +1] & 0x02) != 0)
                {
                     _logger?.Debug("Invalid FlacFrame. Reservedbit_0 is 1");
                    return false;
                }

                var reader = new FlacBitReader(
                    headerBuffer, offset);

                #region blocksize

                //blocksize
                val = headerBuffer[offset +2] >> 4;
                var blocksize = -1;

                switch (val)
                {
                    case 0:
                        _logger?.Debug("Invalid Blocksize value: 0");
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

                //samplerate
                val = headerBuffer[offset + 2] & 0x0F;
                var sampleRate = -1;

                switch (val)
                {
                    case 0 when streamInfo != null:
                        sampleRate = streamInfo.SampleRate;
                        break;
                    case 0:
                        _logger?.Debug("Missing Samplerate. Samplerate Index = 0 && streamInfoMetaData == null.");
                        return false;
                    case >= 1 and <= 11:
                        sampleRate = FlacConstant.SampleRateTable[val];
                        break;
                    case >= 12 and <= 14:
                        _sampleRateHint = val;
                        break;
                    default:
                        _logger?.Debug("Invalid SampleRate value: {Value}", val);
                        return false;
                }

                SampleRate = sampleRate;

                #endregion samplerate

                #region channels

                val = headerBuffer[offset +3] >> 4; //cc: unsigned
                int channels;
                if ((val & 8) != 0)
                {
                    channels = 2;
                    if ((val & 7) > 2 || (val & 7) < 0)
                    {
                         _logger?.Debug("Invalid ChannelAssignment");
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

                val = (headerBuffer[offset + 3] & 0x0E) >> 1;
                int bitsPerSample;
                switch (val)
                {
                    case 0 when streamInfo != null:
                        bitsPerSample = streamInfo.BitsPerSample;
                        break;
                    case 0:
                         _logger?.Debug("Missing BitsPerSample. Index = 0 && streamInfoMetaData == null.");
                        return false;
                    case 3:
                    case >= 7:
                    case < 0:
                        _logger?.Debug("Invalid BitsPerSampleIndex");

                        return false;
                    default:
                        bitsPerSample = FlacConstant.BitPerSampleTable[val];
                        break;
                }

                BitsPerSample = bitsPerSample;

                #endregion bitspersample

                if ((headerBuffer[offset + 3] & 0x01) != 0) // reserved bit -> 0
                {
                    _logger?.Debug("Invalid FlacFrame. Reservedbit_1 is 1");

                    return false;
                }

                reader.ReadBits(32); //skip the first 4 bytes since they got already processed

                //BYTE 4

                #region utf8

                //variable blocksize
                if ((headerBuffer[offset +1] & 0x01) != 0 ||
                    (streamInfo != null && streamInfo.MinBlockSize != streamInfo.MaxBlockSize))
                {
                    ulong samplenumber;
                    if (reader.ReadUTF8_64(out samplenumber) && samplenumber != ulong.MaxValue)
                    {
                        BlockingStrategy = BlockingStrategy.VariableBlockSize;
                        SampleNumber = (long)samplenumber;
                    }
                    else
                    {
                        _logger?.Debug("Invalid UTF8 Samplenumber coding");
                        return false;
                    }
                }
                else //fixed blocksize
                {
                    uint framenumber;

                    if (reader.ReadUTF8_32(out framenumber) && framenumber != uint.MaxValue)
                    {
                        BlockingStrategy = BlockingStrategy.FixedBlockSize;
                        FrameNumber = (int)framenumber;
                    }
                    else
                    {
                        _logger?.Debug("Invalid UTF8 Framenumber coding");
                        return false;
                    }
                }

                #endregion utf8

                #region read hints

                //blocksize am ende des frameheaders
                if (_blocksizeHint != 0)
                {
                    val = (int)reader.ReadBits(8);
                    if (_blocksizeHint == 7)
                    {
                        val = (val << 8) | (int)reader.ReadBits(8);
                    }

                    BlockSize = val + 1;
                }

                //samplerate
                if (_sampleRateHint != 0)
                {
                    val = (int)reader.ReadBits(8);
                    if (_sampleRateHint != 12)
                    {
                        val = (val << 8) | (int)reader.ReadBits(8);
                    }

                    if (_sampleRateHint == 12)
                        SampleRate = val * 1000;
                    else if (_sampleRateHint == 13)
                        SampleRate = val;
                    else
                        SampleRate = val * 10;
                }

                #endregion read hints

                if (DoCrc)
                {
                    var crc8 = Utils.CRC8.Instance.CalcCheckSum(reader.Buffer, 0, reader.Position);
                    Crc8 = (byte)reader.ReadBits(8);
                    if (Crc8 != crc8)
                    {
                        _logger?.Debug("CRC8 missmatch");
                        return false;
                    }
                }
                else
                {
                    Crc8 = (byte)reader.ReadBits(8);
                }

                bytesConsumed = reader.Position;
                return true;
            }

            _logger?.Debug("Invalid Syncbits");

            return false;
        }


        /// <summary>
        /// Indicates whether the format of the current <see cref="FlacFrameHeader"/> is equal to the format of another <see cref="FlacFrameHeader"/>.
        /// </summary>
        /// <param name="other">A <see cref="FlacFrameHeader"/> which provides the format to compare with the format of the current <see cref="FlacFrameHeader"/>.</param>
        /// <returns><c>true</c> if the format of the current <see cref="FlacFrameHeader"/> is equal to the format of the <paramref name="other"/> <see cref="FlacFrameHeader"/>.</returns>
        public bool IsFormatEqualTo(FlacFrameHeader other)
        {
            return (BitsPerSample == other.BitsPerSample &&
                    Channels == other.Channels &&
                    SampleRate == other.SampleRate);
        }
    }
}