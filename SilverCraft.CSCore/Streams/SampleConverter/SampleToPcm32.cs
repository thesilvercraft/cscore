using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SilverCraft.CSCore.Streams.SampleConverter;

/// <summary>
/// Converts a <see cref="ISampleSource"/> to a 32-bit PCM <see cref="IWaveSource"/>.
/// </summary>
public class SampleToPcm32 : SampleToWaveBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SampleToPcm32"/> class.
    /// </summary>
    /// <param name="source">The underlying <see cref="ISampleSource"/> which has to get converted to a 32-bit PCM <see cref="IWaveSource"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public SampleToPcm32(ISampleSource source)
        : base(source, 32, AudioEncoding.Pcm)
    {
        ArgumentNullException.ThrowIfNull(source);
    }

    /// <summary>
    ///     Reads a sequence of bytes from the <see cref="SampleToPcm32" /> and advances the position within the stream by the
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
    public override unsafe int Read(byte[] buffer, int offset, int count)
    {
        var sampleCount = count >> 2;
        if (sampleCount <= 0) return 0;

        Buffer = Buffer.CheckBuffer(sampleCount);

        var samplesRead = Source.Read(Buffer, 0, sampleCount);
        if (samplesRead <= 0) return 0;

        var bytesRead = samplesRead << 2;

        fixed (float* srcStart = Buffer)
        fixed (byte* dstStart = buffer)
        {
            var dest = (int*)(dstStart + offset);
            var i = 0;
            if (Avx.IsSupported)
            {
                var scaleMultiplier = Vector256.Create((float)int.MaxValue);
                var avxLimit = samplesRead & ~7; 
                for (; i < avxLimit; i += 8)
                {
                    var floatInput = Unsafe.ReadUnaligned<Vector256<float>>(srcStart + i);
                    var multiplied = Avx.Multiply(floatInput, scaleMultiplier);
                    var intVector = Avx.ConvertToVector256Int32(multiplied);
                    Unsafe.WriteUnaligned(dest + i, intVector);
                }
            }
            else if (Sse2.IsSupported)
            {
                var scaleMultiplier128 = Vector128.Create((float)int.MaxValue);
                var sseLimit = samplesRead & ~3;
                for (; i < sseLimit; i += 4)
                {
                    var floatInput = Unsafe.ReadUnaligned<Vector128<float>>(srcStart + i);
                    var multiplied = Sse.Multiply(floatInput, scaleMultiplier128);
                    var intVector = Sse2.ConvertToVector128Int32WithTruncation(multiplied);
                    Unsafe.WriteUnaligned(dest + i, intVector);
                }
            }
            for (; i < samplesRead; i++)
            {
                dest[i] = (int)(srcStart[i] * int.MaxValue);
            }
        }

        return bytesRead;
    }
}