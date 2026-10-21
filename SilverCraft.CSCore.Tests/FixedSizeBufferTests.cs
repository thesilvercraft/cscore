using SilverCraft.CSCore.Utils.Buffer;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class FixedSizeBufferTests
{
    [Fact]
    public void WriteAndRead_WithinCapacity_ReturnsExpectedElements()
    {
        using var buffer = new FixedSizeBuffer<int>(10);
        Assert.Equal(10, buffer.Length);
        Assert.Equal(0, buffer.Buffered);

        var data = new[] { 1, 2, 3, 4, 5 };
        var written = buffer.Write(data, 0, data.Length);
        Assert.Equal(5, written);
        Assert.Equal(5, buffer.Buffered);

        var readTarget = new int[5];
        var read = buffer.Read(readTarget, 0, 5);
        Assert.Equal(5, read);
        Assert.Equal(data, readTarget);
        Assert.Equal(0, buffer.Buffered);
    }

    [Fact]
    public void Read_WhenEmpty_ReturnsZero()
    {
        using var buffer = new FixedSizeBuffer<byte>(16);
        var output = new byte[8];
        var read = buffer.Read(output, 0, output.Length);

        Assert.Equal(0, read);
    }

    [Fact]
    public void WriteAndRead_WithOffset_HonorsArrayOffsets()
    {
        using var buffer = new FixedSizeBuffer<int>(8);
        var source = new[] { 0, 10, 20, 30, 0 };
        buffer.Write(source, 1, 3);

        var destination = new[] { 99, 99, 99, 99, 99 };
        var read = buffer.Read(destination, 1, 3);

        Assert.Equal(3, read);
        Assert.Equal(new[] { 99, 10, 20, 30, 99 }, destination);
    }

    [Fact]
    public void CircularWrapAround_ReadsAcrossBufferEndCorrectly()
    {
        using var buffer = new FixedSizeBuffer<int>(4);

        // Fill 3 elements: [1, 2, 3, _]
        buffer.Write([1, 2, 3], 0, 3);
        // Read 2 elements: [_, _, 3, _], readOffset=2, buffered=1
        var temp = new int[2];
        buffer.Read(temp, 0, 2);
        Assert.Equal(new[] { 1, 2 }, temp);
        Assert.Equal(1, buffer.Buffered);

        // Write 3 elements: wraps around to index 0 and 1 -> [5, 6, 3, 4]
        buffer.Write([4, 5, 6], 0, 3);
        Assert.Equal(4, buffer.Buffered);

        // Read all 4 elements: should read 3, 4, 5, 6 across the boundary
        var result = new int[4];
        var read = buffer.Read(result, 0, 4);

        Assert.Equal(4, read);
        Assert.Equal(new[] { 3, 4, 5, 6 }, result);
        Assert.Equal(0, buffer.Buffered);
    }

    [Fact]
    public void Overwrite_WhenWritingMoreThanCapacity_KeepsMostRecentElements()
    {
        using var buffer = new FixedSizeBuffer<int>(3);

        // Writing 5 elements in one call into a capacity-3 buffer
        buffer.Write([1, 2, 3, 4, 5], 0, 5);

        Assert.Equal(3, buffer.Buffered);

        var result = new int[3];
        var read = buffer.Read(result, 0, 3);

        Assert.Equal(3, read);
        Assert.Equal(new[] { 3, 4, 5 }, result);
    }

    [Fact]
    public void Clear_ResetsBufferState()
    {
        using var buffer = new FixedSizeBuffer<int>(5);
        buffer.Write([10, 20, 30], 0, 3);
        Assert.Equal(3, buffer.Buffered);

        buffer.Clear();
        Assert.Equal(0, buffer.Buffered);

        var output = new int[3];
        Assert.Equal(0, buffer.Read(output, 0, 3));
    }

    [Fact]
    public void ToStream_AllowsByteStreamReadAndWrite()
    {
        using var buffer = new FixedSizeBuffer<byte>(16);
        using var stream = buffer.ToStream();

        Assert.True(stream.CanRead);
        Assert.True(stream.CanWrite);

        var data = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
        stream.Write(data, 0, data.Length);
        Assert.Equal(4, buffer.Buffered);

        var readBytes = new byte[4];
        var readCount = stream.Read(readBytes, 0, 4);

        Assert.Equal(4, readCount);
        Assert.Equal(data, readBytes);
    }

    [Fact]
    public void ToStream_ForNonByte_ThrowsNotSupportedException()
    {
        using var buffer = new FixedSizeBuffer<float>(8);
        Assert.Throws<NotSupportedException>(() => buffer.ToStream());
    }
}
