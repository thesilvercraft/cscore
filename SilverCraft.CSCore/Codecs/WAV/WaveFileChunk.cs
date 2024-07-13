using System;
using System.IO;

namespace SilverCraft.CSCore.Codecs.WAV
{
    /// <summary>
    ///     Represents a wave file chunk. For more information see
    ///     <see href="http://www.sonicspot.com/guide/wavefiles.html#wavefilechunks" />.
    /// </summary>
    public class WaveFileChunk
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WaveFileChunk" /> class.
        /// </summary>
        /// <param name="stream"><see cref="Stream" /> which contains the wave file chunk.</param>
        public WaveFileChunk(Stream stream)
            : this(new BinaryReader(stream))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WaveFileChunk" /> class.
        /// </summary>
        /// <param name="reader"><see cref="BinaryReader" /> which should be used to read the wave file chunk.</param>
        public WaveFileChunk(BinaryReader reader)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ChunkID = reader.ReadInt32();
            ChunkDataSize = reader.ReadUInt32();
        }

        /// <summary>
        ///     Gets the unique ID of the Chunk. Each type of chunk has its own id.
        /// </summary>
        public int ChunkID { get; private set; }

        /// <summary>
        ///     Gets the data size of the chunk.
        /// </summary>
        public long ChunkDataSize { get; private set; }

        /// <summary>
        ///     Parses the <paramref name="stream" /> and returns a <see cref="WaveFileChunk" />. Note that the position of the
        ///     stream has to point to a wave file chunk.
        /// </summary>
        /// <param name="stream"><see cref="Stream" /> which points to a wave file chunk.</param>
        /// <returns>
        ///     Instance of the <see cref="WaveFileChunk" /> class or any derived classes. It the stream does not point to a
        ///     wave file chunk the instance of the <see cref="WaveFileChunk" /> which gets return will be invalid.
        /// </returns>
        public static WaveFileChunk FromStream(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (stream.CanRead == false)
                throw new ArgumentException("stream is not readable");

            var reader = new BinaryReader(stream);
            var id = reader.ReadInt32();
            stream.Position -= 4;

            return id switch
            {
                FmtChunk.FmtChunkID => new FmtChunk(reader),
                DataChunk.DataChunkID => new DataChunk(reader),
                _ => new WaveFileChunk(reader)
            };
        }
    }
}