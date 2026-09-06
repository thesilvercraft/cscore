using System.Buffers.Binary;

// ReSharper disable once CheckNamespace
namespace SilverCraft.CSCore.Codecs.FLAC
{
    /// <summary>
    ///     Represents the streaminfo metadata flac which provides general information about the flac stream.
    /// </summary>
    public class FlacMetadataStreamInfo : FlacMetadata
    {
        /// <summary>
        /// Initializes the properties of the <see cref="FlacMetadata"/> by reading them from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream which contains the metadata.</param>
        protected override void InitializeByStream(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            //http://flac.sourceforge.net/format.html#metadata_block_streaminfo
            //https://www.rfc-editor.org/rfc/rfc9639.html#name-streaminfo
            var initialBuffer = new byte[4];
            stream.ReadExactly(initialBuffer);
            try
            {
                MinBlockSize = BinaryPrimitives.ReadInt16BigEndian(initialBuffer);
                MaxBlockSize = BinaryPrimitives.ReadInt16BigEndian(initialBuffer.AsSpan()[2..]);
            }
            catch (IOException e)
            {
                throw new FlacException(e, FlacLayer.Metadata);
            }
            const int bytesToRead = (240 / 8) - 16;
            var buffer = new byte[bytesToRead];
            stream.ReadExactly(buffer);
            var bitreader = new FlacBitReader(buffer, 0);
            MinFrameSize = (int)bitreader.ReadBits(24);
            MaxFrameSize = (int)bitreader.ReadBits(24);
            SampleRate = (int)bitreader.ReadBits(20);
            Channels = 1 + (int)bitreader.ReadBits(3);
            BitsPerSample = 1 + (int)bitreader.ReadBits(5);
            TotalSamples = (long)bitreader.ReadBits64(36);
            var md5Bytes = new byte[16];
            stream.ReadExactly(md5Bytes);
            Md5 = Convert.ToHexString(md5Bytes);
        }

        /// <summary>
        /// Gets the type of the <see cref="FlacMetadata"/>.
        /// </summary>
        public override FlacMetaDataType MetaDataType => FlacMetaDataType.StreamInfo;

        /// <summary>
        ///  The minimum block size (in samples) used in the stream, excluding the last block.
        /// </summary>
        public short MinBlockSize { get; private set; }

        /// <summary>
        /// The maximum block size (in samples) used in the stream.
        /// </summary>
        public short MaxBlockSize { get; private set; }
        /// <summary>
        ///  The minimum frame size (in bytes) used in the stream. A value of 0 signifies that the value is not known.
        /// </summary>
        public int MinFrameSize { get; private set; }
        /// <summary>
        /// The maximum frame size (in bytes) used in the stream. A value of 0 signifies that the value is not known.
        /// </summary>
        public int MaxFrameSize { get; private set; }
  
        /// <summary>
        /// Sample rate in Hz.
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// Number of channels. FLAC supports from 1 to 8 channels.
        /// </summary>
        public int Channels { get; private set; }

        /// <summary>
        /// Bits per sample. FLAC supports from 4 to 32 bits per sample.
        /// </summary>
        public int BitsPerSample { get; private set; }

        /// <summary>
        ///  Total number of interchannel samples in the stream. A value of 0 here means the number of total samples is unknown.
        /// </summary>
        public long TotalSamples { get; private set; }

        /// <summary>
        ///  MD5 checksum of the unencoded audio data as HEX string. A value of <code>"0000000000000000000000000000"</code> signifies that the value is not known.
        /// </summary>
        public string Md5 { get; private set; }
    }
}