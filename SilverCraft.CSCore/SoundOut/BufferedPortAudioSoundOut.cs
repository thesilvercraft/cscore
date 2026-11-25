using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SilverCraft.CSCore;
using SilverCraft.CSCore.PortAudio;
using SilverCraft.CSCore.PortAudio.Native;
using SilverCraft.CSCore.SoundOut;

namespace SilverCraft.CSCore.SoundOut;

/// <summary>
/// PortAudio <see cref="ISoundOut"/> implementation that decouples decoding from the real-time
/// audio callback using an SPSC ring buffer and opens PortAudio via <c>Pa_OpenStream</c>
/// with configurable output device and latency.
/// </summary>
public class BufferedPortAudioSoundOut : ISoundOut
{
    public const string DefaultDeviceLabel = PortAudioHelper.DefaultDeviceLabel;

    private static readonly ILogger? _log = LogLocation.GetLogger<BufferedPortAudioSoundOut>();
    private readonly Lock _streamLock = new();
    private readonly Lock _sourceReadLock = new();
    private readonly string? _preferredDevice;
    private readonly double _targetLatencySeconds;
    private readonly int _decodeBufferMs;

    private StreamCallBack? _callback;
    private PaStreamFinishedCallback? _finishedCallback;
    private unsafe PaStream* _stream;
    private int _currentStreamId;
    private bool _isDisposed;
    private bool _isPaInit;

    private ISampleSource? _sampleSource;
    private int _channels;
    private int _outputChannels;
    private int _sampleRate;
    private float _volume = 1f;
    private double _streamOutputLatencySeconds;

    // SPSC ring buffer (power-of-two capacity)
    private float[] _ringBuffer = [];
    private int _ringMask;
    private long _writePos;
    private long _readPos;
    private volatile bool _endOfStream;
    private int _isFlushing;
    private int _inCallback;

    private Thread? _producerThread;
    private CancellationTokenSource? _producerCts;
    private readonly AutoResetEvent _spaceAvailable = new(false);

    public BufferedPortAudioSoundOut(
        string? preferredDevice = null,
        TimeSpan? targetLatency = null,
        TimeSpan? decodeBuffer = null)
    {
        _preferredDevice = preferredDevice;
        _targetLatencySeconds = Math.Clamp((targetLatency ?? TimeSpan.FromMilliseconds(50)).TotalSeconds, 0.01, 1.0);
        _decodeBufferMs = Math.Clamp((int)(decodeBuffer ?? TimeSpan.FromMilliseconds(100)).TotalMilliseconds, 20, 1000);
    }

    public float Volume
    {
        get => Volatile.Read(ref _volume);
        set
        {
            if (value is < 0f or > 1f) throw new ArgumentOutOfRangeException(nameof(value));
            Volatile.Write(ref _volume, value);
        }
    }

    public IWaveSource WaveSource { get; private set; } = null!;
    public PlaybackState PlaybackState { get; private set; } = PlaybackState.Stopped;

    public event EventHandler<PlaybackStoppedEventArgs>? Stopped;

    /// <summary>
    /// Total unplayed audio latency currently buffered in the ring buffer and PortAudio stream.
    /// </summary>
    public TimeSpan OutputLatency
    {
        get
        {
            if (_sampleRate <= 0 || _channels <= 0 || Volatile.Read(ref _isFlushing) != 0)
            {
                return TimeSpan.FromSeconds(_streamOutputLatencySeconds);
            }

            var bufferedSamples = Math.Max(0L, Volatile.Read(ref _writePos) - Volatile.Read(ref _readPos));
            var bufferedSeconds = (double)bufferedSamples / (_sampleRate * _channels);
            return TimeSpan.FromSeconds(bufferedSeconds + _streamOutputLatencySeconds);
        }
    }

    public static bool IsSupported() => PortAudioHelper.IsSupported();

    /// <summary>
    /// Enumerates available PortAudio output devices formatted as <c>DeviceName (HostApiName)</c>,
    /// prefixed with <see cref="DefaultDeviceLabel"/>.
    /// </summary>
    public static string[] GetOutputDeviceNames() => PortAudioHelper.GetDeviceNames(isOutput: true, _log);

    public unsafe void Initialize(IWaveSource source)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(source);

        if (!_isPaInit)
        {
            PortAudioLifetimeManager.Initialize();
            _isPaInit = true;
        }

        CloseActiveStreamInternal();

        if (source != WaveSource) WaveSource?.Dispose();
        _sampleSource?.Dispose();

        WaveSource = source;
        _sampleSource = source.ToSampleSource();
        _channels = _sampleSource.WaveFormat.Channels;
        _sampleRate = _sampleSource.WaveFormat.SampleRate;

        AllocateRingBuffer(_sampleRate, _channels);

        _callback = CallBackMethod;
        _finishedCallback = OnStreamFinished;

        var newStreamId = Interlocked.Increment(ref _currentStreamId);
        var deviceIndex = PortAudioHelper.ResolveDeviceIndex(_preferredDevice, isOutput: true, _log);
        var deviceInfo = NativeMethods.Pa_GetDeviceInfo(deviceIndex);
        if (deviceInfo == null)
        {
            throw new PortAudioException($"PortAudio failed to get info for output device index {deviceIndex}");
        }

        var suggestedLatency = Math.Max(deviceInfo->defaultHighOutputLatency, _targetLatencySeconds);
        var outputChannels = _channels;
        if (deviceInfo->maxOutputChannels > 0 && outputChannels > deviceInfo->maxOutputChannels)
        {
            outputChannels = deviceInfo->maxOutputChannels;
        }
        _outputChannels = outputChannels;

        var outputParams = new PaStreamParameters
        {
            device = deviceIndex,
            channelCount = outputChannels,
            sampleFormat = NativeMethods.paFloat32,
            suggestedLatency = suggestedLatency,
            hostApiSpecificStreamInfo = null
        };

        var callbackPtr = (delegate* unmanaged[Cdecl]<void*, void*, ulong, PaStreamCallbackTimeInfo*, nuint, void*, int>)
            Marshal.GetFunctionPointerForDelegate(_callback);

        var framesPerBuffer = (nuint)Math.Clamp((long)(_sampleRate * suggestedLatency / 2), 512, 4096);

        PaStream* paStream;
        PortAudioHelper.ThrowIfError(NativeMethods.Pa_OpenStream(
            &paStream,
            null,
            &outputParams,
            _sampleRate,
            framesPerBuffer,
            NativeMethods.paNoFlag,
            callbackPtr,
            (void*)(nint)newStreamId));

        try
        {
            PortAudioHelper.ThrowIfError(NativeMethods.Pa_SetStreamFinishedCallback(paStream, _finishedCallback));

            var streamInfo = NativeMethods.Pa_GetStreamInfo(paStream);
            _streamOutputLatencySeconds = streamInfo != null ? streamInfo->outputLatency : suggestedLatency;
        }
        catch
        {
            _ = NativeMethods.Pa_CloseStream(paStream);
            throw;
        }

        lock (_streamLock)
        {
            _stream = paStream;
            PlaybackState = PlaybackState.Stopped;
        }

        StartProducerThread();
    }

    private void AllocateRingBuffer(int sampleRate, int channels)
    {
        // Allocate power-of-two ring buffer sized to _decodeBufferMs
        var minSamples = Math.Max((int)((long)sampleRate * channels * _decodeBufferMs / 1000), 8192);
        var capacity = 1;
        while (capacity < minSamples) capacity <<= 1;
        _ringBuffer = new float[capacity];
        _ringMask = capacity - 1;
        _writePos = 0;
        _readPos = 0;
        _endOfStream = false;
        Volatile.Write(ref _isFlushing, 0);
    }

    internal int CurrentStreamId => Volatile.Read(ref _currentStreamId);

    internal void InitializeBufferForTesting(ISampleSource sampleSource, int outputChannels = 0, double streamLatencySeconds = 0)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(sampleSource);

        StopProducerThread();
        _sampleSource?.Dispose();

        _sampleSource = sampleSource;
        _channels = sampleSource.WaveFormat.Channels;
        _outputChannels = outputChannels > 0 ? Math.Min(outputChannels, _channels) : _channels;
        _sampleRate = sampleSource.WaveFormat.SampleRate;
        _streamOutputLatencySeconds = streamLatencySeconds;

        Interlocked.Increment(ref _currentStreamId);
        AllocateRingBuffer(_sampleRate, _channels);
    }

    internal void StartProducerForTesting() => StartProducerThread();

    internal void StopProducerForTesting() => StopProducerThread();

    internal unsafe int RenderCallbackForTesting(Span<float> output, ulong frameCount, int streamId)
    {
        fixed (float* ptr = output)
        {
            return CallBackMethod(null, ptr, frameCount, null, default, streamId);
        }
    }

    /// <summary>
    /// Executes a seek operation serialized against the decode producer thread and flushes
    /// stale samples from the ring buffer.
    /// </summary>
    public void ExecuteSeek(Action seekAction)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(seekAction);
        Volatile.Write(ref _isFlushing, 1);

        var spin = new SpinWait();
        while (Volatile.Read(ref _inCallback) != 0)
        {
            spin.SpinOnce();
        }

        try
        {
            lock (_sourceReadLock)
            {
                seekAction();
                Volatile.Write(ref _readPos, 0);
                Volatile.Write(ref _writePos, 0);
                _endOfStream = false;
            }
        }
        finally
        {
            Volatile.Write(ref _isFlushing, 0);
            try
            {
                _spaceAvailable.Set();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private void StartProducerThread()
    {
        StopProducerThread();

        _producerCts = new CancellationTokenSource();
        var token = _producerCts.Token;
        _producerThread = new Thread(() => ProducerLoop(token))
        {
            Name = "SAP PortAudio Decode Buffer",
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal
        };
        _producerThread.Start();
    }

    private void StopProducerThread()
    {
        if (_producerCts != null)
        {
            _producerCts.Cancel();
            _spaceAvailable.Set();
        }

        if (_producerThread != null && _producerThread != Thread.CurrentThread)
        {
            _producerThread.Join(TimeSpan.FromSeconds(2));
        }

        _producerCts?.Dispose();
        _producerCts = null;
        _producerThread = null;
    }

    private void ProducerLoop(CancellationToken token)
    {
        var chunkSize = Math.Max(_channels * 1024, 2048);
        var tempBuffer = new float[chunkSize];

        while (!token.IsCancellationRequested)
        {
            if (Volatile.Read(ref _isFlushing) != 0 || _endOfStream)
            {
                WaitForSpace(15);
                continue;
            }

            var capacity = _ringBuffer.Length;
            if (capacity == 0)
            {
                WaitForSpace(15);
                continue;
            }

            var writePos = Volatile.Read(ref _writePos);
            var readPos = Volatile.Read(ref _readPos);
            var freeSpace = capacity - (int)(writePos - readPos);

            if (freeSpace <= 0)
            {
                WaitForSpace(10);
                continue;
            }

            var toRead = Math.Min(freeSpace, tempBuffer.Length);
            int read;
            lock (_sourceReadLock)
            {
                if (token.IsCancellationRequested || Volatile.Read(ref _isFlushing) != 0)
                {
                    continue;
                }

                var src = _sampleSource;
                if (src == null)
                {
                    _endOfStream = true;
                    continue;
                }

                try
                {
                    read = src.Read(tempBuffer, 0, toRead);
                }
                catch (ObjectDisposedException)
                {
                    _endOfStream = true;
                    continue;
                }
                catch (Exception ex)
                {
                    _log?.LogError(ex, "Error decoding audio in BufferedPortAudioSoundOut producer");
                    _endOfStream = true;
                    continue;
                }
            }

            if (read <= 0)
            {
                _endOfStream = true;
                continue;
            }

            // Copy decoded floats into circular ring buffer (SPSC: only producer advances _writePos)
            var idx = (int)(writePos & _ringMask);
            var firstPart = Math.Min(read, capacity - idx);
            Array.Copy(tempBuffer, 0, _ringBuffer, idx, firstPart);
            if (read > firstPart)
            {
                Array.Copy(tempBuffer, firstPart, _ringBuffer, 0, read - firstPart);
            }

            Volatile.Write(ref _writePos, writePos + read);
        }
    }

    private void WaitForSpace(int timeoutMs)
    {
        try
        {
            _spaceAvailable.WaitOne(timeoutMs);
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public unsafe void Play()
    {
        if (PlaybackState == PlaybackState.Playing) return;

        PaStream* s;
        PlaybackState previousState;
        lock (_streamLock)
        {
            ObjectDisposedException.ThrowIf(_stream == null, this);
            previousState = PlaybackState;
            PlaybackState = PlaybackState.Playing;
            s = _stream;
        }

        try
        {
            PortAudioHelper.ThrowIfError(NativeMethods.Pa_StartStream(s));
        }
        catch
        {
            lock (_streamLock)
            {
                if (_stream == s && PlaybackState == PlaybackState.Playing)
                {
                    PlaybackState = previousState;
                }
            }
            throw;
        }
    }

    public void Resume()
    {
        if (PlaybackState != PlaybackState.Paused) return;
        Play();
    }

    public unsafe void Pause()
    {
        if (PlaybackState != PlaybackState.Playing) return;

        PaStream* s;
        lock (_streamLock)
        {
            ObjectDisposedException.ThrowIf(_stream == null, this);
            PlaybackState = PlaybackState.Paused;
            s = _stream;
        }

        try
        {
            PortAudioHelper.ThrowIfError(NativeMethods.Pa_StopStream(s));
        }
        catch
        {
            lock (_streamLock)
            {
                if (_stream == s && PlaybackState == PlaybackState.Paused)
                {
                    PlaybackState = PlaybackState.Playing;
                }
            }
            throw;
        }
    }

    public void Stop()
    {
        if (CloseActiveStreamInternal())
        {
            Stopped?.Invoke(this, new PlaybackStoppedEventArgs());
        }
    }

    private unsafe void OnStreamFinished(void* userData)
    {
        var callbackStreamId = (int)(nint)userData;
        if (callbackStreamId != Volatile.Read(ref _currentStreamId)) return;
        if (PlaybackState != PlaybackState.Playing) return;

        Task.Run(OnStop);
    }

    private void OnStop()
    {
        if (CloseActiveStreamInternal())
        {
            Stopped?.Invoke(this, new PlaybackStoppedEventArgs());
        }
    }

    private unsafe bool CloseActiveStreamInternal()
    {
        PaStream* s;
        bool transitionedFromActive;
        lock (_streamLock)
        {
            transitionedFromActive = PlaybackState != PlaybackState.Stopped;
            if (_stream == null)
            {
                PlaybackState = PlaybackState.Stopped;
                StopProducerThread();
                return transitionedFromActive;
            }

            s = _stream;
            _stream = null;
            PlaybackState = PlaybackState.Stopped;
        }

        PortAudioHelper.AbortAndCloseStream(s, _log);

        StopProducerThread();
        return transitionedFromActive;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        _isDisposed = true;

        CloseActiveStreamInternal();

        if (disposing)
        {
            lock (_sourceReadLock)
            {
                _sampleSource?.Dispose();
                _sampleSource = null;
                WaveSource?.Dispose();
                WaveSource = null!;
            }

            _ringBuffer = [];
            _spaceAvailable.Dispose();
        }

        _callback = null;
        _finishedCallback = null;

        if (!_isPaInit) return;
        PortAudioLifetimeManager.Terminate(_log);
        _isPaInit = false;
    }

    ~BufferedPortAudioSoundOut()
    {
        Dispose(false);
    }

    private unsafe int CallBackMethod(void* input, void* output, ulong frameCount, PaStreamCallbackTimeInfo* timeInfo,
        PaStreamCallbackFlags statusFlags, nint userData)
    {
        Interlocked.Increment(ref _inCallback);
        try
        {
            var callbackStreamId = (int)userData;
            if (callbackStreamId != Volatile.Read(ref _currentStreamId))
            {
                return (int)PaStreamCallbackResult.paAbort;
            }

            var outputChannels = _outputChannels > 0 ? _outputChannels : _channels;
            var outputBuffer = (float*)output;
            if (frameCount > (ulong)(int.MaxValue / Math.Max(Math.Max(_channels, outputChannels), 1)))
            {
                return (int)PaStreamCallbackResult.paAbort;
            }

            var framesRequested = (int)frameCount;
            var outputSamplesRequested = framesRequested * outputChannels;

            if (Volatile.Read(ref _isFlushing) != 0 || _ringBuffer.Length == 0 || _channels <= 0)
            {
                new Span<float>(outputBuffer, outputSamplesRequested).Clear();
                return (int)PaStreamCallbackResult.paContinue;
            }

            var writePos = Volatile.Read(ref _writePos);
            var readPos = Volatile.Read(ref _readPos);
            var available = Math.Max(0, (int)(writePos - readPos));
            var vol = Volatile.Read(ref _volume);
            var capacity = _ringBuffer.Length;

            if (outputChannels == _channels)
            {
                var toRead = Math.Min(available, outputSamplesRequested);

                if (toRead > 0)
                {
                    var idx = (int)(readPos & _ringMask);
                    var firstPart = Math.Min(toRead, capacity - idx);

                    fixed (float* ringPtr = _ringBuffer)
                    {
                        Unsafe.CopyBlockUnaligned(outputBuffer, ringPtr + idx, (uint)(firstPart * sizeof(float)));
                        if (toRead > firstPart)
                        {
                            Unsafe.CopyBlockUnaligned(outputBuffer + firstPart, ringPtr,
                                (uint)((toRead - firstPart) * sizeof(float)));
                        }
                    }

                    if (vol != 1f)
                    {
                        for (var i = 0; i < toRead; i++)
                        {
                            outputBuffer[i] *= vol;
                        }
                    }

                    Volatile.Write(ref _readPos, readPos + toRead);
                    _spaceAvailable.Set();
                }

                if (toRead < outputSamplesRequested)
                {
                    new Span<float>(outputBuffer + toRead, outputSamplesRequested - toRead).Clear();
                    if (_endOfStream && available == toRead)
                    {
                        return (int)PaStreamCallbackResult.paComplete;
                    }
                }

                return (int)PaStreamCallbackResult.paContinue;
            }

            // Output device has fewer channels than source: copy the first outputChannels per frame.
            var availableFrames = available / _channels;
            var framesToRead = Math.Min(availableFrames, framesRequested);
            var samplesConsumed = framesToRead * _channels;
            var samplesWritten = framesToRead * outputChannels;

            if (framesToRead > 0)
            {
                for (var f = 0; f < framesToRead; f++)
                {
                    var srcBase = (int)((readPos + f * _channels) & _ringMask);
                    var dstBase = f * outputChannels;
                    for (var ch = 0; ch < outputChannels; ch++)
                    {
                        var sample = _ringBuffer[(srcBase + ch) & _ringMask];
                        outputBuffer[dstBase + ch] = vol == 1f ? sample : sample * vol;
                    }
                }

                Volatile.Write(ref _readPos, readPos + samplesConsumed);
                _spaceAvailable.Set();
            }

            if (samplesWritten < outputSamplesRequested)
            {
                new Span<float>(outputBuffer + samplesWritten, outputSamplesRequested - samplesWritten).Clear();
                if (_endOfStream && availableFrames == framesToRead)
                {
                    return (int)PaStreamCallbackResult.paComplete;
                }
            }

            return (int)PaStreamCallbackResult.paContinue;
        }
        catch (Exception ex)
        {
            _log?.LogError(ex, "Error in BufferedPortAudioSoundOut callback thread");
            return (int)PaStreamCallbackResult.paAbort;
        }
        finally
        {
            Interlocked.Decrement(ref _inCallback);
        }
    }
}
