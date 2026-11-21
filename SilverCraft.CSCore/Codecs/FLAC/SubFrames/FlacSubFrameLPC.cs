using System.Numerics;
using System.Runtime.CompilerServices;
using SilverCraft.CSCore.Codecs.FLAC.SubFrames;
using SilverCraft.CSCore.Utils;

// ReSharper disable once CheckNamespace
namespace SilverCraft.CSCore.Codecs.FLAC;

internal sealed partial class FlacSubFrameLPC : FlacSubFrameBase
{
#if FLAC_DEBUG
    public int QLPCoeffPrecision { get; private set; }
    public int LPCShiftNeeded { get; private set; }
    public int[] QLPCoeffs { get; private set; }
    public int[] Warmup { get; private set; }
    public FlacResidual Residual { get; private set; }

    public FlacSubFrameLPC(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample, int order)
        : base(header)
    {
        Span<int> q = stackalloc int[order];
        ReadWarmupAndCoeffs(ref reader, header, data, bitsPerSample, order, q, out var coefPrecision, out var shiftNeeded);

        var residual = new FlacResidual(ref reader, header, data, order);
        Restore(data.ResidualSpan, data.DestinationSpan, header.BlockSize, order, q, bitsPerSample, coefPrecision, shiftNeeded);

        QLPCoeffPrecision = coefPrecision;
        LPCShiftNeeded = shiftNeeded;
        Warmup = data.DestinationSpan[..order].ToArray();
        Residual = residual;
        QLPCoeffs = q.ToArray();
    }
#endif

    public static void Decode(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample, int order)
    {
        Span<int> q = stackalloc int[order];
        ReadWarmupAndCoeffs(ref reader, header, data, bitsPerSample, order, q, out var coefPrecision, out var shiftNeeded);

        FlacResidual.Decode(ref reader, header, data, order);
        Restore(data.ResidualSpan, data.DestinationSpan, header.BlockSize, order, q, bitsPerSample, coefPrecision, shiftNeeded);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReadWarmupAndCoeffs(
        ref SpanBitReader reader,
        FlacFrameHeader header,
        FlacSubFrameData data,
        int bitsPerSample,
        int order,
        scoped Span<int> q,
        out int coefPrecision,
        out int shiftNeeded)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(order, header.BlockSize);

        var destSpan = data.DestinationSpan;
        for (var i = 0; i < order; i++)
        {
            destSpan[i] = reader.ReadBitsSigned(bitsPerSample);
        }

        coefPrecision = (int)reader.ReadBits(4);
        if (coefPrecision == 0x0F)
            throw new FlacException("Invalid \"quantized linear predictor coefficients' precision in bits\" was invalid. Must not be 0x0F.",
                FlacLayer.SubFrame);
        coefPrecision += 1;

        shiftNeeded = reader.ReadBitsSigned(5);
        if (shiftNeeded < 0)
            throw new FlacException("'\"Quantized linear predictor coefficient shift needed in bits\" was negative.", FlacLayer.SubFrame);

        for (var i = 0; i < order; i++)
        {
            q[i] = reader.ReadBitsSigned(coefPrecision);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Restore(
        scoped ReadOnlySpan<int> resSpan,
        scoped Span<int> destSpan,
        int blockSize,
        int order,
        scoped ReadOnlySpan<int> q,
        int bitsPerSample,
        int coefPrecision,
        int shiftNeeded)
    {
        if (bitsPerSample + coefPrecision + BitOperations.Log2((uint)order) <= 32)
        {
            RestoreLPCSignal32(resSpan, destSpan, blockSize, order, q, shiftNeeded);
        }
        else
        {
            RestoreLPCSignal64(resSpan, destSpan, blockSize, order, q, shiftNeeded);
        }
    }
}