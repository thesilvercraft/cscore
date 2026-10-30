using System.Text;

namespace SilverCraft.CSCore.Codecs.WAV
{
    /// <summary>
    ///     Represents the <see cref="FmtChunk" /> of a wave file.
    /// </summary>
    public class FmtChunk : WaveFileChunk
    {
        /// <summary>
        ///     Chunk ID of the <see cref="FmtChunk" />.
        /// </summary>
        public const int FmtChunkID = 0x20746D66;

        private readonly WaveFormat _waveFormat;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FmtChunk" /> class.
        /// </summary>
        /// <param name="stream"><see cref="Stream" /> which contains the fmt chunk.</param>
        public FmtChunk(Stream stream)
            : base(stream) // Chain to the base stream constructor
        {
            // The base class read the ChunkID and Size, now we safely create a scoped reader for the rest
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            if (ChunkID != FmtChunkID) return; //"fmt "

            _waveFormat = ParseWaveFormat(reader, ChunkDataSize);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FmtChunk" /> class.
        /// </summary>
        /// <param name="reader"><see cref="BinaryReader" /> which should be used to read the fmt chunk.</param>
        public FmtChunk(BinaryReader reader)
            : base(reader, disposeReader: false) // Ensure the base class leaves the reader open for us
        {
            ArgumentNullException.ThrowIfNull(reader);

            if (ChunkID != FmtChunkID) return; //"fmt "

            _waveFormat = ParseWaveFormat(reader, ChunkDataSize);
        }

        private static WaveFormat ParseWaveFormat(BinaryReader reader, long chunkDataSize)
        {
            var encoding = (AudioEncoding)reader.ReadInt16();
            int channels = reader.ReadInt16();
            var sampleRate = reader.ReadInt32();
            var avgBps = reader.ReadInt32();
            int blockAlign = reader.ReadInt16();
            int bitsPerSample = reader.ReadInt16();

            var extraSize = 0;
            if (chunkDataSize > 16)
            {
                extraSize = reader.ReadInt16();
                if (extraSize != chunkDataSize - 18)
                    extraSize = (int)(chunkDataSize - 18);

                for (var i = (int)(chunkDataSize - 16); i > 0; i--)
                {
                    reader.ReadByte();
                }

                reader.BaseStream.Position -= 2;
            }

            return new WaveFormat(sampleRate, (short)bitsPerSample, (short)channels, encoding, extraSize);
        }

        /// <summary>
        ///     Gets the <see cref="WaveFormat" /> specified by the <see cref="FmtChunk" />.
        /// </summary>
        public WaveFormat WaveFormat => _waveFormat;
    }
}