using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class TimeConverterTests
{
    [Fact]
    public void WaveSourceTimeConverter_ConvertsTimeSpanAndBytesAccurately()
    {
        var format = new WaveFormat(44100, 16, 2); // 176400 bytes per second
        var converter = TimeConverter.WaveSourceTimeConverter;

        var oneSecond = TimeSpan.FromSeconds(1);
        var bytes = converter.ToRawElements(format, oneSecond);
        Assert.Equal(176400, bytes);

        var timeSpan = converter.ToTimeSpan(format, 176400);
        Assert.Equal(oneSecond, timeSpan);
    }

    [Fact]
    public void WaveSourceTimeConverter_FractionalDurationConvertsCorrectly()
    {
        var format = new WaveFormat(48000, 24, 2); // 288000 bytes per second
        var converter = TimeConverter.WaveSourceTimeConverter;

        var halfSecond = TimeSpan.FromMilliseconds(500);
        var bytes = converter.ToRawElements(format, halfSecond);
        Assert.Equal(144000, bytes);

        var timeSpan = converter.ToTimeSpan(format, bytes);
        Assert.Equal(halfSecond, timeSpan);
    }

    [Fact]
    public void SampleSourceTimeConverter_ConvertsTimeSpanAndSamplesAccurately()
    {
        var format = new WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat);
        var converter = TimeConverter.SampleSourceTimeConverter;

        var twoSeconds = TimeSpan.FromSeconds(2);
        var samples = converter.ToRawElements(format, twoSeconds);
        // 44100 * 2 seconds * 2 channels = 176400 samples total
        Assert.Equal(176400, samples);

        var timeSpan = converter.ToTimeSpan(format, samples);
        Assert.Equal(twoSeconds, timeSpan);
    }

    [Fact]
    public void TimeConverterFactory_ResolvesBuiltInSourceTypes()
    {
        var factory = TimeConverterFactory.Instance;

        using var waveSource = new TestWaveSource([], new WaveFormat(44100, 16, 2));
        var waveConverter = factory.GetTimeConverterForSource(waveSource);
        Assert.Same(TimeConverter.WaveSourceTimeConverter, waveConverter);

        using var sampleSource = new TestSampleSource([]);
        var sampleConverter = factory.GetTimeConverterForSource(sampleSource);
        Assert.Same(TimeConverter.SampleSourceTimeConverter, sampleConverter);
    }

    [Fact]
    public void TimeConverterFactory_ThrowsWhenSourceIsNull()
    {
        var factory = TimeConverterFactory.Instance;
        Assert.Throws<ArgumentNullException>(() => factory.GetTimeConverterForSource<IWaveSource>(null!));
    }
}
