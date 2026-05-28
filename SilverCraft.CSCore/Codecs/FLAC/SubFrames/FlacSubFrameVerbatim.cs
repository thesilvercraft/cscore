// ReSharper disable once CheckNamespace
using SilverCraft.CSCore.Codecs.FLAC.SubFrames;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    internal sealed class FlacSubFrameVerbatim : FlacSubFrameBase
    {
        public FlacSubFrameVerbatim(FlacBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample)
            : base(header)
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