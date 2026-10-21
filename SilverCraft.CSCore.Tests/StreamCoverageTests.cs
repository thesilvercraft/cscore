using System.Reflection;
using SilverCraft.CSCore.DSP;
using SilverCraft.CSCore.Streams;
using SilverCraft.CSCore.Streams.Effects;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class StreamCoverageTests
{
    [Fact]
    public void BufferSource_RejectsNonPositiveAndUnalignedBufferSizes()
    {
        using var source = new TestWaveSource(new byte[8], new WaveFormat(44100, 16, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BufferSource(source, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BufferSource(source, 3));
    }

    [Fact]
    public void CachedSoundSource_CachesDataAndSupportsAlignedSeeking()
    {
        using var source = new TestWaveSource([1, 2, 3, 4, 5, 6], new WaveFormat(44100, 16, 1));
        using var cached = new CachedSoundSource(source);
        var output = new byte[4];

        Assert.Equal(6, cached.Length);
        Assert.Equal(4, cached.Read(output, 0, 4));
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, output);
        cached.Position = 3;
        Assert.Equal(2, cached.Position);
        Assert.Equal(2, cached.Read(output, 0, 2));
        Assert.Equal(new byte[] { 3, 4 }, output[..2]);
    }

    [Fact]
    public void ChannelConversionSource_UsesMatrixToDownmixStereo()
    {
        using var source = new TestSampleSource([1f, 0f, 0f, 1f], new WaveFormat(1000, 32, 2, AudioEncoding.IeeeFloat));
        var type = typeof(ChannelMatrix).Assembly.GetType("SilverCraft.CSCore.Streams.ChannelConversionSource", throwOnError: true)!;
        var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, [typeof(ISampleSource), typeof(ChannelMatrix)])!;
        using var converted = Assert.IsAssignableFrom<ISampleSource>(constructor.Invoke([source, ChannelMatrix.StereoToMonoMatrix]));
        var output = new float[2];

        Assert.Equal(2, converted.Read(output, 0, output.Length));
        Assert.Equal(new[] { 0.5f, 0.5f }, output);
        Assert.Equal(1, converted.WaveFormat.Channels);
    }

    [Fact]
    public void PanSource_PansToOneChannelAndValidatesStereoInput()
    {
#pragma warning disable CS0618
        using var source = new TestSampleSource([0.75f, -0.5f], new WaveFormat(1000, 32, 2, AudioEncoding.IeeeFloat));
        using var pan = new PanSource(source) { Pan = 1f };
        var output = new float[2];
        Assert.Equal(2, pan.Read(output, 0, 2));
        Assert.Equal(new[] { 0.75f, 0f }, output);

        using var mono = new TestSampleSource([0.2f]);
        Assert.Throws<ArgumentException>(() => new PanSource(mono));
#pragma warning restore CS0618
    }

    [Fact]
    public void FadeInOut_UsesLinearStrategyAndRaisesCompletionEvent()
    {
        using var source = new TestSampleSource(Enumerable.Repeat(1f, 6).ToArray(), new WaveFormat(4, 32, 1, AudioEncoding.IeeeFloat));
        using var fade = new FadeInOut(source);
        var strategy = new LinearFadeStrategy();
        var finished = 0;
        strategy.FadingFinished += (_, _) => finished++;
        fade.FadeStrategy = strategy;
        strategy.StartFading(0f, 1f, TimeSpan.FromSeconds(1));
        var output = new float[6];

        Assert.Equal(6, fade.Read(output, 0, output.Length));
        Assert.Equal(new[] { 0f, 0.25f, 0.5f, 0.75f, 1f, 1f }, output);
        Assert.False(strategy.IsFading);
        Assert.Equal(1, finished);
    }

    [Fact]
    public void LinearFadeStrategy_ValidatesConfigurationAndFadeRange()
    {
        var strategy = new LinearFadeStrategy();
        Assert.Throws<InvalidOperationException>(() => strategy.StartFading(null, 0.5f, 100));
        strategy.SampleRate = 44100;
        strategy.Channels = 2;
        Assert.Throws<ArgumentOutOfRangeException>(() => strategy.StartFading(null, 1.1f, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => strategy.StartFading(-0.1f, 1f, 100));
    }

    [Fact]
    public void NotificationSource_RaisesEventsInWholeMultichannelBlocks()
    {
        using var source = new TestSampleSource([1f, 2f, 3f, 4f, 5f, 6f], new WaveFormat(1000, 32, 2, AudioEncoding.IeeeFloat));
        using var notifications = new NotificationSource(source) { BlockCount = 2 };
        var blocks = new List<float[]>();
        notifications.BlockRead += (_, args) => blocks.Add(args.Data);
        var output = new float[6];

        Assert.Equal(6, notifications.Read(output, 0, output.Length));
        Assert.Single(blocks);
        Assert.Equal(new[] { 1f, 2f, 3f, 4f }, blocks[0]);
        Assert.Equal(2, notifications.Interval);
    }

    [Fact]
    public void PeakMeter_ReportsPerChannelPeaksAndResetsAfterInterval()
    {
        using var source = new TestSampleSource([-.2f, .8f, .7f, -.4f], new WaveFormat(1000, 32, 2, AudioEncoding.IeeeFloat));
        using var meter = new PeakMeter(source);
#pragma warning disable CS0618
        meter.BlocksToProcess = 2;
#pragma warning restore CS0618
        PeakEventArgs? reported = null;
        meter.PeakCalculated += (_, args) => reported = new PeakEventArgs(args.ChannelPeakValues.ToArray(), args.PeakValue);
        var output = new float[4];

        Assert.Equal(4, meter.Read(output, 0, 4));
        Assert.NotNull(reported);
        Assert.Equal(new[] { 0.7f, 0.8f }, reported.ChannelPeakValues);
        Assert.Equal(0.75f, reported.PeakValue, 5);
        Assert.Equal(new[] { 0f, 0f }, meter.ChannelPeakValues);
        Assert.Throws<ArgumentOutOfRangeException>(() => meter.Read(new float[3], 0, 3));
    }

    [Fact]
    public void EventArguments_ExposePayloadAndValidateLengths()
    {
        var block = new BlockReadEventArgs<int>([1, 2], 2);
        Assert.Equal(2, block.Length);
        Assert.Equal(new[] { 1, 2 }, block.Data);
        Assert.Throws<ArgumentOutOfRangeException>(() => new BlockReadEventArgs<int>([], -1));
        Assert.Throws<ArgumentNullException>(() => new BlockReadEventArgs<int>(null!, 0));

        var peak = new PeakEventArgs([0.25f, 0.75f], 0.5f);
        Assert.Equal(0.5f, peak.PeakValue);
        Assert.Throws<ArgumentException>(() => new PeakEventArgs([], 0));
    }

    [Fact]
    public void BufferSource_ReadsBufferedDataAndTracksPosition()
    {
        using var source = new TestWaveSource([1, 2, 3, 4, 5, 6, 7, 8], new WaveFormat(44100, 16, 1));
        using var buffered = new BufferSource(source, 16);
        var output = new byte[8];

        Assert.Equal(8, buffered.Read(output, 0, output.Length));
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, output);
        Assert.Equal(8, buffered.Position);
    }}



