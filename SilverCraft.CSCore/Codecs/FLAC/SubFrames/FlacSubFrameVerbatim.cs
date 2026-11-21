using SilverCraft.CSCore.Codecs.FLAC.SubFrames;
using SilverCraft.CSCore.Utils;

// ReSharper disable once CheckNamespace
namespace SilverCraft.CSCore.Codecs.FLAC
{
    internal sealed class FlacSubFrameVerbatim : FlacSubFrameBase
    {
#if FLAC_DEBUG
        public FlacSubFrameVerbatim(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample)
            : base(header)
        {
            Decode(ref reader, header, data, bitsPerSample);
        }
#endif

        public static void Decode(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample)
        {
            var destSpan = data.DestinationSpan;
            var residualSpan = data.ResidualSpan;
            for (var i = 0; i < header.BlockSize; i++)
            {
                var sample = (int)reader.ReadBits(bitsPerSample);
                destSpan[i] = sample;
                residualSpan[i] = sample;
            }
        }
    }
}