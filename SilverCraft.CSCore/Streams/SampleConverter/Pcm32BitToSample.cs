using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SilverCraft.CSCore.Streams.SampleConverter
{
    /// <summary>
    /// Converts a 32-bit PCM <see cref="IWaveSource"/> to a <see cref="ISampleSource"/>.
    /// </summary>
    public class Pcm32BitToSample : WaveToSampleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pcm32BitToSample"/> class.
        /// </summary>
        /// <param name="source">The underlying 32-bit POCM <see cref="IWaveSource"/> instance which has to get converted to a <see cref="ISampleSource"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentException">The format of the <paramref name="source"/> is not 32-bit PCM.</exception>
        public Pcm32BitToSample(IWaveSource source)
            : base(source)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (!source.WaveFormat.IsPCM() || source.WaveFormat.BitsPerSample != 32)
                throw new InvalidOperationException("Invalid format. Format has to 32 bit Pcm.");
        }

        /// <summary>
        ///     Reads a sequence of samples from the <see cref="Pcm32BitToSample" /> and advances the position within the stream by the
        ///     number of samples read.
        /// </summary>
        /// <param name="buffer">
        ///     An array of floats. When this method returns, the <paramref name="buffer" /> contains the specified
        ///     float array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
        ///     <paramref name="count" /> - 1) replaced by the floats read from the current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based offset in the <paramref name="buffer" /> at which to begin storing the data
        ///     read from the current stream.
        /// </param>
        /// <param name="count">The maximum number of samples to read from the current source.</param>
        /// <returns>The total number of samples read into the buffer.</returns>
        public override int Read(float[] buffer, int offset, int count)
        {
            var bytesToRead = count * 4;
            Buffer = Buffer.CheckBuffer(bytesToRead);
            var bytesRead = Source.Read(Buffer, 0, bytesToRead);

            ReadOnlySpan<byte> rawBytes = Buffer.AsSpan(0, bytesRead);
            var sourceSamples = MemoryMarshal.Cast<byte, int>(rawBytes);
            var targetSamples = buffer.AsSpan(offset, sourceSamples.Length);

            var i = 0;
            if (Vector256.IsHardwareAccelerated)
            {
                var scaleVector = Vector256.Create(1.0f / 2147483648f);
                var vectorSize = Vector256<int>.Count;
                var vectorLoopEnd = sourceSamples.Length - (sourceSamples.Length % vectorSize);

                for (; i < vectorLoopEnd; i += vectorSize)
                {
                    var intVec = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(sourceSamples), (uint)i);
                    var floatVec = Vector256.ConvertToSingle(intVec);
                    var resultVec = floatVec * scaleVector;
                    resultVec.StoreUnsafe(ref MemoryMarshal.GetReference(targetSamples), (uint)i);
                }
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                var scaleVector = Vector128.Create(1.0f / 2147483648f);
                var vectorSize = Vector128<int>.Count;
                var vectorLoopEnd = sourceSamples.Length - (sourceSamples.Length % vectorSize);

                for (; i < vectorLoopEnd; i += vectorSize)
                {
                    var intVec = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(sourceSamples), (uint)i);
                    var floatVec = Vector128.ConvertToSingle(intVec);
                    var resultVec = floatVec * scaleVector;
                    resultVec.StoreUnsafe(ref MemoryMarshal.GetReference(targetSamples), (uint)i);
                }
            }
            for (; i < sourceSamples.Length; i++)
            {
                targetSamples[i] = sourceSamples[i] / 2147483648f;
            }

            return sourceSamples.Length;
        }
    }
}
