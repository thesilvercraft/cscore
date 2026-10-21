using SilverCraft.CSCore.DSP;
using SilverCraft.CSCore.Streams.Effects;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class DspCoverageTests
{
    [Theory]
    [InlineData(100.0)]
    [InlineData(10000.0)]
    public void BiquadFilters_ProduceFiniteStableOutput(double frequency)
    {
        var filters = new BiQuad[]
        {
            new BandpassFilter(48000, frequency),
            new HighShelfFilter(48000, frequency, 6),
            new LowShelfFilter(48000, frequency, -6),
            new NotchFilter(48000, frequency),
            new PeakFilter(48000, frequency, 1, 6)
        };

        foreach (var filter in filters)
        {
            var data = Enumerable.Repeat(0.5f, 512).ToArray();
            filter.Process(data);
            Assert.All(data, sample => Assert.True(float.IsFinite(sample)));
            Assert.Contains(data, sample => Math.Abs(sample) > 0.001f);
        }
    }

    [Fact]
    public void ShelfAndPeakFilters_RecalculateForBoostAndCut()
    {
        var low = new LowShelfFilter(48000, 200, 6);
        var high = new HighShelfFilter(48000, 8000, 6);
        var peak = new PeakFilter(48000, 1000, 1, 6);
        low.GainDB = -6;
        high.GainDB = -6;
        peak.GainDB = -6;

        Assert.All(new BiQuad[] { low, high, peak }, filter =>
        {
            var samples = new[] { 1f, 0f, 0f, 0f };
            filter.Process(samples);
            Assert.All(samples, sample => Assert.True(float.IsFinite(sample)));
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => peak.BandWidth = 0);
    }

    [Fact]
    public void ChannelMatrix_MapsStereoToMonoAndFlipRestoresDimensions()
    {
        var matrix = ChannelMatrix.GetMatrix(ChannelMasks.StereoMask, ChannelMasks.MonoMask);
        Assert.Equal(2, matrix.InputChannelCount);
        Assert.Equal(1, matrix.OutputChannelCount);
        Assert.Equal(0.5f, matrix[0, 0].Value);
        Assert.Equal(0.5f, matrix[1, 0].Value);

        var flipped = matrix.Flip();
        Assert.Equal(1, flipped.InputChannelCount);
        Assert.Equal(2, flipped.OutputChannelCount);
        Assert.Equal(matrix[1, 0].Value, flipped[0, 1].Value);
    }

    [Fact]
    public void ChannelMatrix_ValidatesMasksAndMatrixDimensions()
    {
        Assert.Throws<ArgumentException>(() => new ChannelMatrix(0, ChannelMasks.StereoMask));
        var matrix = ChannelMatrix.MonoToStereoMatrix;
        Assert.Throws<ArgumentException>(() => matrix.SetMatrix(new float[1, 1]));
        Assert.Throws<ArgumentNullException>(() => matrix.SetMatrix(null!));
        Assert.Throws<ArgumentException>(() => ChannelMatrix.GetMatrix(ChannelMasks.StereoMask, ChannelMasks.StereoMask));
    }

    [Fact]
    public void ChannelMatrixElement_StoresChannelMappingAndCoefficient()
    {
        var element = new ChannelMatrixElement(ChannelMask.SpeakerFrontLeft, ChannelMask.SpeakerFrontRight)
        {
            Value = 0.75f
        };
        Assert.Equal(ChannelMask.SpeakerFrontLeft, element.InputChannel);
        Assert.Equal(ChannelMask.SpeakerFrontRight, element.OutputChannel);
        Assert.Equal(0.75f, element.Value);
    }

    [Fact]
    public void FftProvider_ConsumesNewDataOnceAndValidatesBuffers()
    {
        var provider = new FftProvider(1, FftSize.Fft64);
        var result = new float[64];
        Assert.False(provider.IsNewDataAvailable);
        Assert.False(provider.GetFftData(result));

        provider.Add(Enumerable.Repeat(1f, 64).ToArray(), 64);
        Assert.True(provider.IsNewDataAvailable);
        Assert.True(provider.GetFftData(result));
        Assert.False(provider.IsNewDataAvailable);
        Assert.False(provider.GetFftData(result));
        Assert.Throws<ArgumentException>(() => provider.GetFftData(new float[8]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FftProvider(0, FftSize.Fft64));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FftProvider(1, (FftSize)42));
    }

    [Fact]
    public void EqualizerFilterCollection_SortsBandsAndAppliesAverageGain()
    {
        using var source = new TestSampleSource([0.2f, 0.4f]);
        using var equalizer = new Equalizer(source);
        var high = new EqualizerFilter(1, new EqualizerChannelFilter(44100, 2000, 1, 0));
        var low = new EqualizerFilter(1, new EqualizerChannelFilter(44100, 200, 1, 0));
        equalizer.SampleFilters.Add(high);
        equalizer.SampleFilters.Add(low);

        Assert.Same(low, equalizer.SampleFilters[0]);
        Assert.Equal(200, low.AverageFrequency);
        low.AverageGainDB = 4;
        Assert.Equal(4, low.AverageGainDB);
        Assert.Equal(2, equalizer.SampleFilters.Count);
        Assert.True(low.Filters[0].GainDB == 4);
    }

    [Fact]
    public void EqualizerAndChannelFilters_CloneAndClampOutput()
    {
        var channelFilter = new EqualizerChannelFilter(44100, 1000, 1, 0);
        var clone = Assert.IsType<EqualizerChannelFilter>(channelFilter.Clone());
        Assert.NotSame(channelFilter, clone);
        Assert.Equal(channelFilter.Frequency, clone.Frequency);

        using var source = new TestSampleSource([2f, -2f]);
        using var eq = new Equalizer(source);
        var output = new float[2];
        Assert.Equal(2, eq.Read(output, 0, 2));
        Assert.Equal(new[] { 1f, -1f }, output);
    }

    [Fact]
    public void CreateTenBandEqualizer_RequiresSampleRateAtLeast32000()
    {
        using var lowRate = new TestSampleSource([0f], new WaveFormat(22050, 32, 1, AudioEncoding.IeeeFloat));
        Assert.Throws<ArgumentException>(() => Equalizer.Create10BandEqualizer(lowRate));

        using var validRate = new TestSampleSource([0f], new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat));
        using var eq = Equalizer.Create10BandEqualizer(validRate);
        Assert.Equal(10, eq.SampleFilters.Count);
    }

    [Fact]
    public void PitchShifter_ProcessesShiftedSamplesAndKeepsOutputBounded()
    {
        var input = Enumerable.Range(0, 4096).Select(i => (float)(0.5 * Math.Sin(2 * Math.PI * 440 * i / 44100))).ToArray();
        using var source = new TestSampleSource(input);
        using var shifter = new PitchShifter(source) { PitchShiftFactor = 1.5f };
        var output = new float[input.Length];

        Assert.Equal(input.Length, shifter.Read(output, 0, output.Length));
        Assert.All(output, sample => Assert.True(float.IsFinite(sample) && sample is >= -1f and <= 1f));
        Assert.Contains(output, sample => Math.Abs(sample) > 0.01f);
    }
}

