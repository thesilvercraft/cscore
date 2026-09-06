using System.Buffers.Binary;

// ReSharper disable once CheckNamespace
namespace SilverCraft.CSCore.Codecs.FLAC
{
    /// <summary>
    /// Represents a flac seektable.
    /// </summary>
    public class FlacMetadataSeekTable : FlacMetadata
    {
        private FlacSeekPoint[] _seekPoints;

        /// <summary>
        /// Gets the number of entries, the seektable offers.
        /// </summary>
        public int EntryCount { get; private set; }

        /// <summary>
        /// Gets the seek points.
        /// </summary>
        public FlacSeekPoint[] SeekPoints => _seekPoints;

        /// <summary>
        /// Gets the <see cref="FlacSeekPoint"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <value>
        /// The <see cref="FlacSeekPoint"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="FlacSeekPoint"/> at the specified <paramref name="index"/>.</returns>
        public FlacSeekPoint this[int index] => _seekPoints[index];

        /// <summary>
        /// Initializes the properties of the <see cref="FlacMetadata"/> by reading them from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream which contains the metadata.</param>
        protected override void InitializeByStream(Stream stream)
        {
            var entryCount = Length / 18;
            EntryCount = entryCount;
            _seekPoints = new FlacSeekPoint[entryCount];
            Span<byte> buffer = stackalloc byte[18];
            try
            {
                for (var i = 0; i < entryCount; i++)
                {
                    stream.ReadExactly(buffer);

                    long sampleNumber = BinaryPrimitives.ReadInt64BigEndian(buffer[..8]);
                    long offset       = BinaryPrimitives.ReadInt64BigEndian(buffer[8..16]);
                    short frameSize   = BinaryPrimitives.ReadInt16BigEndian(buffer[16..18]);

                    _seekPoints[i] = new FlacSeekPoint(sampleNumber, offset, frameSize);
                }
            }
            catch (IOException e)
            {
                throw new FlacException(e, FlacLayer.Metadata);
            }
        }

        /// <summary>
        /// Gets the type of the <see cref="FlacMetadata"/>.
        /// </summary>
        public override FlacMetaDataType MetaDataType => FlacMetaDataType.Seektable;
    }
}