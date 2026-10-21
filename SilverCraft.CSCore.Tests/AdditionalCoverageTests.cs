using System.Buffers.Binary;
using System.Text;
using SilverCraft.CSCore.Codecs.AIFF;
using SilverCraft.CSCore.Codecs.RAW;
using SilverCraft.CSCore.DSP.Resampler;
using SilverCraft.CSCore.Streams;
using SilverCraft.CSCore.Tags.ID3;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class AdditionalCoverageTests
{
    [Fact]
    public void AiffReader_ReadsBigEndianSamplesAndSeeksOnBlockBoundaries()
    {
        using var stream = new MemoryStream(CreateAiff16BitMono([0x12, 0x34, 0xAB, 0xCD]));
        using var reader = new AiffReader(stream);
        var output = new byte[4];

        Assert.Equal(4, reader.Read(output, 0, output.Length));
        Assert.Equal(new byte[] { 0x34, 0x12, 0xCD, 0xAB }, output);
        reader.Position = 1;
        Assert.Equal(0, reader.Position);
    }

    [Fact]
    public void AiffReader_RejectsMissingRequiredChunks()
    {
        using var stream = new MemoryStream([.. "FORM\0\0\0\u0004AIFF"u8]);
        Assert.Throws<AiffException>(() => new AiffReader(stream));
    }

    [Fact]
    public void RawDataReader_ReadsFromCurrentStreamPositionAndAlignsReads()
    {
        using var stream = new MemoryStream([99, 1, 2, 3, 4, 5, 6]);
        stream.Position = 1;
        using var reader = new RawDataReader(stream, new WaveFormat(44100, 16, 1));
        var output = new byte[8];

        Assert.Equal(4, reader.Read(output, 0, 5));
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, output[..4]);
        Assert.Equal(4, reader.Position);
        reader.Position = 1;
        Assert.Equal(0, reader.Position);
    }

    [Fact]
    public void RawDataReader_RejectsUnsupportedEncoding()
    {
        using var stream = new MemoryStream();
        var format = new WaveFormat(44100, 16, 2, AudioEncoding.Unknown);
        Assert.Throws<ArgumentException>(() => new RawDataReader(stream, format));
    }

    [Fact]
    public void Id3v2_ParsesTitleFrameAndRejectsTruncatedTag()
    {
        var payload = new byte[11];
        Encoding.ASCII.GetBytes("TIT2").CopyTo(payload, 0);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), 1);
        payload[10] = 0; // ISO-8859-1 text encoding
        var tagBytes = new byte[10 + payload.Length];
        Encoding.ASCII.GetBytes("ID3").CopyTo(tagBytes, 0);
        tagBytes[3] = 3;
        tagBytes[9] = (byte)payload.Length;
        payload.CopyTo(tagBytes, 10);

        using var stream = new MemoryStream(tagBytes);
        var tag = ID3v2.FromStream(stream);
        Assert.NotNull(tag);
        Assert.NotNull(tag["TIT2"]);

        using var truncated = new MemoryStream(tagBytes[..^1]);
        Assert.Null(ID3v2.FromStream(truncated));
    }

    [Fact]
    public void SimpleResampler_PreservesConstantSignalAndOutputChannelFormat()
    {
        using var source = new TestSampleSource(Enumerable.Repeat(0.25f, 32).ToArray());
        using var resampler = new SimpleResampler(source, 22050);
        var output = new float[8];

        Assert.Equal(8, resampler.Read(output, 0, output.Length));
        Assert.Equal(22050, resampler.WaveFormat.SampleRate);
        Assert.All(output, sample => Assert.Equal(0.25f, sample, 5));
    }

    [Fact]
    public void LoopStream_RepeatsShortSourceToFillRead()
    {
        using var source = new TestWaveSource([1, 2, 3, 4], new WaveFormat(44100, 16, 1));
        using var loop = new LoopStream(source);
        var output = new byte[10];

        Assert.Equal(10, loop.Read(output, 0, output.Length));
        Assert.Equal(new byte[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2 }, output);
    }

    private static byte[] CreateAiff16BitMono(byte[] pcm)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("FORM"));
        WriteUInt32BigEndian(writer, (uint)(4 + 8 + 18 + 8 + 8 + pcm.Length));
        writer.Write(Encoding.ASCII.GetBytes("AIFF"));
        writer.Write(Encoding.ASCII.GetBytes("COMM"));
        WriteUInt32BigEndian(writer, 18);
        WriteUInt16BigEndian(writer, 1);
        WriteUInt32BigEndian(writer, (uint)(pcm.Length / 2));
        WriteUInt16BigEndian(writer, 16);
        // 44100 Hz in the AIFF 80-bit extended representation.
        writer.Write(new byte[] { 0x40, 0x0E, 0xAC, 0x44, 0, 0, 0, 0, 0, 0 });
        writer.Write(Encoding.ASCII.GetBytes("SSND"));
        WriteUInt32BigEndian(writer, (uint)(8 + pcm.Length));
        WriteUInt32BigEndian(writer, 0);
        WriteUInt32BigEndian(writer, 0);
        writer.Write(pcm);
        return stream.ToArray();
    }

    private static void WriteUInt16BigEndian(BinaryWriter writer, ushort value)
    {
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
        writer.Write(bytes);
    }

    private static void WriteUInt32BigEndian(BinaryWriter writer, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        writer.Write(bytes);
    }
}

