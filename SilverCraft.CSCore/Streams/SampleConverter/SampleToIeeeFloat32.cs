using System;
using System.Runtime.InteropServices;

namespace SilverCraft.CSCore.Streams.SampleConverter
{
    /// <summary>
    /// Converts a <see cref="ISampleSource"/> to a 32-bit IeeeFloat <see cref="IWaveSource"/>.
    /// </summary>
    public class SampleToIeeeFloat32 : SampleToWaveBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToIeeeFloat32"/> class.
        /// </summary>
        /// <param name="source">The underlying <see cref="ISampleSource"/> which has to get converted to a 32-bit IeeeFloat <see cref="IWaveSource"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public SampleToIeeeFloat32(ISampleSource source)
            : base(source, 32, AudioEncoding.IeeeFloat)
        {
            ArgumentNullException.ThrowIfNull(source);
        }

        /// <summary>
        ///     Reads a sequence of bytes from the <see cref="SampleToIeeeFloat32" /> and advances the position within the stream by the
        ///     number of bytes read.
        /// </summary>
        /// <param name="buffer">
        ///     An array of bytes. When this method returns, the <paramref name="buffer" /> contains the specified
        ///     byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
        ///     <paramref name="count" /> - 1) replaced by the bytes read from the current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based byte offset in the <paramref name="buffer" /> at which to begin storing the data
        ///     read from the current stream.
        /// </param>
        /// <param name="count">The maximum number of bytes to read from the current source.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var sampleCount = count >> 2; 
            if (sampleCount <= 0) return 0;
            Buffer = Buffer.CheckBuffer(sampleCount);
            var samplesRead = Source.Read(Buffer, 0, sampleCount);
            if (samplesRead <= 0) return 0;
            var bytesRead = samplesRead << 2;
            var floatSpan = new ReadOnlySpan<float>(Buffer, 0, samplesRead);
            var sourceBytes = MemoryMarshal.AsBytes(floatSpan);
            var destBytes = new Span<byte>(buffer, offset, bytesRead);
            sourceBytes.CopyTo(destBytes);
            return bytesRead;
        }
    }
}