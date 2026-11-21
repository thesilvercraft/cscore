using SilverCraft.CSCore.Utils;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    internal static class FlacPartitionedRice
    {
        public static void ProcessResidual(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data,
            int order, int partitionOrder, FlacResidualCodingMethod codingMethod)
        {
            data.Content.UpdateSize(partitionOrder);
            var isRice2 = codingMethod == FlacResidualCodingMethod.PartitionedRice2;
            var riceParameterLength = isRice2 ? 5 : 4;
            var escapeCode = isRice2 ? 31 : 15;

            var partitionCount = 1 << partitionOrder;

            var residualSpan = data.ResidualSpan[order..];

            for (var p = 0; p < partitionCount; p++)
            {
                int samplesPerPartition;
                if (partitionOrder == 0)
                    samplesPerPartition = header.BlockSize - order;
                else if (p > 0)
                    samplesPerPartition = header.BlockSize >> partitionOrder;
                else
                    samplesPerPartition = (header.BlockSize >> partitionOrder) - order;

                var riceParameter = reader.ReadBits(riceParameterLength);
                data.Content.Parameters[p] = (int)riceParameter;

                if (riceParameter >= escapeCode)
                {
                    var raw = (int)reader.ReadBits(5);
                    data.Content.RawBits[p] = raw;

                    if (raw == 0)
                    {
                        residualSpan[..samplesPerPartition].Clear();
                    }
                    else
                    {
                        for (var i = 0; i < samplesPerPartition; i++)
                        {
                            residualSpan[i] = reader.ReadBitsSigned(raw);
                        }
                    }
                }
                else
                {
                    ReadFlacRiceBlock(ref reader, samplesPerPartition, (int)riceParameter, residualSpan[..samplesPerPartition]);
                }

                residualSpan = residualSpan[samplesPerPartition..];
            }
        }

        private static void ReadFlacRiceBlock(ref SpanBitReader reader, int nvals, int riceParameter, Span<int> ptrDest)
        {
            if (riceParameter == 0)
            {
                for (var i = 0; i < nvals; i++)
                {
                    ptrDest[i] = reader.ReadUnarySigned();
                }
            }
            else
            {
                for (var i = 0; i < nvals; i++)
                {
                    ptrDest[i] = reader.ReadRiceSample(riceParameter);
                }
            }
        }
    }
}