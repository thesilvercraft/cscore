using System;
using System.Runtime.Serialization;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    /// <summary>
    /// FLAC Exception.
    /// </summary>
    [Serializable]
    public class FlacException : Exception
    {
        /// <summary>
        /// Gets the layer of the flac stream the exception got thrown.
        /// </summary>
        /// <remarks>Used for debugging purposes.</remarks>
        public FlacLayer Layer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacException"/> class.
        /// </summary>
        /// <param name="message">A message which describes the error.</param>
        /// <param name="layer">The layer of the flac stream the exception got thrown.</param>
        public FlacException(string message, FlacLayer layer)
            : base(message)
        {
            Layer = layer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacException"/> class.
        /// </summary>
        /// <param name="innerException">The InnerException which caused the error.</param>
        /// <param name="layer">The layer.The layer of the flac stream the exception got thrown.</param>
        public FlacException(Exception innerException, FlacLayer layer)
            : base("See InnerException for more details.", innerException)
        {
            Layer = layer;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FlacException" /> class from serialization data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> object that holds the serialized object data.</param>
        /// <param name="context">
        ///     The StreamingContext object that supplies the contextual information about the source or
        ///     destination.
        /// </param>
        protected FlacException(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);

            Layer = (FlacLayer) info.GetValue("Layer", typeof (FlacLayer));
        }
    }
}