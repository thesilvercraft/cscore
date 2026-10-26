using System.Runtime.InteropServices;
using SilverCraft.CSCore.DSP;
using SilverCraft.CSCore.Streams;
using SilverCraft.CSCore.Streams.Effects;
using SilverCraft.CSCore.Streams.SampleConverter;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class AudioReadOffsetTests
{
    public static IEnumerable<object[]> SampleSourceKinds =>
    [
        ["Volume"], ["Gain"], ["Pan"], ["PeakMeter"], ["MonoToStereo"], ["StereoToMono"],
        ["FadeInOut"], ["Notification"], ["SimpleNotification"], ["PitchShifter"]
    ];

    public static IEnumerable<object[]> WaveToSampleKinds =>
    [
        ["Pcm8"], ["Pcm16"], ["Pcm24"], ["Pcm32"], ["Float32"]
    ];

    public static IEnumerable<object[]> SampleToWaveKinds =>
    [
        ["Pcm8"], ["Pcm16"], ["Pcm24"], ["Pcm32"], ["Float32"]
    ];

    [Theory]
    [MemberData(nameof(SampleSourceKinds))]
    public void SampleSources_WriteOnlyTheRequestedOffsetRange(string kind)
    {
        var channels = kind is "Pan" or "PeakMeter" or "StereoToMono" ? 2 : 1;
        var samples = Enumerable.Range(0, 128).Select(i => (i % 17 - 8) / 10f).ToArray();
        var format = new WaveFormat(44100, 32, channels, AudioEncoding.IeeeFloat);

        AssertOffsetRead(
            () => CreateSampleSource(kind, new TestSampleSource(samples, format)),
            offset: 4,
            count: 8,
            sentinel: 1234f);
    }

    [Theory]
    [MemberData(nameof(WaveToSampleKinds))]
    public void WaveToSampleConverters_WriteOnlyTheRequestedOffsetRange(string kind)
    {
        var (bytes, format) = CreateWaveData(kind);
        AssertOffsetRead(
            () => CreateWaveToSampleConverter(kind, new TestWaveSource(bytes, format)),
            offset: 5,
            count: 8,
            sentinel: 1234f);
    }

    [Theory]
    [MemberData(nameof(SampleToWaveKinds))]
    public void SampleToWaveConverters_WriteOnlyTheRequestedOffsetRange(string kind)
    {
        var samples = Enumerable.Range(0, 32).Select(i => (i % 13 - 6) / 8f).ToArray();
        var sourceFormat = new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat);
        var byteCount = kind switch { "Pcm8" => 8, "Pcm16" => 16, "Pcm24" => 24, _ => 32 };

        AssertOffsetRead(
            () => CreateSampleToWaveConverter(kind, new TestSampleSource(samples, sourceFormat)),
            offset: 5,
            count: byteCount,
            sentinel: (byte)0xA5);
    }

    [Fact]
    public void PeakMeter_TracksAllSamplesReadAtANonzeroOffset()
    {
        using var source = new TestSampleSource(
            [0.1f, 0.1f, 0.2f, 0.2f, 0.3f, 0.4f, 0.9f, 0.8f],
            new WaveFormat(1000, 32, 2, AudioEncoding.IeeeFloat));
        using var meter = new PeakMeter(source);
        var output = Enumerable.Repeat(99f, 16).ToArray();

        Assert.Equal(8, meter.Read(output, 4, 8));

        Assert.Equal(new[] { 0.9f, 0.8f }, meter.ChannelPeakValues);
        Assert.Equal(new[] { 99f, 99f, 99f, 99f }, output[..4]);
        Assert.Equal(new[] { 99f, 99f, 99f, 99f }, output[12..]);
    }

    private static ISampleSource CreateSampleSource(string kind, TestSampleSource source) => kind switch
    {
        "Volume" => new VolumeSource(source) { Volume = 0.5f },
        "Gain" => new GainSource(source) { Volume = 0.5f },
#pragma warning disable CS0618
        "Pan" => new PanSource(source) { Pan = 0.25f },
#pragma warning restore CS0618
        "PeakMeter" => new PeakMeter(source),
        "MonoToStereo" => new MonoToStereoSource(source),
        "StereoToMono" => new StereoToMonoSource(source),
        "FadeInOut" => new FadeInOut(source),
        "Notification" => new NotificationSource(source) { BlockCount = 2 },
        "SimpleNotification" => new SimpleNotificationSource(source) { BlockCount = 2 },
        "PitchShifter" => new PitchShifter(source),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown sample source kind.")
    };

    private static ISampleSource CreateWaveToSampleConverter(string kind, TestWaveSource source) => kind switch
    {
        "Pcm8" => new Pcm8BitToSample(source),
        "Pcm16" => new Pcm16BitToSample(source),
        "Pcm24" => new Pcm24BitToSample(source),
        "Pcm32" => new Pcm32BitToSample(source),
        "Float32" => new IeeeFloatToSample(source),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown converter kind.")
    };

    private static IWaveSource CreateSampleToWaveConverter(string kind, TestSampleSource source) => kind switch
    {
        "Pcm8" => new SampleToPcm8(source),
        "Pcm16" => new SampleToPcm16(source),
        "Pcm24" => new SampleToPcm24(source),
        "Pcm32" => new SampleToPcm32(source),
        "Float32" => new SampleToIeeeFloat32(source),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown converter kind.")
    };

    private static (byte[] Data, WaveFormat Format) CreateWaveData(string kind)
    {
        var bits = kind switch { "Pcm8" => 8, "Pcm16" => 16, "Pcm24" => 24, _ => 32 };
        var encoding = kind == "Float32" ? AudioEncoding.IeeeFloat : AudioEncoding.Pcm;
        var format = new WaveFormat(44100, bits, 1, encoding);
        var data = new byte[format.BlockAlign * 8];
        if (kind == "Float32")
        {
            var samples = Enumerable.Range(0, 8).Select(i => (i - 4) / 8f).ToArray();
            Buffer.BlockCopy(samples, 0, data, 0, data.Length);
        }
        else
        {
            for (var i = 0; i < data.Length; i++)
                data[i] = (byte)(i * 19 + 7);
        }
        return (data, format);
    }

    private static void AssertOffsetRead<T>(Func<IReadableAudioSource<T>> createSource, int offset, int count, T sentinel)
    {
        using var referenceSource = createSource();
        var expected = new T[count];
        var expectedRead = referenceSource.Read(expected, 0, count);

        using var offsetSource = createSource();
        var actual = Enumerable.Repeat(sentinel, offset + count + 3).ToArray();
        var actualRead = offsetSource.Read(actual, offset, count);

        Assert.Equal(expectedRead, actualRead);
        Assert.Equal(expected[..expectedRead], actual[offset..(offset + actualRead)]);
        Assert.All(actual[..offset], value => Assert.Equal(sentinel, value));
        Assert.All(actual[(offset + actualRead)..], value => Assert.Equal(sentinel, value));
    }
}

