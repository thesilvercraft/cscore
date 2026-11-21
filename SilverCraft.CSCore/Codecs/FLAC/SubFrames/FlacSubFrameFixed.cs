using SilverCraft.CSCore.Codecs.FLAC.SubFrames;
using SilverCraft.CSCore.Utils;

// ReSharper disable once CheckNamespace
namespace SilverCraft.CSCore.Codecs.FLAC
{
    internal sealed class FlacSubFrameFixed : FlacSubFrameBase
    {
#if FLAC_DEBUG
        public FlacResidual Residual { get; private set; }

        public FlacSubFrameFixed(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample, int order)
            : base(header)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(order, header.BlockSize);
            DecodeWarmup(ref reader, data, bitsPerSample, order);
            Residual = new FlacResidual(ref reader, header, data, order);
            RestoreSignal(data, header.BlockSize - order, order);
        }
#endif

        public static void Decode(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int bitsPerSample, int order)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(order, header.BlockSize);
            DecodeWarmup(ref reader, data, bitsPerSample, order);
            FlacResidual.Decode(ref reader, header, data, order);
            RestoreSignal(data, header.BlockSize - order, order);
        }

        private static void DecodeWarmup(ref SpanBitReader reader, FlacSubFrameData data, int bitsPerSample, int order)
        {
            var resSpan = data.ResidualSpan;
            var destSpan = data.DestinationSpan;

            for (var i = 0; i < order; i++)
            {
                var val = reader.ReadBitsSigned(bitsPerSample);
                resSpan[i] = destSpan[i] = val;
            }
        }

        private static void RestoreSignal(FlacSubFrameData subframeData, int length, int order)
        {
            // See https://mi.eng.cam.ac.uk/reports/svr-ftp/auto-pdf/robinson_tr156.pdf chapter 3.2
            var residual = subframeData.ResidualSpan;
            var destBuffer = subframeData.DestinationSpan;

            switch (order)
            {
                case 0:
                    for (var i = order; i < length + order; i++)
                    {
                        destBuffer[i] = residual[i];
                    }
                    break;

                case 1:
                    for (var i = order; i < length + order; i++)
                    {
                        // s(t-1)
                        destBuffer[i] = residual[i] + destBuffer[i - 1];
                    }
                    break;

                case 2:
                    for (var i = order; i < length + order; i++)
                    {
                        // 2s(t-1) - s(t-2)
                        destBuffer[i] = residual[i] + (destBuffer[i - 1] << 1) - destBuffer[i - 2];
                    }
                    break;

                case 3:
                    for (var i = order; i < length + order; i++)
                    {
                        // 3s(t-1) - 3s(t-2) + s(t-3)
                        destBuffer[i] = residual[i] + 
                            3 * destBuffer[i - 1] - 3 * destBuffer[i - 2] + destBuffer[i - 3]; 
                    }
                    break;

                case 4:
                    // "FLAC adds a fourth-order predictor to the zero-to-third-order predictors used by Shorten."
                    for (var i = order; i < length + order; i++)
                    {
                        // 4s(t-1) - 6s(t-2) + 4s(t-3) - s(t-4)
                        destBuffer[i] = residual[i] +
                            (destBuffer[i - 1] << 2) - (destBuffer[i - 2] * 6) + (destBuffer[i - 3] << 2) - destBuffer[i - 4];
                    }
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine("Invalid FlacFixedSubFrame predictororder.");
                    return;
            }
        }
    }
}