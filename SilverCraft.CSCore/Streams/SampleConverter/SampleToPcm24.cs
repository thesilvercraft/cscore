using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SilverCraft.CSCore.Streams.SampleConverter
{
    /// <summary>
    /// Converts a <see cref="ISampleSource"/> to a 24-bit PCM <see cref="IWaveSource"/>.
    /// </summary>
    public class SampleToPcm24 : SampleToWaveBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToPcm24"/> class.
        /// </summary>
        /// <param name="source">The underlying <see cref="ISampleSource"/> which has to get converted to a 24-bit PCM <see cref="IWaveSource"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public SampleToPcm24(ISampleSource source)
            : base(source, 24, AudioEncoding.Pcm)
        {
            ArgumentNullException.ThrowIfNull(source);
        }

        /// <summary>
        ///     Reads a sequence of bytes from the <see cref="SampleToPcm24" /> and advances the position within the stream by the
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
            var sampleCount = count / 3;
            Buffer = Buffer.CheckBuffer(sampleCount);
            var samplesRead = Source.Read(Buffer, 0, sampleCount);

            ReadOnlySpan<float> sourceSamples = Buffer.AsSpan(0, samplesRead);
            var targetBytes = buffer.AsSpan(offset, samplesRead * 3);

            var i = 0;

            if (Vector256.IsHardwareAccelerated && sourceSamples.Length >= 8)
            {
                var min = Vector256.Create(-1.0f);
                var max = Vector256.Create(1.0f);
                var scale = Vector256.Create(8388607.0f);

                ReadOnlySpan<byte> shufflePattern = [0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 255, 255, 255, 255];
                var shuffleMask = Vector128.Create(shufflePattern);

                ref var sourceRef = ref MemoryMarshal.GetReference(sourceSamples);
                ref var targetRef = ref MemoryMarshal.GetReference(targetBytes);

                var vectorLength = sourceSamples.Length - (sourceSamples.Length % 8);

                for (; i < vectorLength; i += 8)
                {
                    var v = Vector256.LoadUnsafe(ref sourceRef, (uint)i);
                    v = Vector256.Min(Vector256.Max(v, min), max);
                    v = Vector256.Multiply(v, scale);
                    var ints = Vector256.ConvertToInt32(v);
                    var lowerInts = ints.GetLower();
                    var upperInts = ints.GetUpper();
                    var packedLower = Vector128.Shuffle(lowerInts.AsByte(), shuffleMask);
                    var packedUpper = Vector128.Shuffle(upperInts.AsByte(), shuffleMask);
                    var byteIndex = i * 3;
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex),
                        packedLower.AsUInt64().GetElement(0));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex + 8),
                        packedLower.AsUInt32().GetElement(2));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex + 12),
                        packedUpper.AsUInt64().GetElement(0));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex + 20),
                        packedUpper.AsUInt32().GetElement(2));
                }
            }
            else if (Vector128.IsHardwareAccelerated && sourceSamples.Length >= 4)
            {
                var min = Vector128.Create(-1.0f);
                var max = Vector128.Create(1.0f);
                var scale = Vector128.Create(8388607.0f);

                ReadOnlySpan<byte> shufflePattern = [0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 255, 255, 255, 255];
                var shuffleMask = Vector128.Create(shufflePattern);

                ref var sourceRef = ref MemoryMarshal.GetReference(sourceSamples);
                ref var targetRef = ref MemoryMarshal.GetReference(targetBytes);

                var vectorLength8 = sourceSamples.Length - (sourceSamples.Length % 8);

                for (; i < vectorLength8; i += 8)
                {
                    var v0 = Vector128.LoadUnsafe(ref sourceRef, (uint)i);
                    var v1 = Vector128.LoadUnsafe(ref sourceRef, (uint)(i + 4));

                    v0 = Vector128.Multiply(Vector128.Min(Vector128.Max(v0, min), max), scale);
                    v1 = Vector128.Multiply(Vector128.Min(Vector128.Max(v1, min), max), scale);

                    var ints0 = Vector128.ConvertToInt32(v0);
                    var ints1 = Vector128.ConvertToInt32(v1);

                    var packed0 = Vector128.Shuffle(ints0.AsByte(), shuffleMask);
                    var packed1 = Vector128.Shuffle(ints1.AsByte(), shuffleMask);

                    var byteIndex = i * 3;

                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex), packed0.AsUInt64().GetElement(0));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex + 8),
                        packed0.AsUInt32().GetElement(2));

                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex + 12),
                        packed1.AsUInt64().GetElement(0));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex + 20),
                        packed1.AsUInt32().GetElement(2));
                }

                if (i <= sourceSamples.Length - 4)
                {
                    var v = Vector128.LoadUnsafe(ref sourceRef, (uint)i);
                    v = Vector128.Multiply(Vector128.Min(Vector128.Max(v, min), max), scale);
                    var ints = Vector128.ConvertToInt32(v);
                    var packed = Vector128.Shuffle(ints.AsByte(), shuffleMask);

                    var byteIndex = i * 3;
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex), packed.AsUInt64().GetElement(0));
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref targetRef, byteIndex + 8),
                        packed.AsUInt32().GetElement(2));

                    i += 4;
                }
            }

            for (; i < sourceSamples.Length; i++)
            {
                var sample = Math.Clamp(sourceSamples[i], -1.0f, 1.0f);
                var sample24 = (int)(sample * 8388607f);
                var byteIndex = i * 3;

                targetBytes[byteIndex] = (byte)(sample24 & 0xFF);
                targetBytes[byteIndex + 1] = (byte)((sample24 >> 8) & 0xFF);
                targetBytes[byteIndex + 2] = (byte)((sample24 >> 16) & 0xFF);
            }

            return samplesRead * 3;
        }
    }
}