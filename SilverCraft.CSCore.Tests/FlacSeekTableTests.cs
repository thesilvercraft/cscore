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

    [Fact]
    public void FileWithoutSeekTable_SeekAcrossFramesAndLastFrame_MatchesSequentialPcmAndCleanEof()
    {
        var expected = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tone-reference.pcm"));

        using var flac = OpenFixture("tone-no-seektable.flac", FlacPreScanMode.Sync);
        Assert.True(flac.CanSeek);
        Assert.Equal(expected.Length, flac.Length);

        // Seek across multiple frames including past the final frame boundary
        var seekTargets = new[]
        {
            0L,
            expected.Length / 3 & ~3L,
            (expected.Length - 1024) & ~3L,
            (expected.Length - 64) & ~3L
        };

        var window = new byte[256];
        foreach (var target in seekTargets)
        {
            flac.Position = target;
            Assert.Equal(target, flac.Position);

            var toRead = (int)Math.Min(window.Length, expected.Length - target);
            var read = flac.Read(window, 0, window.Length);
            Assert.Equal(toRead, read);
            Assert.Equal(expected.AsSpan((int)target, toRead).ToArray(), window.AsSpan(0, read).ToArray());
        }

        // Subsequent reads at EOF return 0 without throwing EndOfStreamException
        Assert.Equal(expected.Length, flac.Position);
        Assert.Equal(0, flac.Read(window, 0, window.Length));
        Assert.Equal(expected.Length, flac.Position);
    }

    [Fact]
    public void FileWithSeekTable_SeekBeforeFirstRead_MatchesSequentialPcm()
    {
        var expected = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tone-reference.pcm"));

        using var flac = OpenFixture("tone-seektable.flac", FlacPreScanMode.None);
        var seekPos = (expected.Length / 2) & ~3;
        flac.Position = seekPos;

        var window = new byte[1024];
        var read = flac.Read(window, 0, window.Length);

        Assert.Equal(window.Length, read);
        Assert.Equal(expected.AsSpan(seekPos, read).ToArray(), window.AsSpan(0, read).ToArray());
    }

    [Fact]
    public void FileWithUndersizedMaxFrameSize_RetriesBufferingAndDecodesBitExact()
    {
        var raw = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tone-no-seektable.flac"));
        var expected = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tone-reference.pcm"));

        // Patch STREAMINFO.MaxFrameSize (bytes 15..17, 24-bit big-endian) to an undersized value (64 bytes)
        raw[15] = 0x00;
        raw[16] = 0x00;
        raw[17] = 0x40;

        using var stream = new ChunkedReadStream(raw, maxReadChunk: 128);
        using var flac = new FlacFile(stream, FlacPreScanMode.None);

        var actual = new byte[expected.Length];
        var total = 0;
        while (total < actual.Length)
        {
            var read = flac.Read(actual, total, actual.Length - total);
            if (read == 0)
                break;
            total += read;
        }

        Assert.Equal(expected.Length, total);
        Assert.Equal(expected, actual);
    }

    private sealed class ChunkedReadStream(byte[] data, int maxReadChunk) : Stream
    {
        private int _pos;
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => data.Length;
        public override long Position
        {
            get => _pos;
            set => _pos = (int)value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var toCopy = Math.Min(Math.Min(count, maxReadChunk), data.Length - _pos);
            if (toCopy <= 0)
                return 0;
            Array.Copy(data, _pos, buffer, offset, toCopy);
            _pos += toCopy;
            return toCopy;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin)
            => Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _pos + offset,
                SeekOrigin.End => data.Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
