using SilverCraft.CSCore.Codecs.WAV;
using SilverCraft.CSCore.Streams;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class BugReproTests
{
    [Fact]
    public void BufferSource_DoesNotLeaveStaleBytes_WhenOffsetIsNonZero()
    {
        var sourceData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var source = new TestWaveSource(sourceData, new WaveFormat(44100, 8, 1));

        using var bufferSource = new BufferSource(source, 16);

        var destination = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA };
        var read = bufferSource.Read(destination, 1, 2);

        Assert.Equal(2, read);
        Assert.Equal(0xAA, destination[0]); // untouched prefix
        Assert.Equal(sourceData[0], destination[1]); // first byte read
        Assert.Equal(sourceData[1], destination[2]); // second byte read
        Assert.Equal(0xAA, destination[3]); // untouched suffix
    }

    [Fact]
    public void BufferSource_ZerosUnfilledTail_WhenOffsetIsNonZeroAndReadIsShort()
    {
        var sourceData = new byte[] { 1, 2, 3, 4 };
        var source = new TestWaveSource(sourceData, new WaveFormat(44100, 8, 1));

        using var bufferSource = new BufferSource(source, 16);

        // Only 4 bytes available, but we request 8 starting at offset 2 -> tail must be zeroed, not stale.
        var destination = new byte[12];
        for (var i = 0; i < destination.Length; i++) destination[i] = 0xAA;
        var read = bufferSource.Read(destination, 2, 8);

        Assert.Equal(4, read);
        Assert.Equal(sourceData[0], destination[2]);
        Assert.Equal(sourceData[3], destination[5]);
        // indices 6..9 belong to the requested range [2,10) but are unfilled -> must be zeroed
        Assert.Equal(0, destination[8]);
    }

    [Fact]
    public void CachedSoundSource_PositionAfterCache_StartsAtZero()
    {
        var sourceData = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();
        var source = new TestWaveSource(sourceData, new WaveFormat(44100, 8, 1));
        source.Position = 40;

        using var cached = new CachedSoundSource(source);

        Assert.Equal(0, cached.Position);

        var buffer = new byte[5];
        var read = cached.Read(buffer, 0, buffer.Length);
        Assert.Equal(5, read);
        Assert.Equal(sourceData[0], buffer[0]); // must start at beginning of cache
        Assert.Equal(sourceData[4], buffer[4]);
    }

    [Fact]
    public void WaveWriter_EncodePcm8_SilenceMapsToMidpointByte()
    {
        var format = new WaveFormat(44100, 8, 1);
        using var stream = new MemoryStream();
        using var writer = new WaveWriter(stream, format);

        writer.WriteSample(0f);   // silence -> 0x80
        writer.WriteSample(1f);   // max   -> 0xFF
        writer.WriteSample(-1f);  // min   -> 0x00

        stream.Position = 44; // skip header
        var bytes = new byte[3];
        Assert.Equal(3, stream.Read(bytes, 0, 3));

        Assert.Equal(0x80, bytes[0]);
        Assert.Equal(0xFF, bytes[1]);
        Assert.Equal(0x00, bytes[2]);
    }

    [Fact]
    public void WaveWriter_EncodeExtensibleFloat32_PreservesFractionalSample()
    {
        var format = new WaveFormatExtensible(44100, 32, 1, AudioSubTypes.IeeeFloat);
        using var stream = new MemoryStream();
        using var writer = new WaveWriter(stream, format);

        writer.WriteSample(0.5f);

        stream.Position = 44; // skip header
        var bytes = new byte[4];
        Assert.Equal(4, stream.Read(bytes, 0, 4));

        var expected = BitConverter.GetBytes(0.5f);
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void WaveAggregatorBase_AlignsCountToBlockAlign_NotJustChannels()
    {
        // 16-bit stereo: BlockAlign = 4, Channels = 2
        var format = new WaveFormat(44100, 16, 2);
        var sourceData = new byte[100];
        for (var i = 0; i < sourceData.Length; i++)
            sourceData[i] = (byte)i;
        var source = new TestWaveSource(sourceData, format);

        using var aggregator = new LoopStream(source);

        // Request 6 bytes -> must be rounded down to 4 (one full frame), not 6 (3 channels-worth).
        var buffer = new byte[8];
        var read = aggregator.Read(buffer, 0, 6);

        Assert.Equal(4, read);
    }
}
