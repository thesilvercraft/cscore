using SilverCraft.CSCore.Codecs.WAV;
using SilverCraft.CSCore.DSP.Resampler;
using SilverCraft.CSCore.Streams;
using SilverCraft.CSCore.Tags.ID3;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class ReviewRegressionTests
{
    [Fact]
    public void WaveWriter_OverwritingShorterFile_TruncatesOldTail()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cscore-{Guid.NewGuid():N}.wav");
        try
        {
            File.WriteAllBytes(path, Enumerable.Repeat((byte)0xCC, 512).ToArray());

            using (var writer = new WaveWriter(path, new WaveFormat(44100, 8, 1)))
            {
                writer.Write(new byte[] { 0x10, 0x20, 0x30 }, 0, 3);
            }

            Assert.Equal(47, new FileInfo(path).Length);
            using (var reader = new WaveFileReader(path))
            {
                Assert.Equal(3, reader.Length);
                var data = new byte[4];
                Assert.Equal(3, reader.Read(data, 0, data.Length));
                Assert.Equal(new byte[] { 0x10, 0x20, 0x30 }, data[..3]);
            }
        }
        finally
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }
    }

    [Fact]
    public void StereoToMono_Position_IsMeasuredInMonoSamples()
    {
        var source = new TestSampleSource(Enumerable.Range(0, 12).Select(i => (float)i).ToArray(),
            new WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat));
        using var mono = new StereoToMonoSource(source);

        mono.Position = 1;

        Assert.Equal(1, mono.Position);
        Assert.Equal(2, source.Position);
    }

    [Fact]
    public void MonoToStereo_Position_IsMeasuredInOutputSamples()
    {
        var source = new TestSampleSource(Enumerable.Range(0, 12).Select(i => (float)i).ToArray());
        using var stereo = new MonoToStereoSource(source);

        stereo.Position = 2;

        Assert.Equal(2, stereo.Position);
        Assert.Equal(1, source.Position);
    }

    [Fact]
    public void ID3v2_FromStream_HandlesPartialReadsOfTagPayload()
    {
        // ID3v2.3 header with a four-byte all-padding payload.
        var tag = new byte[] { 0x49, 0x44, 0x33, 3, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0 };
        using var stream = new ShortReadStream(tag, maxReadSize: 1);

        var parsed = ID3v2.FromStream(stream);

        Assert.NotNull(parsed);
        Assert.Equal(14, stream.Position);
    }

    [Fact]
    public void SimpleResampler_ReadHonorsDestinationOffset()
    {
        // Include one interpolation lookahead sample so the request is not at end-of-stream.
        using var source = new TestSampleSource([0.25f, 0.5f, 0.75f, 1f]);
        using var resampler = new SimpleResampler(source, 44100);
        var output = Enumerable.Repeat(-1f, 6).ToArray();

        var read = resampler.Read(output, 1, 4);

        Assert.Equal(4, read);
        Assert.Equal(new[] { -1f, 0.25f, 0.5f, 0.75f, 1f, -1f }, output);
    }

    [Fact]
    public void SimpleResampler_ReadAtEndReturnsZero()
    {
        using var source = new TestSampleSource([0.25f]);
        using var resampler = new SimpleResampler(source, 44100);
        var output = new float[2];
        Assert.Equal(1, resampler.Read(output, 0, 1));

        Assert.Equal(0, resampler.Read(output, 0, 1));
    }

    [Fact]
    public void WaveFileReader_SkipsPaddingAfterOddSizedChunks()
    {
        using var stream = new MemoryStream(BuildWaveWithOddSizedChunk());

        using var reader = new WaveFileReader(stream);

        var data = new byte[2];
        Assert.Equal(2, reader.Read(data, 0, data.Length));
        Assert.Equal(new byte[] { 0x34, 0x12 }, data);
    }

    private static byte[] BuildWaveWithOddSizedChunk()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write("RIFF"u8.ToArray());
        writer.Write(0); // RIFF size is not used by this reader.
        writer.Write("WAVE"u8.ToArray());
        writer.Write("JUNK"u8.ToArray());
        writer.Write(1);
        writer.Write((byte)0x7F);
        writer.Write((byte)0); // RIFF chunks are padded to an even size.
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(44100);
        writer.Write(44100);
        writer.Write((short)1);
        writer.Write((short)8);
        writer.Write("data"u8.ToArray());
        writer.Write(2);
        writer.Write((byte)0x34);
        writer.Write((byte)0x12);
        return stream.ToArray();
    }

    private sealed class ShortReadStream(byte[] data, int maxReadSize) : MemoryStream(data)
    {
        public override int Read(byte[] buffer, int offset, int count) =>
            base.Read(buffer, offset, Math.Min(count, maxReadSize));
    }
}