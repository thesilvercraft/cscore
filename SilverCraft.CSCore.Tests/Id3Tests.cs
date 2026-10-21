using System.Text;
using SilverCraft.CSCore.Tags.ID3;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class Id3Tests
{
    [Fact]
    public void ID3v1_FromStream_ParsesValidTag()
    {
        var tagBytes = new byte[128];
        var pos = 0;

        // "TAG" header (3 bytes)
        Encoding.ASCII.GetBytes("TAG").CopyTo(tagBytes, pos);
        pos += 3;

        // Title (30 bytes)
        Encoding.ASCII.GetBytes("Test Song").CopyTo(tagBytes, pos);
        pos += 30;

        // Artist (30 bytes)
        Encoding.ASCII.GetBytes("Test Artist").CopyTo(tagBytes, pos);
        pos += 30;

        // Album (30 bytes)
        Encoding.ASCII.GetBytes("Test Album").CopyTo(tagBytes, pos);
        pos += 30;

        // Year (4 bytes)
        Encoding.ASCII.GetBytes("2026").CopyTo(tagBytes, pos);
        pos += 4;

        // Comment (30 bytes)
        Encoding.ASCII.GetBytes("Sample Comment").CopyTo(tagBytes, pos);
        pos += 30;

        // Genre (1 byte)
        tagBytes[pos] = (byte)ID3Genre.Rock;

        using var stream = new MemoryStream(tagBytes);
        var tag = ID3v1.FromStream(stream);

        Assert.NotNull(tag);
        Assert.Equal("Test Song", tag.Title);
        Assert.Equal("Test Artist", tag.Artist);
        Assert.Equal("Test Album", tag.Album);
        Assert.Equal(2026, tag.Year);
        Assert.Equal("Sample Comment", tag.Comment);
        Assert.Equal(ID3Genre.Rock, tag.Genre);
    }

    [Fact]
    public void ID3v1_FromStream_ReturnsNullWhenNoTagHeader()
    {
        var emptyData = new byte[128];
        using var stream = new MemoryStream(emptyData);

        var tag = ID3v1.FromStream(stream);
        Assert.Null(tag);
    }

    [Fact]
    public void ID3v1_FromStream_ThrowsWhenStreamNull()
    {
        Assert.Throws<ArgumentNullException>(() => ID3v1.FromStream(null!));
    }
}
