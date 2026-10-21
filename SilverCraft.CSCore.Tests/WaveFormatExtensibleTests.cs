using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class WaveFormatExtensibleTests
{
    [Fact]
    public void Constructor_WithValidChannelMask_InitializesCorrectly()
    {
        // 6 channels for 5.1
        var format = new WaveFormatExtensible(
            48000,
            24,
            6,
            AudioSubTypes.Pcm,
            ChannelMasks.FiveDotOneWithRearMask);

        Assert.Equal(48000, format.SampleRate);
        Assert.Equal(24, format.BitsPerSample);
        Assert.Equal(6, format.Channels);
        Assert.Equal(ChannelMasks.FiveDotOneWithRearMask, format.ChannelMask);
        Assert.Equal(AudioSubTypes.Pcm, format.SubFormat);
    }

    [Fact]
    public void Constructor_MismatchedChannelMask_ThrowsArgumentException()
    {
        // 2 channels declared, but stereo mask is 2 flags -> this should pass
        _ = new WaveFormatExtensible(44100, 16, 2, AudioSubTypes.Pcm, ChannelMasks.StereoMask);

        // 2 channels declared, but 5.1 mask (6 flags) -> should throw
        Assert.Throws<ArgumentException>(() =>
            new WaveFormatExtensible(44100, 16, 2, AudioSubTypes.Pcm, ChannelMasks.FiveDotOneWithRearMask));
    }

    [Fact]
    public void SubTypeFromWaveFormat_ReturnsCorrectGuids()
    {
        var standardPcm = new WaveFormat(44100, 16, 2, AudioEncoding.Pcm);
        Assert.Equal(AudioSubTypes.Pcm, WaveFormatExtensible.SubTypeFromWaveFormat(standardPcm));

        var standardFloat = new WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat);
        Assert.Equal(AudioSubTypes.IeeeFloat, WaveFormatExtensible.SubTypeFromWaveFormat(standardFloat));

        var extensible = new WaveFormatExtensible(48000, 24, 2, AudioSubTypes.Pcm, ChannelMasks.StereoMask);
        Assert.Equal(AudioSubTypes.Pcm, WaveFormatExtensible.SubTypeFromWaveFormat(extensible));
    }

    [Fact]
    public void ToWaveFormat_ConvertsBackToStandardWaveFormat()
    {
        var extensible = new WaveFormatExtensible(44100, 16, 2, AudioSubTypes.Pcm, ChannelMasks.StereoMask);
        var standard = extensible.ToWaveFormat();

        Assert.Equal(44100, standard.SampleRate);
        Assert.Equal(16, standard.BitsPerSample);
        Assert.Equal(2, standard.Channels);
        Assert.Equal(AudioEncoding.Pcm, standard.WaveFormatTag);
    }

    [Fact]
    public void ValidBitsPerSample_InitializesToBits()
    {
        var format = new WaveFormatExtensible(48000, 24, 2, AudioSubTypes.Pcm, ChannelMasks.StereoMask);

        Assert.Equal(24, format.BitsPerSample);
        Assert.Equal(24, format.ValidBitsPerSample);
    }
}
