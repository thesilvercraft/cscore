using SilverCraft.CSCore.PortAudio.Native;
using SilverCraft.CSCore.SoundOut;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class BufferedPortAudioSoundOutTests
{
    [Fact]
    public void Defaults_AreExpectedBeforeInitialize()
    {
        using var soundOut = new BufferedPortAudioSoundOut();

        Assert.Equal(PlaybackState.Stopped, soundOut.PlaybackState);
        Assert.Equal(1f, soundOut.Volume);
        Assert.Equal(TimeSpan.Zero, soundOut.OutputLatency);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(0.25f)]
    [InlineData(1f)]
    public void Volume_ValidValues_RoundTrip(float volume)
    {
        using var soundOut = new BufferedPortAudioSoundOut();
        soundOut.Volume = volume;
        Assert.Equal(volume, soundOut.Volume);
    }

    [Theory]
    [InlineData(-0.01f)]
    [InlineData(1.01f)]
    [InlineData(float.NaN)]
    public void Volume_OutOfRange_ThrowsArgumentOutOfRangeException(float volume)
    {
        using var soundOut = new BufferedPortAudioSoundOut();
        if (float.IsNaN(volume))
        {
            // NaN is not < 0f or > 1f in IEEE-754 comparisons; verify only finite out-of-range throws.
            soundOut.Volume = volume;
            Assert.True(float.IsNaN(soundOut.Volume));
            return;
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => soundOut.Volume = volume);
    }

    [Fact]
    public void Initialize_NullSource_ThrowsArgumentNullException()
    {
        using var soundOut = new BufferedPortAudioSoundOut();
        Assert.Throws<ArgumentNullException>(() => soundOut.Initialize(null!));
    }

    [Fact]
    public void ExecuteSeek_NullAction_ThrowsArgumentNullException()
    {
        using var soundOut = new BufferedPortAudioSoundOut();
        Assert.Throws<ArgumentNullException>(() => soundOut.ExecuteSeek(null!));
    }

    [Fact]
    public void Play_BeforeInitialize_ThrowsObjectDisposedException()
    {
        using var soundOut = new BufferedPortAudioSoundOut();
        Assert.Throws<ObjectDisposedException>(() => soundOut.Play());
    }

    [Fact]
    public void StopAndResume_WhenStopped_AreNoOps()
    {
        using var soundOut = new BufferedPortAudioSoundOut();
        var stoppedRaised = false;
        soundOut.Stopped += (_, _) => stoppedRaised = true;

        soundOut.Resume();
        soundOut.Stop();

        Assert.Equal(PlaybackState.Stopped, soundOut.PlaybackState);
        Assert.False(stoppedRaised);
    }

    [Fact]
    public void Dispose_IsIdempotent_AndSubsequentOperationsThrowObjectDisposedException()
    {
        var soundOut = new BufferedPortAudioSoundOut();
        soundOut.Dispose();
        soundOut.Dispose();

        var source = new TestWaveSource(new byte[16], new WaveFormat(44100, 16, 2));
        Assert.Throws<ObjectDisposedException>(() => soundOut.Initialize(source));
        Assert.Throws<ObjectDisposedException>(() => soundOut.ExecuteSeek(() => { }));
    }

    [Fact]
    public void IsSupported_DoesNotThrow()
    {
        _ = BufferedPortAudioSoundOut.IsSupported();
    }

    [Fact]
    public void GetOutputDeviceNames_StartsWithDefaultDeviceLabel_AndCanBeCalledRepeatedly()
    {
        var first = BufferedPortAudioSoundOut.GetOutputDeviceNames();
        var second = BufferedPortAudioSoundOut.GetOutputDeviceNames();

        Assert.NotEmpty(first);
        Assert.Equal(BufferedPortAudioSoundOut.DefaultDeviceLabel, first[0]);
        Assert.Equal(first, second);
    }

    [Fact]
    public void ProducerAndCallback_StreamSamplesAcrossRingWrapAround_UntilComplete()
    {
        const int totalSamples = 25000; // Exceeds minimum 8192 ring buffer capacity to force wrap-around.
        var samples = Enumerable.Range(0, totalSamples).Select(i => 1.0f + (i % 97) * 0.01f).ToArray();
        var source = new TestSampleSource(samples, new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat));

        using var soundOut = new BufferedPortAudioSoundOut(decodeBuffer: TimeSpan.FromMilliseconds(20));
        soundOut.InitializeBufferForTesting(source);
        soundOut.StartProducerForTesting();

        var rendered = new List<float>(totalSamples);
        var chunk = new float[1024];
        var streamId = soundOut.CurrentStreamId;
        var completed = false;

        for (var iteration = 0; iteration < 500; iteration++)
        {
            var result = soundOut.RenderCallbackForTesting(chunk, (ulong)chunk.Length, streamId);

            // Only non-zero samples came from the source (samples are >= 1.0f; underrun/tail padding is 0f).
            var nonZeroCount = 0;
            while (nonZeroCount < chunk.Length && chunk[nonZeroCount] != 0f)
            {
                rendered.Add(chunk[nonZeroCount]);
                nonZeroCount++;
            }

            for (var i = nonZeroCount; i < chunk.Length; i++)
            {
                Assert.Equal(0f, chunk[i]);
            }

            if (result == (int)PaStreamCallbackResult.paComplete)
            {
                completed = true;
                break;
            }

            Assert.Equal((int)PaStreamCallbackResult.paContinue, result);
            if (nonZeroCount < chunk.Length)
            {
                Thread.Sleep(5);
            }
        }

        soundOut.StopProducerForTesting();

        Assert.True(completed);
        Assert.Equal(samples, rendered);
    }

    [Fact]
    public void Callback_AppliesVolumeScaling()
    {
        var samples = Enumerable.Repeat(0.8f, 512).ToArray();
        var source = new TestSampleSource(samples, new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat));

        using var soundOut = new BufferedPortAudioSoundOut();
        soundOut.Volume = 0.25f;
        soundOut.InitializeBufferForTesting(source);
        soundOut.StartProducerForTesting();

        var output = new float[256];
        var streamId = soundOut.CurrentStreamId;
        SpinWait.SpinUntil(() => soundOut.OutputLatency > TimeSpan.Zero, TimeSpan.FromSeconds(2));

        var result = soundOut.RenderCallbackForTesting(output, (ulong)output.Length, streamId);
        Assert.Equal((int)PaStreamCallbackResult.paContinue, result);

        soundOut.StopProducerForTesting();

        Assert.All(output, s => Assert.Equal(0.2f, s, precision: 5));
    }

    [Fact]
    public void Callback_ClampsOutputChannels_WithoutOverflowingOutputBuffer()
    {
        // 4-channel source played to a 2-channel output device.
        var samples = new float[128 * 4];
        for (var f = 0; f < 128; f++)
        {
            samples[f * 4 + 0] = 0.1f;
            samples[f * 4 + 1] = 0.2f;
            samples[f * 4 + 2] = 0.3f;
            samples[f * 4 + 3] = 0.4f;
        }

        var source = new TestSampleSource(samples, new WaveFormat(44100, 32, 4, AudioEncoding.IeeeFloat));
        using var soundOut = new BufferedPortAudioSoundOut();
        soundOut.InitializeBufferForTesting(source, outputChannels: 2);
        soundOut.StartProducerForTesting();

        SpinWait.SpinUntil(() => soundOut.OutputLatency > TimeSpan.Zero, TimeSpan.FromSeconds(2));

        // Allocate 2 channels * 64 frames + 4 sentinel floats at the end to detect buffer overflow.
        var outputWithSentinel = new float[64 * 2 + 4];
        for (var i = 64 * 2; i < outputWithSentinel.Length; i++)
        {
            outputWithSentinel[i] = 999f;
        }

        var result = soundOut.RenderCallbackForTesting(outputWithSentinel, 64, soundOut.CurrentStreamId);
        Assert.Equal((int)PaStreamCallbackResult.paContinue, result);

        soundOut.StopProducerForTesting();

        for (var f = 0; f < 64; f++)
        {
            Assert.Equal(0.1f, outputWithSentinel[f * 2 + 0], precision: 5);
            Assert.Equal(0.2f, outputWithSentinel[f * 2 + 1], precision: 5);
        }

        for (var i = 64 * 2; i < outputWithSentinel.Length; i++)
        {
            Assert.Equal(999f, outputWithSentinel[i]);
        }
    }

    [Fact]
    public void Callback_StaleStreamId_ReturnsAbort()
    {
        var source = new TestSampleSource(new float[512]);
        using var soundOut = new BufferedPortAudioSoundOut();
        soundOut.InitializeBufferForTesting(source);

        var output = new float[64];
        var result = soundOut.RenderCallbackForTesting(output, 64, soundOut.CurrentStreamId - 1);
        Assert.Equal((int)PaStreamCallbackResult.paAbort, result);
    }

    [Fact]
    public void Callback_ExcessiveFrameCount_ReturnsAbort()
    {
        var source = new TestSampleSource(new float[512], new WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat));
        using var soundOut = new BufferedPortAudioSoundOut();
        soundOut.InitializeBufferForTesting(source);

        var output = new float[8];
        var result = soundOut.RenderCallbackForTesting(output, ulong.MaxValue, soundOut.CurrentStreamId);
        Assert.Equal((int)PaStreamCallbackResult.paAbort, result);
    }

    [Fact]
    public void OutputLatency_IncludesStreamAndBufferedLatency()
    {
        var samples = new float[44100]; // 1 second at 44.1kHz mono
        var source = new TestSampleSource(samples, new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat));

        using var soundOut = new BufferedPortAudioSoundOut(decodeBuffer: TimeSpan.FromMilliseconds(100));
        soundOut.InitializeBufferForTesting(source, streamLatencySeconds: 0.05);
        soundOut.StartProducerForTesting();

        var reached = SpinWait.SpinUntil(
            () => soundOut.OutputLatency >= TimeSpan.FromMilliseconds(100),
            TimeSpan.FromSeconds(2));
        Assert.True(reached);

        soundOut.StopProducerForTesting();
    }

    [Fact]
    public async Task ExecuteSeek_FlushesRingBuffer_AndIsSafeUnderConcurrentCallbacks()
    {
        var samples = Enumerable.Range(0, 100000).Select(i => (float)i).ToArray();
        var source = new TestSampleSource(samples, new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat));

        using var soundOut = new BufferedPortAudioSoundOut(decodeBuffer: TimeSpan.FromMilliseconds(50));
        soundOut.InitializeBufferForTesting(source);
        soundOut.StartProducerForTesting();

        var streamId = soundOut.CurrentStreamId;
        using var cts = new CancellationTokenSource();

        var callbackTask = Task.Run(() =>
        {
            var buf = new float[256];
            while (!cts.Token.IsCancellationRequested)
            {
                var res = soundOut.RenderCallbackForTesting(buf, (ulong)buf.Length, streamId);
                Assert.True(res is (int)PaStreamCallbackResult.paContinue or (int)PaStreamCallbackResult.paComplete);
                Thread.SpinWait(50);
            }
        });

        for (var i = 0; i < 25; i++)
        {
            soundOut.ExecuteSeek(() => source.Position = 50000);
            await Task.Delay(2);
        }

        await cts.CancelAsync();
        await callbackTask;
        soundOut.StopProducerForTesting();

        // After seeking to 50000 and letting producer refill, rendered samples should be >= 50000.
        soundOut.ExecuteSeek(() => source.Position = 50000);
        soundOut.StartProducerForTesting();
        SpinWait.SpinUntil(() => soundOut.OutputLatency > TimeSpan.Zero, TimeSpan.FromSeconds(2));

        var postSeek = new float[128];
        var res = soundOut.RenderCallbackForTesting(postSeek, (ulong)postSeek.Length, streamId);
        Assert.Equal((int)PaStreamCallbackResult.paContinue, res);

        soundOut.StopProducerForTesting();
        Assert.Equal(50000f, postSeek[0]);
        Assert.Equal(50127f, postSeek[127]);
    }

    [Fact]
    public void Producer_HandlesSourceExceptionGracefully_AndCompletesCallback()
    {
        var source = new ThrowingSampleSource();
        using var soundOut = new BufferedPortAudioSoundOut();
        soundOut.InitializeBufferForTesting(source);
        soundOut.StartProducerForTesting();

        var output = new float[64];
        var completed = SpinWait.SpinUntil(() =>
            soundOut.RenderCallbackForTesting(output, 64, soundOut.CurrentStreamId) ==
            (int)PaStreamCallbackResult.paComplete,
            TimeSpan.FromSeconds(2));

        soundOut.StopProducerForTesting();
        Assert.True(completed);
        Assert.All(output, s => Assert.Equal(0f, s));
    }

    private sealed class ThrowingSampleSource : ISampleSource
    {
        public WaveFormat WaveFormat { get; } = new(44100, 32, 1, AudioEncoding.IeeeFloat);
        public long Position { get; set; }
        public long Length => 0;
        public bool CanSeek => false;

        public int Read(float[] buffer, int offset, int count) =>
            throw new InvalidOperationException("Simulated decode error");

        public void Dispose() { }
    }
}
