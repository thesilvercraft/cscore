using SilverCraft.CSCore.Streams;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class StreamChannelTests
{
    [Fact]
    public void MonoToStereo_DuplicatesChannelSamples()
    {
        var monoSamples = new[] { 0.2f, -0.4f, 0.6f, -0.8f };
        var monoFormat = new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat);
        using var source = new TestSampleSource(monoSamples, monoFormat);
        using var stereoSource = new MonoToStereoSource(source);

        Assert.Equal(2, stereoSource.WaveFormat.Channels);
        Assert.Equal(monoSamples.Length * 2, stereoSource.Length);

        var output = new float[8];
        var read = stereoSource.Read(output, 0, 8);

        Assert.Equal(8, read);
        Assert.Equal(new[] { 0.2f, 0.2f, -0.4f, -0.4f, 0.6f, 0.6f, -0.8f, -0.8f }, output);
    }

    [Fact]
    public void MonoToStereo_RejectsNonMonoSource()
    {
        var stereoFormat = new WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat);
        using var source = new TestSampleSource([], stereoFormat);

        Assert.Throws<ArgumentException>(() => new MonoToStereoSource(source));
    }

    [Fact]
    public void StereoToMono_AveragesStereoChannels()
    {
        var stereoSamples = new[] { 0.2f, 0.4f, -0.6f, 0.2f }; // Frame 0: (0.2+0.4)/2 = 0.3; Frame 1: (-0.6+0.2)/2 = -0.2
        var stereoFormat = new WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat);
        using var source = new TestSampleSource(stereoSamples, stereoFormat);
        using var monoSource = new StereoToMonoSource(source);

        Assert.Equal(1, monoSource.WaveFormat.Channels);
        Assert.Equal(stereoSamples.Length / 2, monoSource.Length);

        var output = new float[2];
        var read = monoSource.Read(output, 0, 2);

        Assert.Equal(2, read);
        Assert.Equal(0.3f, output[0], 5);
        Assert.Equal(-0.2f, output[1], 5);
    }

    [Fact]
    public void StereoToMono_RejectsNonStereoSource()
    {
        var monoFormat = new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat);
        using var source = new TestSampleSource([], monoFormat);

        Assert.Throws<ArgumentException>(() => new StereoToMonoSource(source));
    }

    [Fact]
    public void GainSource_AmplifiesAndClipsWhenConfigured()
    {
        var samples = new[] { 0.5f, -0.8f, 0.2f };
        using var source = new TestSampleSource(samples);
        using var gain = new GainSource(source)
        {
            Volume = 2.0f,
            ClipOverflows = true
        };

        var output = new float[3];
        var read = gain.Read(output, 0, 3);

        Assert.Equal(3, read);
        Assert.Equal(1.0f, output[0]); // 0.5 * 2.0 = 1.0
        Assert.Equal(-1.0f, output[1]); // -0.8 * 2.0 = -1.6, clipped to -1.0
        Assert.Equal(0.4f, output[2], 5); // 0.2 * 2.0 = 0.4
    }

    [Fact]
    public void GainSource_AllowsOverflowsWhenClipIsDisabled()
    {
        var samples = new[] { 0.8f, -0.9f };
        using var source = new TestSampleSource(samples);
        using var gain = new GainSource(source)
        {
            Volume = 2.0f,
            ClipOverflows = false
        };

        var output = new float[2];
        var read = gain.Read(output, 0, 2);

        Assert.Equal(2, read);
        Assert.Equal(1.6f, output[0], 5);
        Assert.Equal(-1.8f, output[1], 5);
    }
}
