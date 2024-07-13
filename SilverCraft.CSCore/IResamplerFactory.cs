namespace SilverCraft.CSCore
{
    public interface IResamplerFactory
    {
        IWaveSource CreateResampler(IWaveSource waveSource, int targetSampleRate);
        ISampleSource CreateResampler(ISampleSource sampleSource, int targetSampleRate);
    }
}
