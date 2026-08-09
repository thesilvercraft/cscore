using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SilverCraft.CSCore.Streams.SampleConverter
{
    /// <summary>
    /// Converts a <see cref="ISampleSource"/> to a 16-bit PCM <see cref="IWaveSource"/>.
    /// </summary>
    public class SampleToPcm16 : SampleToWaveBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToPcm16"/> class.
        /// </summary>
        /// <param name="source">The underlying <see cref="ISampleSource"/> which has to get converted to a 16-bit PCM <see cref="IWaveSource"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public SampleToPcm16(ISampleSource source)
            : base(source, 16, AudioEncoding.Pcm)
        {
            ArgumentNullException.ThrowIfNull(source);
        }

        /// <summary>
        ///     Reads a sequence of bytes from the <see cref="SampleToPcm16" /> and advances the position within the stream by the
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
            ArgumentNullException.ThrowIfNull(buffer);

            var samplesToRead = count / 2;
            if (samplesToRead <= 0) return 0;

            Buffer = Buffer.CheckBuffer(samplesToRead);
            var readSamples = Source.Read(Buffer, 0, samplesToRead);
            if (readSamples <= 0) return 0;

            var readBytes = readSamples * 2;
            ReadOnlySpan<float> sourceSpan = Buffer.AsSpan(0, readSamples);
            var targetSpan = MemoryMarshal.Cast<byte, short>(buffer.AsSpan(offset, readBytes));

            var i = 0;
            ref var srcRef = ref MemoryMarshal.GetReference(sourceSpan);
            ref var dstRef = ref MemoryMarshal.GetReference(targetSpan);

            if (Vector256.IsHardwareAccelerated && sourceSpan.Length >= 16)
            {
                var scale = Vector256.Create(32768.0f);
                var maxVal = Vector256.Create((float)short.MaxValue);
                var minVal = Vector256.Create((float)short.MinValue);

                var simdLimit = sourceSpan.Length & ~15;

                for (; i < simdLimit; i += 16)
                {
                    var f1 = Vector256.LoadUnsafe(ref Unsafe.Add(ref srcRef, i));
                    var f2 = Vector256.LoadUnsafe(ref srcRef, (nuint)(i + 8));

                    var clamped1 = Vector256.Min(Vector256.Max(Vector256.Multiply(f1, scale), minVal), maxVal);
                    var clamped2 = Vector256.Min(Vector256.Max(Vector256.Multiply(f2, scale), minVal), maxVal);

                    var int1 = Vector256.ConvertToInt32(clamped1);
                    var int2 = Vector256.ConvertToInt32(clamped2);

                    var shorts256 = Vector256.Narrow(int1, int2);

                    shorts256.StoreUnsafe(ref Unsafe.Add(ref dstRef, i));
                }
            }

            if (Vector128.IsHardwareAccelerated && (sourceSpan.Length - i) >= 8)
            {
                var scale = Vector128.Create(32768.0f);
                var maxVal = Vector128.Create((float)short.MaxValue);
                var minVal = Vector128.Create((float)short.MinValue);

                var simdLimit = sourceSpan.Length & ~7;

                for (; i < simdLimit; i += 8)
                {
                    var f1 = Vector128.LoadUnsafe(ref Unsafe.Add(ref srcRef, i));
                    var f2 = Vector128.LoadUnsafe(ref Unsafe.Add(ref srcRef, i + 4));

                    var clamped1 = Vector128.Min(Vector128.Max(Vector128.Multiply(f1, scale), minVal), maxVal);
                    var clamped2 = Vector128.Min(Vector128.Max(Vector128.Multiply(f2, scale), minVal), maxVal);

                    var int1 = Vector128.ConvertToInt32(clamped1);
                    var int2 = Vector128.ConvertToInt32(clamped2);

                    var shorts128 = Vector128.Narrow(int1, int2);

                    shorts128.StoreUnsafe(ref Unsafe.Add(ref dstRef, i));
                }
            }

            for (; i < readSamples; i++)
            {
                var scaled = sourceSpan[i] * 32768.0f;
                targetSpan[i] = (short)Math.Clamp((int)scaled, short.MinValue, short.MaxValue);
            }

            return readBytes;
        }
    }
}