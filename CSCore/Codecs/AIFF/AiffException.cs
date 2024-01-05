using System;

namespace CSCore.Codecs.AIFF
{
    /// <summary>
    ///     Represents errors that occur when decoding or encoding Aiff-streams/files.
    /// </summary>
    [Serializable]
    public class AiffException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AiffException" /> class.
        /// </summary>
        public AiffException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AiffException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AiffException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AiffException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The <see cref="Exception" /> that caused the <see cref="AiffException" />.</param>
        public AiffException(string message, Exception inner) : base(message, inner)
        {
        }
        
    }
}