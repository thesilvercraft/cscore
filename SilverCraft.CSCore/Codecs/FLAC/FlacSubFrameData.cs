namespace SilverCraft.CSCore.Codecs.FLAC
{
    internal  class FlacSubFrameData
    {
        public Memory<int> DestinationBuffer { get; set; }
        public Memory<int> ResidualBuffer { get; set; }
        public Span<int> DestinationSpan => DestinationBuffer.Span;
        public Span<int> ResidualSpan => ResidualBuffer.Span;
        public FlacPartitionedRiceContent Content = new();
    }
}