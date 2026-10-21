using System.Buffers.Binary;
using SilverCraft.CSCore.Streams.SampleConverter;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class SampleConverterTests
{
    [Fact]
    public void Pcm16BitToSample_ConvertsKnownPcmValuesAccurately()
    {
        // 0 -> 0.0f, 32767 -> 32767/32768, -32768 -> -1.0f
        var pcmBytes = new byte[6];
        BinaryPrimitives.WriteInt16LittleEndian(pcmBytes.AsSpan(0, 2), 0);
        BinaryPrimitives.WriteInt16LittleEndian(pcmBytes.AsSpan(2, 2), 32767);
        BinaryPrimitives.WriteInt16LittleEndian(pcmBytes.AsSpan(4, 2), -32768);

        using var waveSource = new TestWaveSource(pcmBytes, new WaveFormat(44100, 16, 1));
        using var converter = new Pcm16BitToSample(waveSource);

        var samples = new float[3];
        var read = converter.Read(samples, 0, 3);

        Assert.Equal(3, read);
        Assert.Equal(0.0f, samples[0], 4);
        Assert.Equal(32767f / 32768f, samples[1], 4);
        Assert.Equal(-1.0f, samples[2], 4);
    }

    [Fact]
    public void Pcm16BitToSample_And_SampleToPcm16_RoundTripWithSimdLength()
    {
        // 35 samples to test both SIMD chunk (>= 16 samples) and remainder loop
        var originalSamples = new float[35];
        for (var i = 0; i < originalSamples.Length; i++)
        {
            originalSamples[i] = (float)Math.Sin(i * 0.2);
        }

        using var sampleSource = new TestSampleSource(originalSamples);
        using var toWave = new SampleToPcm16(sampleSource);

        var pcmBytes = new byte[originalSamples.Length * 2];
        var bytesRead = toWave.Read(pcmBytes, 0, pcmBytes.Length);
        Assert.Equal(pcmBytes.Length, bytesRead);

        using var waveSource = new TestWaveSource(pcmBytes, toWave.WaveFormat);
        using var toSample = new Pcm16BitToSample(waveSource);

        var reconstructed = new float[originalSamples.Length];
        var samplesRead = toSample.Read(reconstructed, 0, reconstructed.Length);

        Assert.Equal(originalSamples.Length, samplesRead);
        for (var i = 0; i < originalSamples.Length; i++)
        {
            // 16-bit PCM quantization error should be within 1/32768 (~0.0001f)
            Assert.Equal(originalSamples[i], reconstructed[i], 0.001f);
        }
    }

    [Fact]
    public void SampleToPcm16_ClampsOverflowingValues()
    {
        var input = new[] { -2.5f, -1.0f, 0.0f, 1.0f, 2.5f };
        using var sampleSource = new TestSampleSource(input);
        using var toWave = new SampleToPcm16(sampleSource);

        var pcmBytes = new byte[input.Length * 2];
        var read = toWave.Read(pcmBytes, 0, pcmBytes.Length);
        Assert.Equal(pcmBytes.Length, read);

        var minVal = BinaryPrimitives.ReadInt16LittleEndian(pcmBytes.AsSpan(0, 2));
        var maxVal = BinaryPrimitives.ReadInt16LittleEndian(pcmBytes.AsSpan(8, 2));

        Assert.Equal(short.MinValue, minVal);
        Assert.Equal(short.MaxValue, maxVal);
    }

    [Fact]
    public void Pcm24BitToSample_ConvertsKnownValuesAccurately()
    {
        // 24-bit little endian: 3 bytes per sample
        // 0 -> 0x00, 0x00, 0x00
        // max 8388607 -> 0xFF, 0xFF, 0x7F
        // min -8388608 -> 0x00, 0x00, 0x80
        var bytes = new byte[]
        {
            0x00, 0x00, 0x00,
            0xFF, 0xFF, 0x7F,
            0x00, 0x00, 0x80
        };

        using var waveSource = new TestWaveSource(bytes, new WaveFormat(44100, 24, 1));
        using var converter = new Pcm24BitToSample(waveSource);

        var samples = new float[3];
        var read = converter.Read(samples, 0, 3);

        Assert.Equal(3, read);
        Assert.Equal(0.0f, samples[0], 5);
        Assert.Equal(8388607f / 8388608f, samples[1], 5);
        Assert.Equal(-1.0f, samples[2], 5);
    }

    [Fact]
    public void Pcm24Bit_RoundTripTest()
    {
        var original = new[] { 0.0f, 0.5f, -0.5f, 0.999f, -0.999f };
        using var sampleSource = new TestSampleSource(original);
        using var toWave = new SampleToPcm24(sampleSource);

        var pcm = new byte[original.Length * 3];
        var bytesRead = toWave.Read(pcm, 0, pcm.Length);
        Assert.Equal(pcm.Length, bytesRead);

        using var waveSource = new TestWaveSource(pcm, toWave.WaveFormat);
        using var toSample = new Pcm24BitToSample(waveSource);

        var reconstructed = new float[original.Length];
        var samplesRead = toSample.Read(reconstructed, 0, reconstructed.Length);

        Assert.Equal(original.Length, samplesRead);
        for (var i = 0; i < original.Length; i++)
        {
            Assert.Equal(original[i], reconstructed[i], 4);
        }
    }

    [Fact]
    public void Pcm8BitToSample_ConvertsKnownUnsignedValuesAccurately()
    {
        // 8-bit PCM is unsigned: 128 is 0, 0 is -1, 255 is ~0.992
        var pcm = new byte[] { 128, 0, 255 };
        using var waveSource = new TestWaveSource(pcm, new WaveFormat(44100, 8, 1));
        using var toSample = new Pcm8BitToSample(waveSource);

        var samples = new float[3];
        var read = toSample.Read(samples, 0, 3);

        Assert.Equal(3, read);
        Assert.Equal(0.0f, samples[0], 3);
        Assert.Equal(-1.0f, samples[1], 3);
        Assert.Equal((255f / 128f) - 1.0f, samples[2], 3);
    }

    [Fact]
    public void Pcm8Bit_RoundTripWithSimdLength()
    {
        // 40 samples to test both SIMD (>=32 bytes) and remainder
        var original = new float[40];
        for (var i = 0; i < original.Length; i++)
        {
            original[i] = (float)Math.Sin(i * 0.3) * 0.9f;
        }

        using var sampleSource = new TestSampleSource(original);
        using var toWave = new SampleToPcm8(sampleSource);

        var pcm = new byte[original.Length];
        var bytesRead = toWave.Read(pcm, 0, pcm.Length);
        Assert.Equal(pcm.Length, bytesRead);

        using var waveSource = new TestWaveSource(pcm, toWave.WaveFormat);
        using var toSample = new Pcm8BitToSample(waveSource);

        var reconstructed = new float[original.Length];
        var samplesRead = toSample.Read(reconstructed, 0, reconstructed.Length);

        Assert.Equal(original.Length, samplesRead);
        for (var i = 0; i < original.Length; i++)
        {
            // 8-bit has ~1/128 quantization error (~0.01)
            Assert.Equal(original[i], reconstructed[i], 1);
        }
    }

    [Fact]
    public void IeeeFloat_PassesSamplesThroughLosslessly()
    {
        var original = new[] { 0.123f, -0.456f, 0.789f, -0.999f };
        using var sampleSource = new TestSampleSource(original);
        using var toWave = new SampleToIeeeFloat32(sampleSource);

        var bytes = new byte[original.Length * 4];
        var bytesRead = toWave.Read(bytes, 0, bytes.Length);
        Assert.Equal(bytes.Length, bytesRead);

        using var waveSource = new TestWaveSource(bytes, toWave.WaveFormat);
        using var toSample = new IeeeFloatToSample(waveSource);

        var reconstructed = new float[original.Length];
        var read = toSample.Read(reconstructed, 0, reconstructed.Length);

        Assert.Equal(original.Length, read);
        Assert.Equal(original, reconstructed);
    }

    [Fact]
    public void Constructor_RejectsInvalidAudioEncodings()
    {
        var floatFormat = new WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat);
        using var waveSource = new TestWaveSource([], floatFormat);

        // Expects 16-bit PCM, not IEEE Float
        Assert.Throws<InvalidOperationException>(() => new Pcm16BitToSample(waveSource));
    }
    [Fact]
    public void SampleToPcm32_ConvertsVectorAndTailSamplesToLittleEndianPcm()
    {
        var input = new[] { -0.5f, -0.25f, 0f, 0.25f, 0.5f, -0.125f, 0.125f, 0.375f, -0.375f, 0.0625f, -0.0625f };
        const float scale = 2147483520.0f;
        using var source = new TestSampleSource(input);
        using var converter = new SampleToPcm32(source);
        var output = new byte[input.Length * sizeof(int)];

        Assert.Equal(output.Length, converter.Read(output, 0, output.Length));

        for (var i = 0; i < input.Length; i++)
        {
            var actual = BinaryPrimitives.ReadInt32LittleEndian(output.AsSpan(i * sizeof(int), sizeof(int)));
            Assert.Equal((int)(input[i] * scale), actual);
        }
    }

    [Fact]
    public void Pcm32BitToSample_ConvertsLittleEndianPcmAndHandlesTailSamples()
    {
        var values = new[] { int.MinValue, -1073741824, 0, 1073741824, int.MaxValue, -1, 1, 123456789, -123456789 };
        var bytes = new byte[values.Length * sizeof(int)];
        for (var i = 0; i < values.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * sizeof(int), sizeof(int)), values[i]);

        using var source = new TestWaveSource(bytes, new WaveFormat(44100, 32, 1));
        using var converter = new Pcm32BitToSample(source);
        var output = new float[values.Length];

        Assert.Equal(output.Length, converter.Read(output, 0, output.Length));
        for (var i = 0; i < values.Length; i++)
            Assert.Equal(values[i] / 2147483648f, output[i], 6);
    }
}


