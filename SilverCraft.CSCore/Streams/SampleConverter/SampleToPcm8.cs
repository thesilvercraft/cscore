using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SilverCraft.CSCore.Streams.SampleConverter
{
    /// <summary>
    /// Converts a <see cref="ISampleSource"/> to a 8-bit PCM <see cref="IWaveSource"/>.
    /// </summary>
    public class SampleToPcm8 : SampleToWaveBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToPcm8"/> class.
        /// </summary>
        /// <param name="source">The underlying <see cref="ISampleSource"/> which has to get converted to a 8-bit PCM <see cref="IWaveSource"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public SampleToPcm8(ISampleSource source)
            : base(source, 8, AudioEncoding.Pcm)
        {
        }

        /// <summary>
        ///     Reads a sequence of bytes from the <see cref="SampleToPcm8" /> and advances the position within the stream by the
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

            Buffer = Buffer.CheckBuffer(count);
            var readSamples = Source.Read(Buffer, 0, count);

            if (readSamples == 0) return 0;

            ReadOnlySpan<float> sourceSpan = Buffer.AsSpan(0, readSamples);
            var targetSpan = buffer.AsSpan(offset, readSamples);

            var i = 0;
            ref var srcRef = ref MemoryMarshal.GetReference(sourceSpan);
            ref var dstRef = ref MemoryMarshal.GetReference(targetSpan);

            if (Vector256.IsHardwareAccelerated && sourceSpan.Length >= 32)
            {
                var ones = Vector256.Create(1.0f);
                var scale = Vector256.Create(128.0f);
                var simdLimit = sourceSpan.Length & ~31;
                for (; i < simdLimit; i += 32)
                {
                    var f1 = Vector256.LoadUnsafe(ref Unsafe.Add(ref srcRef, i));
                    var f2 = Vector256.LoadUnsafe(ref Unsafe.Add(ref srcRef, i + 8));
                    var f3 = Vector256.LoadUnsafe(ref Unsafe.Add(ref srcRef, i + 16));
                    var f4 = Vector256.LoadUnsafe(ref Unsafe.Add(ref srcRef, i + 24));
                    var m1 = Vector256.Multiply(Vector256.Add(f1, ones), scale);
                    var m2 = Vector256.Multiply(Vector256.Add(f2, ones), scale);
                    var m3 = Vector256.Multiply(Vector256.Add(f3, ones), scale);
                    var m4 = Vector256.Multiply(Vector256.Add(f4, ones), scale);
                    var i1 = Vector256.ConvertToInt32(m1);
                    var i2 = Vector256.ConvertToInt32(m2);
                    var i3 = Vector256.ConvertToInt32(m3);
                    var i4 = Vector256.ConvertToInt32(m4);
                    var s1 = Vector256.Narrow(i1, i2);
                    var s2 = Vector256.Narrow(i3, i4);
                    var bytesOut = Vector256.Narrow(s1.AsUInt16(), s2.AsUInt16());
                    bytesOut.StoreUnsafe(ref Unsafe.Add(ref dstRef, i));
                }
            }

            if (Vector128.IsHardwareAccelerated && (sourceSpan.Length) >= 16)
            {
                var ones = Vector128.Create(1.0f);
                var scale = Vector128.Create(128.0f);
                var minVal = Vector128.Create(0.0f);
                var maxVal = Vector128.Create(255.0f);
                var simdLimit = sourceSpan.Length & ~15;
                for (; i < simdLimit; i += 16)
                {
                    var f1 = Vector128.LoadUnsafe(ref srcRef, (uint)i);
                    var f2 = Vector128.LoadUnsafe(ref srcRef, (uint)(i + 4));
                    var f3 = Vector128.LoadUnsafe(ref srcRef, (uint)(i + 8));
                    var f4 = Vector128.LoadUnsafe(ref srcRef, (uint)(i + 12));
                    var m1 = Vector128.Min(Vector128.Max(Vector128.Multiply(Vector128.Add(f1, ones), scale), minVal),
                        maxVal);
                    var m2 = Vector128.Min(Vector128.Max(Vector128.Multiply(Vector128.Add(f2, ones), scale), minVal),
                        maxVal);
                    var m3 = Vector128.Min(Vector128.Max(Vector128.Multiply(Vector128.Add(f3, ones), scale), minVal),
                        maxVal);
                    var m4 = Vector128.Min(Vector128.Max(Vector128.Multiply(Vector128.Add(f4, ones), scale), minVal),
                        maxVal);
                    var i1 = Vector128.ConvertToInt32(m1);
                    var i2 = Vector128.ConvertToInt32(m2);
                    var i3 = Vector128.ConvertToInt32(m3);
                    var i4 = Vector128.ConvertToInt32(m4);
                    var s1 = Vector128.Narrow(i1, i2);
                    var s2 = Vector128.Narrow(i3, i4);
                    var bytesOut = Vector128.Narrow(s1.AsUInt16(), s2.AsUInt16());
                    bytesOut.StoreUnsafe(ref dstRef, (uint)i);
                }
            }

            for (; i < readSamples; i++)
            {
                var scaled = (sourceSpan[i] + 1.0f) * 128.0f;
                targetSpan[i] = (byte)Math.Clamp((int)scaled, 0, 255);
            }

            return readSamples;
        }
    }
}