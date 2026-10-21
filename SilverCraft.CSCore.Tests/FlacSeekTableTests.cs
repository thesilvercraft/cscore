using SilverCraft.CSCore.Codecs.FLAC;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class FlacSeekTableTests
{
    [Fact]
    public void FileWithSeekTable_DecodesAndCanSeekWithinFirstFrame()
    {
        using var flac = OpenFixture("tone-seektable.flac", FlacPreScanMode.None);
        var firstRead = new byte[128];

        Assert.True(flac.CanSeek);
        Assert.Equal(firstRead.Length, flac.Read(firstRead, 0, firstRead.Length));

        flac.Position = 32;
        var afterSeek = new byte[64];
        Assert.Equal(afterSeek.Length, flac.Read(afterSeek, 0, afterSeek.Length));

        Assert.Equal(firstRead[32..96], afterSeek);
        Assert.Equal(96, flac.Position);
    }

    [Fact]
    public void FileWithoutSeekTable_CanDecodeAndBuildSeekIndex()
    {
        using var flac = OpenFixture("tone-no-seektable.flac", FlacPreScanMode.Sync);
        var firstRead = new byte[128];

        Assert.True(flac.CanSeek);
        Assert.Equal(firstRead.Length, flac.Read(firstRead, 0, firstRead.Length));

        flac.Position = 32;
        var afterSeek = new byte[64];
        Assert.Equal(afterSeek.Length, flac.Read(afterSeek, 0, afterSeek.Length));

        Assert.Equal(firstRead[32..96], afterSeek);
        Assert.Equal(96, flac.Position);
    }

    [Fact]
    public void FileWithMalformedSeekTable_RejectsNonEntrySizedBlock()
    {
        using var stream = FixtureStream("tone-malformed-seektable.flac");

        var exception = Assert.Throws<FlacException>(() => new FlacFile(stream, FlacPreScanMode.None));

        Assert.Equal(FlacLayer.Metadata, exception.Layer);
    }

    private static FlacFile OpenFixture(string fileName, FlacPreScanMode scanMode)
        => new(FixtureStream(fileName), scanMode);

    private static MemoryStream FixtureStream(string fileName)
        => new(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName)));

    [Fact]
    public void SmallOffsetReads_MatchFfmpegPcmAndPreserveBufferGuards()
    {
        using var flac = OpenFixture("tone-seektable.flac", FlacPreScanMode.None);
        var expected = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tone-reference.pcm"));
        var actual = new byte[expected.Length];
        var requestSizes = new[] { 4, 4092, 20, 1024, 12, 8192 };
        var position = 0;
        var requestIndex = 0;

        Assert.Equal(expected.Length, flac.Length);

        while (position < actual.Length)
        {
            var count = Math.Min(requestSizes[requestIndex++ % requestSizes.Length], actual.Length - position);
            var guardedBuffer = Enumerable.Repeat((byte)0xA5, count + 3).ToArray();

            var read = flac.Read(guardedBuffer, 2, count);

            Assert.Equal(count, read);
            Assert.Equal((byte)0xA5, guardedBuffer[0]);
            Assert.Equal((byte)0xA5, guardedBuffer[1]);
            Assert.Equal((byte)0xA5, guardedBuffer[^1]);
            Array.Copy(guardedBuffer, 2, actual, position, read);
            position += read;
            Assert.Equal(position, flac.Position);
        }

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TwentyFourBitFrames_MatchFfmpegPcmAcrossOffsetReads()
    {
        using var flac = OpenFixture("tone-24bit.flac", FlacPreScanMode.None);
        var expected = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tone-24bit-reference.pcm"));
        var actual = new byte[expected.Length];
        var requestSizes = new[] { 6, 4092, 18, 1020, 8190 };
        var position = 0;
        var requestIndex = 0;

        Assert.Equal(24, flac.WaveFormat.BitsPerSample);
        Assert.Equal(expected.Length, flac.Length);

        while (position < actual.Length)
        {
            var count = Math.Min(requestSizes[requestIndex++ % requestSizes.Length], actual.Length - position);
            var guardedBuffer = Enumerable.Repeat((byte)0xA5, count + 3).ToArray();
            var read = flac.Read(guardedBuffer, 2, count);

            Assert.Equal(count, read);
            Assert.Equal((byte)0xA5, guardedBuffer[0]);
            Assert.Equal((byte)0xA5, guardedBuffer[1]);
            Assert.Equal((byte)0xA5, guardedBuffer[^1]);
            Array.Copy(guardedBuffer, 2, actual, position, read);
            position += read;
        }

        Assert.Equal(expected, actual);
    }
}
