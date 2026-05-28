

// ReSharper disable once CheckNamespace
using SilverCraft.CSCore.Codecs.FLAC.SubFrames;

namespace SilverCraft.CSCore.Codecs.FLAC;
internal sealed partial class FlacSubFrameLPC : FlacSubFrameBase
{
#if FLAC_DEBUG
    public int QLPCoeffPrecision { get; private set; }
    public int LPCShiftNeeded { get; private set; }
    public int[] QLPCoeffs { get; private set; }
    public int[] Warmup { get; private set; }
    public FlacResidual Residual { get; private set; }
#endif

  public FlacSubFrameLPC(FlacBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample, int order)
    : base(header)
{
    var resSpan = data.ResidualSpan;
    var destSpan = data.DestinationSpan;
    var warmup = new int[order];
    for (var i = 0; i < order; i++)
    {
        warmup[i] = reader.ReadBitsSigned(bitsPerSample);
    }

    var coefPrecision = (int)reader.ReadBits(4);
    if (coefPrecision == 0x0F)
        throw new FlacException("Invalid \"quantized linear predictor coefficients' precision in bits\" was invalid. Must not be 0x0F.",
            FlacLayer.SubFrame);
    coefPrecision += 1;

    var shiftNeeded = reader.ReadBitsSigned(5);
    if (shiftNeeded < 0)
        throw new FlacException("'\"Quantized linear predictor coefficient shift needed in bits\" was negative.", FlacLayer.SubFrame);

    var q = new int[order];
    for (var i = 0; i < order; i++)
    {
        q[i] = reader.ReadBitsSigned(coefPrecision);
    }
    warmup.AsSpan().CopyTo(destSpan[..order]);
 
    var residual = new FlacResidual(reader, header, data, order);
    var blockSizeToProcess = header.BlockSize;
    if (bitsPerSample + coefPrecision + Log2(order) <= 32)
    {
        RestoreLPCSignal32(resSpan, destSpan, blockSizeToProcess, order, q, shiftNeeded);
    }
    else
    {
        RestoreLPCSignal64(resSpan, destSpan, blockSizeToProcess, order, q, shiftNeeded);
    }

#if FLAC_DEBUG
    QLPCoeffPrecision = coefPrecision;
    LPCShiftNeeded = shiftNeeded;
    Warmup = warmup;
    Residual = residual;
    QLPCoeffs = q;
#endif
}

    private int Log2(int x)
    {
        var bits = 0;
        while (x > 0)
        {
            bits++;
            x >>= 1;
        }
        return bits;
    }
}