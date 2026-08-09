using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SilverCraft.CSCore.Streams.SampleConverter
{
    /// <summary>
    /// Converts a 8-bit PCM <see cref="IWaveSource"/> to a <see cref="ISampleSource"/>.
    /// </summary>
    public class Pcm8BitToSample : WaveToSampleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pcm8BitToSample"/> class.
        /// </summary>
        /// <param name="source">The underlying 8-bit POCM <see cref="IWaveSource"/> instance which has to get converted to a <see cref="ISampleSource"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentException">The format of the <paramref name="source"/> is not 8-bit PCM.</exception>
        public Pcm8BitToSample(IWaveSource source)
            : base(source)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (!source.WaveFormat.IsPCM() || source.WaveFormat.BitsPerSample != 8)
                throw new InvalidOperationException("Invalid format. Format has to 8 bit Pcm.");
        }

        /// <summary>
        ///     Reads a sequence of samples from the <see cref="Pcm8BitToSample" /> and advances the position within the stream by the
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
            var bytesToRead = count;
            Buffer = Buffer.CheckBuffer(bytesToRead);
            var bytesRead = Source.Read(Buffer, 0, bytesToRead);

            ReadOnlySpan<byte> rawBytes = Buffer.AsSpan(0, bytesRead);
            var targetSamples = buffer.AsSpan(offset, bytesRead);

            var i = 0;
            ref var srcRef = ref MemoryMarshal.GetReference(rawBytes);
            if (Vector256.IsHardwareAccelerated && Vector128.IsHardwareAccelerated && rawBytes.Length >= 32)
            {
                var scale = Vector128.Create(1.0f / 128f);
                var offsetVector = Vector128.Create(1.0f);

                for (; i <= rawBytes.Length - 32; i += 32)
                {
                    var byteVec = Vector256.LoadUnsafe(ref srcRef, (nuint)i);
                    var (lower16, upper16) = Vector256.Widen(byteVec);
                    var (intVec0, intVec1) = Vector256.Widen(lower16);
                    var (intVec2, intVec3) = Vector256.Widen(upper16);

                    ConvertAndStore(intVec0.GetLower(), targetSamples, i);
                    ConvertAndStore(intVec0.GetUpper(), targetSamples, i + 4);
                    ConvertAndStore(intVec1.GetLower(), targetSamples, i + 8);
                    ConvertAndStore(intVec1.GetUpper(), targetSamples, i + 12);
                    ConvertAndStore(intVec2.GetLower(), targetSamples, i + 16);
                    ConvertAndStore(intVec2.GetUpper(), targetSamples, i + 20);
                    ConvertAndStore(intVec3.GetLower(), targetSamples, i + 24);
                    ConvertAndStore(intVec3.GetUpper(), targetSamples, i + 28);
                }

                void ConvertAndStore(Vector128<uint> intVec, Span<float> dest, int index)
                {
                    var floatVec = Vector128.ConvertToSingle(intVec);
                    var result = Fma.IsSupported
                        ? Fma.MultiplySubtract(floatVec, scale, offsetVector)
                        : (floatVec * scale) - offsetVector;

                    result.StoreUnsafe(ref dest[index]);
                }
            }

            for (; i < rawBytes.Length; i++)
            {
                targetSamples[i] = (rawBytes[i] / 128f) - 1.0f;
            }

            return rawBytes.Length;
        }
    }
}