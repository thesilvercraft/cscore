using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SilverCraft.CSCore.Codecs.AIFF
{
    /// <summary>
    ///     Provides all <see cref="AiffChunk" />s of a aiff stream.
    /// </summary>
    public class AiffChunkContainer : AiffChunk
    {
        private readonly List<AiffChunk> _chunks = [];

        /// <summary>
        ///     Initializes a new instance of the <see cref="AiffChunkContainer" /> class.
        /// </summary>
        /// <param name="binaryReader">The binary reader which provides can be used to decode the chunk.</param>
        /// <exception cref="AiffException">
        ///     FORM header not found.
        ///     or
        ///     Invalid Formtype.
        /// </exception>
        public AiffChunkContainer(BinaryReader binaryReader) : base(binaryReader, new string(binaryReader.ReadChars(4)))
        {
            if (ChunkId != "FORM")
                throw new AiffException("FORM header not found. No Aiff file.");

            FormType = new string(binaryReader.ReadChars(4));
            if (FormType != "AIFF" && FormType != "AIFC")
                throw new AiffException("Invalid Formtype. Formtype is: " + FormType);

            AiffChunk chunk;
            while ((chunk = GetNextChunk(binaryReader)) != null)
            {
                chunk.SkipChunk();
                _chunks.Add(chunk);
            }

            if (FormType == "AIFC" && !_chunks.Any(x => x is FormatVersionChunk))
                throw new AiffException(string.Format("Invalid {0}-file. FormatVersionChunk not found.", FormType));
        }

        //AIFF, AIFC = AIFF-C
        /// <summary>
        ///     Gets the form type.
        /// </summary>
        /// <value>
        ///     Either 'AIFF' or 'AIFC'.
        /// </value>
        public string FormType { get; private set; }

        /// <summary>
        ///     Gets all found <see cref="AiffChunk" /> of the <see cref="AiffChunkContainer" />.
        /// </summary>
        public ReadOnlyCollection<AiffChunk> Chunks => _chunks.AsReadOnly();

        private AiffChunk GetNextChunk(BinaryReader binaryReader)
        {
            try
            {
                var chars = binaryReader.ReadChars(4);
                if (chars.Length != 4)
                    throw new EndOfStreamException();

                var chunkId = new string(chars);
                return chunkId switch
                {
                    "COMM" => new CommonChunk(binaryReader),
                    "SSND" => new SoundDataChunk(binaryReader),
                    "FVER" => new FormatVersionChunk(binaryReader),
                    "\0\0\0\0" => null,
                    _ => new AiffChunk(binaryReader, chunkId),
                };
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Seeks to the end of the chunk.
        /// </summary>
        /// <remarks>
        ///     Can be used to make sure that the underlying <see cref="Stream" />/<see cref="BinaryReader" /> points to
        ///     the next <see cref="AiffChunk" />.
        /// </remarks>
        public override void SkipChunk()
        {
            Reader.Skip(DataSize - 4);
        }
    }
}