using SilverCraft.CSCore.Codecs.WAV;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class WaveFileTests
{
    [Fact]
    public void WaveWriterAndReader_RoundTrip_Pcm16()
    {
        var format = new WaveFormat(44100, 16, 2);
        var sampleData = new byte[44100 * 4]; // 1 second of stereo 16-bit audio
        for (var i = 0; i < sampleData.Length; i++)
        {
            sampleData[i] = (byte)(i % 256);
        }

        using var memoryStream = new MemoryStream();
        using (var writer = new WaveWriter(memoryStream, format))
        {
            writer.Write(sampleData, 0, sampleData.Length);
        }

        memoryStream.Position = 0;

        using var reader = new WaveFileReader(memoryStream);
        Assert.Equal(format, reader.WaveFormat);
        Assert.Equal(sampleData.Length, reader.Length);
        Assert.Equal(0, reader.Position);

        var readBuffer = new byte[sampleData.Length];
        var totalRead = 0;
        while (totalRead < readBuffer.Length)
        {
            var read = reader.Read(readBuffer, totalRead, readBuffer.Length - totalRead);
            if (read == 0) break;
            totalRead += read;
        }

        Assert.Equal(sampleData.Length, totalRead);
        Assert.Equal(sampleData, readBuffer);
    }

    [Fact]
    public void WaveFileReader_Seeking_ReadsFromTargetPosition()
    {
        var format = new WaveFormat(44100, 16, 1);
        var sampleData = new byte[1000];
        for (var i = 0; i < sampleData.Length; i++)
        {
            sampleData[i] = (byte)(i & 0xFF);
        }

        using var memoryStream = new MemoryStream();
        using (var writer = new WaveWriter(memoryStream, format))
        {
            writer.Write(sampleData, 0, sampleData.Length);
        }

        memoryStream.Position = 0;

        using var reader = new WaveFileReader(memoryStream);
        Assert.True(reader.CanSeek);

        // Seek to byte 200 (block-aligned to 2 bytes)
        reader.Position = 200;
        Assert.Equal(200, reader.Position);

        var buffer = new byte[100];
        var read = reader.Read(buffer, 0, 100);
        Assert.Equal(100, read);

        var expectedSlice = sampleData.AsSpan(200, 100).ToArray();
        Assert.Equal(expectedSlice, buffer);
    }

    [Fact]
    public void WaveFileReader_ThrowsOnInvalidRiffStream()
    {
        var invalidData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        using var stream = new MemoryStream(invalidData);

        Assert.Throws<EndOfStreamException>(() => new WaveFileReader(stream));
    }
}
