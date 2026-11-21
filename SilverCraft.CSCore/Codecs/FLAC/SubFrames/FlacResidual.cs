using SilverCraft.CSCore.Utils;

// ReSharper disable once CheckNamespace
namespace SilverCraft.CSCore.Codecs.FLAC;

internal sealed class FlacResidual
{
#if FLAC_DEBUG
    public FlacResidualCodingMethod CodingMethodMethod { get; private set; }

    public int PartitionOrder { get; private set; }

    public FlacResidual(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int order)
    {
        Decode(ref reader, header, data, order, out var codingMethod, out var partitionOrder);
        CodingMethodMethod = codingMethod;
        PartitionOrder = partitionOrder;
    }
#endif

    public static void Decode(ref SpanBitReader reader, FlacFrameHeader header, FlacSubFrameData data, int order)
        => Decode(ref reader, header, data, order, out _, out _);

    private static void Decode(
        ref SpanBitReader reader,
        FlacFrameHeader header,
        FlacSubFrameData data,
        int order,
        out FlacResidualCodingMethod codingMethod,
        out int partitionOrder)
    {
        codingMethod = (FlacResidualCodingMethod)reader.ReadBits(2); // 2 Bit

        if (codingMethod is FlacResidualCodingMethod.PartitionedRice or FlacResidualCodingMethod.PartitionedRice2)
        {
            partitionOrder = (int)reader.ReadBits(4); //"Partition order." see https://xiph.org/flac/format.html#partitioned_rice and https://xiph.org/flac/format.html#partitioned_rice2

            FlacPartitionedRice.ProcessResidual(ref reader, header, data, order, partitionOrder, codingMethod);
        }
        else
        {
            throw new FlacException("Not supported RICE-Coding-Method. Stream unparseable!", FlacLayer.SubFrame);
        }
    }
}