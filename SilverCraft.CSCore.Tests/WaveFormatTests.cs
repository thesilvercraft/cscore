using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class WaveFormatTests
{
    [Theory]
    [InlineData(44100, 16, 2, 4, 176400)]
    [InlineData(48000, 24, 2, 6, 288000)]
    [InlineData(22050, 8, 1, 1, 22050)]
    public void PcmFormat_DerivesBlockAndByteRates(int rate, int bits, int channels, int blockAlign, int bytesPerSecond)
    {
        var format = new WaveFormat(rate, bits, channels);

        Assert.Equal(blockAlign, format.BlockAlign);
        Assert.Equal(bytesPerSecond, format.BytesPerSecond);
        Assert.Equal(bits / 8 * channels, format.BytesPerBlock);
    }

    [Theory]
    [InlineData(0, 16, 2, "sampleRate")]
    [InlineData(44100, -1, 2, "bits")]
    [InlineData(44100, 16, 0, "channels")]
    public void Constructor_RejectsInvalidDimensions(int rate, int bits, int channels, string parameter)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new WaveFormat(rate, bits, channels));

        Assert.Equal(parameter, exception.ParamName);
    }

    [Fact]
    public void DurationConversions_RoundDownToWholeBlocks()
    {
        var format = new WaveFormat(44100, 16, 2);

        Assert.Equal(176, format.MillisecondsToBytes(1));
        Assert.Equal(176396 / 176400.0 * 1000, format.BytesToMilliseconds(176399), 10);
    }

    [Fact]
    public void CloneAndEquality_PreserveFormatValues()
    {
        var original = new WaveFormat(48000, 24, 2, AudioEncoding.Pcm, 4);
        var clone = Assert.IsType<WaveFormat>(original.Clone());

        Assert.NotSame(original, clone);
        Assert.Equal(original, clone);
    }
}



