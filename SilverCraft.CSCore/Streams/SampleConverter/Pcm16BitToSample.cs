using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SilverCraft.CSCore.Streams.SampleConverter
{
    /// <summary>
    /// Converts a 16-bit PCM <see cref="IWaveSource"/> to a <see cref="ISampleSource"/>.
    /// </summary>
    public class Pcm16BitToSample : WaveToSampleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pcm16BitToSample"/> class.
        /// </summary>
        /// <param name="source">The underlying 16-bit POCM <see cref="IWaveSource"/> instance which has to get converted to a <see cref="ISampleSource"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentException">The format of the <paramref name="source"/> is not 16-bit PCM.</exception>
        public Pcm16BitToSample(IWaveSource source)
            : base(source)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (!source.WaveFormat.IsPCM() || source.WaveFormat.BitsPerSample != 16)
                throw new InvalidOperationException("Invalid format. Format has to 16 bit Pcm.");
        }

        /// <summary>
        ///     Reads a sequence of samples from the <see cref="Pcm16BitToSample" /> and advances the position within the stream by the
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
            var bytesToRead = count * sizeof(short);
            Buffer = Buffer.CheckBuffer(bytesToRead);
            var bytesRead = Source.Read(Buffer, 0, bytesToRead);

            ReadOnlySpan<byte> rawBytes = Buffer.AsSpan(0, bytesRead);
            var sourceSamples = MemoryMarshal.Cast<byte, short>(rawBytes);
            var targetSamples = buffer.AsSpan(offset, sourceSamples.Length);

            var i = 0;

            if (Vector256.IsHardwareAccelerated && Vector128.IsHardwareAccelerated && sourceSamples.Length >= 16)
            {
                var scale = Vector128.Create(1.0f / 32768f);

                for (; i <= sourceSamples.Length - 16; i += 16)
                {
                    var shortVec =
                        Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(sourceSamples.Slice(i)));

                    var (lowerInts, upperInts) = Vector256.Widen(shortVec);

                    ConvertAndStore(lowerInts.GetLower(), targetSamples, i);
                    ConvertAndStore(lowerInts.GetUpper(), targetSamples, i + 4);
                    ConvertAndStore(upperInts.GetLower(), targetSamples, i + 8);
                    ConvertAndStore(upperInts.GetUpper(), targetSamples, i + 12);
                }

                void ConvertAndStore(Vector128<int> intVec, Span<float> dest, int index)
                {
                    var floatVec = Vector128.ConvertToSingle(intVec);
                    var result = floatVec * scale;
                    result.StoreUnsafe(ref dest[index]);
                }
            }

            for (; i < sourceSamples.Length; i++)
            {
                targetSamples[i] = sourceSamples[i] / 32768f;
            }

            return sourceSamples.Length;
        }
    }
}