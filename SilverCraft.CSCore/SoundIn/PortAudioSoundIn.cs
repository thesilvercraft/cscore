using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SilverCraft.CSCore.PortAudio;
using SilverCraft.CSCore.PortAudio.Native;

namespace SilverCraft.CSCore.SoundIn;

public class PortAudioSoundIn : ISoundIn
{
    public const string DefaultDeviceLabel = PortAudioHelper.DefaultDeviceLabel;

    private readonly ILogger? _log;
    private readonly Lock _streamLock = new();

    private StreamCallBack? _callback;
    private PaStreamFinishedCallback? _finishedCallback;
    private unsafe PaStream* _stream;

    private int _currentStreamId;
    private bool _isPAInit;
    private bool _isDisposed;

    private byte[]? _buffer;

    public PortAudioSoundIn(int channels = 1, int sampleRate = 44100, string? preferredDevice = null)
    {
        Channels = channels;
        SampleRate = sampleRate;
        PreferredDevice = preferredDevice;
        _log = LogLocation.GetLogger<PortAudioSoundIn>();
    }

    public string? PreferredDevice
    {
        get;
        set
        {
            if (RecordingState != RecordingState.Stopped)
                throw new InvalidOperationException("Cannot change preferred device while recording.");
            field = value;
        }
    }

    public static bool IsSupported() => PortAudioHelper.IsSupported();

    public static string[] GetInputDeviceNames() =>
        PortAudioHelper.GetDeviceNames(isOutput: false, LogLocation.GetLogger<PortAudioSoundIn>());

    public int Channels
    {
        get;
        set
        {
            if (RecordingState != RecordingState.Stopped)
                throw new InvalidOperationException("Cannot change channels while recording.");
            field = value;
        }
    }

    public int SampleRate
    {
        get;
        set
        {
            if (RecordingState != RecordingState.Stopped)
                throw new InvalidOperationException("Cannot change sample rate while recording.");
            field = value;
        }
    }

    public WaveFormat WaveFormat { get; private set; } = null!;
    public RecordingState RecordingState { get; private set; } = RecordingState.Stopped;

    public event EventHandler<DataAvailableEventArgs>? DataAvailable;
    public event EventHandler<RecordingStoppedEventArgs>? Stopped;

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        unsafe
        {
            if (!_isPAInit)
            {
                PortAudioLifetimeManager.Initialize();
                _isPAInit = true;
            }

            CloseActiveStreamInternal();

            _callback = CallBackMethod;
            _finishedCallback = OnStreamFinished;

            var newStreamId = Interlocked.Increment(ref _currentStreamId);
            var deviceIndex = PortAudioHelper.ResolveDeviceIndex(PreferredDevice, isOutput: false, _log);
            var deviceInfo = NativeMethods.Pa_GetDeviceInfo(deviceIndex);
            if (deviceInfo == null)
            {
                throw new PortAudioException($"PortAudio failed to get info for input device index {deviceIndex}");
            }

            var inputParams = new PaStreamParameters
            {
                device = deviceIndex,
                channelCount = Channels,
                sampleFormat = NativeMethods.paFloat32,
                suggestedLatency = deviceInfo->defaultHighInputLatency,
                hostApiSpecificStreamInfo = null
            };

            var callbackPtr = (delegate* unmanaged[Cdecl]<void*, void*, ulong, PaStreamCallbackTimeInfo*, nuint, void*, int>)
                Marshal.GetFunctionPointerForDelegate(_callback);

            PaStream* paStream;
            PortAudioHelper.ThrowIfError(NativeMethods.Pa_OpenStream(
                &paStream,
                &inputParams,
                null,
                SampleRate,
                0,
                NativeMethods.paNoFlag,
                callbackPtr,
                (void*)(nint)newStreamId));

            try
            {
                PortAudioHelper.ThrowIfError(NativeMethods.Pa_SetStreamFinishedCallback(paStream, _finishedCallback));
            }
            catch
            {
                _ = NativeMethods.Pa_CloseStream(paStream);
                throw;
            }

            WaveFormat = new WaveFormat(SampleRate, 32, Channels, AudioEncoding.IeeeFloat);
            // PortAudio uses an unspecified callback size. Reserve one second of audio up front
            // so ordinary callback sizes never allocate on the real-time thread.
            _buffer = new byte[checked(SampleRate * Channels * sizeof(float))];

            lock (_streamLock)
            {
                _stream = paStream;
            }
        }
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        unsafe
        {
            lock (_streamLock)
            {
                if (_stream == null)
                    throw new InvalidOperationException("SoundIn object is not initialized.");

                if (RecordingState == RecordingState.Recording)
                    return;

                PortAudioHelper.ThrowIfError(NativeMethods.Pa_StartStream(_stream));
                RecordingState = RecordingState.Recording;
            }
        }
    }

    public void Stop()
    {
        if (RecordingState == RecordingState.Stopped)
            return;

        RecordingState = RecordingState.Stopped;
        CloseActiveStreamInternal();
    }

    private unsafe void CloseActiveStreamInternal()
    {
        PaStream* s;
        lock (_streamLock)
        {
            if (_stream == null)
                return;

            s = _stream;
            _stream = null;
        }

        PortAudioHelper.AbortAndCloseStream(s, _log);
    }

    private unsafe int CallBackMethod(
        void* input,
        void* output,
        ulong frameCount,
        PaStreamCallbackTimeInfo* timeInfo,
        PaStreamCallbackFlags statusFlags,
        nint userData)
    {
        try
        {
            var callbackStreamId = (int)userData;
            if (callbackStreamId != Volatile.Read(ref _currentStreamId))
            {
                return (int)PaStreamCallbackResult.paAbort;
            }

            if (input == null || frameCount == 0)
            {
                return (int)PaStreamCallbackResult.paContinue;
            }

            if (frameCount > (ulong)(int.MaxValue / Channels))
                return (int)PaStreamCallbackResult.paAbort;
            var totalSamples = (int)frameCount * Channels;
            var byteCount = totalSamples * sizeof(float);

            if (_buffer == null || _buffer.Length < byteCount)
            {
                _log?.LogError("PortAudio input callback exceeded the preallocated buffer ({ByteCount} bytes)",
                    byteCount);
                return (int)PaStreamCallbackResult.paAbort;
            }

            var inputSpan = new ReadOnlySpan<byte>(input, byteCount);
            inputSpan.CopyTo(_buffer);

            DataAvailable?.Invoke(this, new DataAvailableEventArgs(_buffer, 0, byteCount, WaveFormat));

            return (int)PaStreamCallbackResult.paContinue;
        }
        catch (Exception e)
        {
            _log?.LogError(e, "ERROR IN THE PORTAUDIO INPUT CALLBACK THREAD");
            return (int)PaStreamCallbackResult.paAbort;
        }
    }

    public unsafe void OnStreamFinished(void* userData)
    {
        var callbackStreamId = (int)(nint)userData;
        if (callbackStreamId != Volatile.Read(ref _currentStreamId))
        {
            return;
        }

        RecordingState = RecordingState.Stopped;
        Stopped?.Invoke(this, new RecordingStoppedEventArgs());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        Stop();

        if (!_isPAInit) return;
        PortAudioLifetimeManager.Terminate(_log);
        _isPAInit = false;
    }

    ~PortAudioSoundIn()
    {
        Dispose(false);
    }
}