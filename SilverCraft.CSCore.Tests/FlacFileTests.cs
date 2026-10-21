using System.Buffers.Binary;
using SilverCraft.CSCore.Codecs.FLAC;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class FlacFileTests
{
    [Fact]
    public void MetadataOnlyStream_ExposesStreamInfoAndHonorsAlignedEmptyReads()
    {
        using var stream = new MemoryStream(CreateEmptyFlacStreamInfo());
        using var flac = new FlacFile(stream, FlacPreScanMode.None);
        var output = Enumerable.Repeat((byte)0xCC, 16).ToArray();

        var read = flac.Read(output, 3, 7);

        Assert.Equal(new WaveFormat(44100, 16, 2), flac.WaveFormat);
        Assert.Equal(0, flac.Length);
        Assert.Equal(0, flac.Position);
        Assert.Equal(0, read);
        Assert.All(output, value => Assert.Equal((byte)0xCC, value));
    }

    [Fact]
    public void Constructor_RejectsMissingFlacSync()
    {
        using var stream = new MemoryStream([0, 1, 2, 3]);

        Assert.Throws<FlacException>(() => new FlacFile(stream, FlacPreScanMode.None));
    }

    private static byte[] CreateEmptyFlacStreamInfo()
    {
        // fLaC + last STREAMINFO metadata block (34 bytes).
        var bytes = new byte[4 + 4 + 34];
        "fLaC"u8.CopyTo(bytes);
        bytes[4] = 0x80; // last metadata block, STREAMINFO type
        bytes[7] = 34;
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(8, 2), 4096);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(10, 2), 4096);
        var streamInfo = ((ulong)44100 << 44) | ((ulong)1 << 41) | ((ulong)15 << 36);
        BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(18, 8), streamInfo);
        return bytes;
    }
}
