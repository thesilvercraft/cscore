using SilverCraft.CSCore.Codecs.FLAC.SubFrames;
using SilverCraft.CSCore.Utils;

// ReSharper disable once CheckNamespace
namespace SilverCraft.CSCore.Codecs.FLAC;

internal sealed class FlacSubFrameConstant : FlacSubFrameBase
{
#if FLAC_DEBUG
    public int Value { get; private set; }

    public FlacSubFrameConstant(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample)
        : base(header)
    {
        Value = Decode(ref reader, header, data, bitsPerSample);
    }
#endif

    public static int Decode(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample)
    {
        var value = (int)reader.ReadBits(bitsPerSample);
        var destSpan = data.DestinationSpan[..header.BlockSize];
        destSpan.Fill(value);
        return value;
    }
}