using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

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
    public override int Read(byte[] buffer, int offset, int count)
    {
        var sampleCount = count >> 2;
        if (sampleCount <= 0) return 0;

        Buffer = Buffer.CheckBuffer(sampleCount);

        var samplesRead = Source.Read(Buffer, 0, sampleCount);
        if (samplesRead <= 0) return 0;

        ReadOnlySpan<float> sourceSpan = Buffer.AsSpan(0, samplesRead);
        var destSpan = MemoryMarshal.Cast<byte, int>(buffer.AsSpan(offset, samplesRead * 4));
        var i = 0;

        ref var srcRef = ref MemoryMarshal.GetReference(sourceSpan);
        ref var destRef = ref MemoryMarshal.GetReference(destSpan);
        if (Vector256.IsHardwareAccelerated)
        {
            var scaleMultiplier = Vector256.Create(2147483520.0f);
            var minVal = Vector256.Create((float)int.MinValue);
            var maxVal = Vector256.Create((float)int.MaxValue);

            var limit = samplesRead & ~7;
            for (; i < limit; i += 8)
            {
                var floatInput = Vector256.LoadUnsafe(ref srcRef, (uint)i);
                var multiplied = Vector256.Multiply(floatInput, scaleMultiplier);
                var clamped = Vector256.Min(Vector256.Max(multiplied, minVal), maxVal);
                var intVector = Vector256.ConvertToInt32(clamped);
                intVector.StoreUnsafe(ref destRef, (uint)i);
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            var scaleMultiplier = Vector128.Create(2147483520.0f);
            var minVal = Vector128.Create((float)int.MinValue);
            var maxVal = Vector128.Create((float)int.MaxValue);

            var limit = samplesRead & ~7;
            for (; i < limit; i += 8)
            {
                var v0 = Vector128.LoadUnsafe(ref srcRef, (uint)i);
                var v1 = Vector128.LoadUnsafe(ref srcRef, (uint)(i + 4));

                v0 = Vector128.Min(Vector128.Max(Vector128.Multiply(v0, scaleMultiplier), minVal), maxVal);
                v1 = Vector128.Min(Vector128.Max(Vector128.Multiply(v1, scaleMultiplier), minVal), maxVal);

                Vector128.ConvertToInt32(v0).StoreUnsafe(ref destRef, (uint)i);
                Vector128.ConvertToInt32(v1).StoreUnsafe(ref destRef, (uint)(i + 4));
            }

            if (i <= samplesRead - 4)
            {
                var v0 = Vector128.LoadUnsafe(ref srcRef, (uint)i);
                v0 = Vector128.Min(Vector128.Max(Vector128.Multiply(v0, scaleMultiplier), minVal), maxVal);
                Vector128.ConvertToInt32(v0).StoreUnsafe(ref destRef, (uint)i);
                i += 4;
            }
        }

        for (; i < samplesRead; i++)
        {
            var scaled = sourceSpan[i] * 2147483520.0f;
            destSpan[i] = (int)Math.Clamp(scaled, int.MinValue, int.MaxValue);
        }

        return samplesRead << 2;
    }
}