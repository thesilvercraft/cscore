// ReSharper disable once CheckNamespace
using SilverCraft.CSCore.Codecs.FLAC.SubFrames;

namespace SilverCraft.CSCore.Codecs.FLAC;

internal sealed class FlacSubFrameConstant : FlacSubFrameBase
{
#if FLAC_DEBUG
    public int Value { get; private set; }
#endif

    public FlacSubFrameConstant(FlacBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample)
        : base(header)
    {
        var value = (int)reader.ReadBits(bitsPerSample);
#if FLAC_DEBUG
        Value = value;
#endif

        var destSpan = data.DestinationSpan[..header.BlockSize];
        destSpan.Fill(value);
    }
}