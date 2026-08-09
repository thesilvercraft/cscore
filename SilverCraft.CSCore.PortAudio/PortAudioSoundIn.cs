using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Serilog;
using SilverCraft.CSCore.PortAudio.Native;
using SilverCraft.CSCore.SoundIn;

namespace SilverCraft.CSCore.PortAudio;

public class PortAudioSoundIn : ISoundIn
{
    private readonly ILogger? _log;
    private readonly object _streamLock = new();

    private StreamCallBack? _callback;
    private PaStreamFinishedCallback? _finishedCallback;
    private unsafe PaStream* _stream;

    private int _currentStreamId;
    private bool _isPAInit;
    private bool _isDisposed;

    private byte[]? _buffer;

    public PortAudioSoundIn(int channels = 1, int sampleRate = 44100)
    {
        Channels = channels;
        SampleRate = sampleRate;
        _log = LogLocation.GetLogger(typeof(PortAudioSoundIn));
    }

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

            PaStream* paStream;
            
            ThrowIfError(NativeMethods.Pa_OpenDefaultStream(
                &paStream,
                Channels,                   
                0,                          
                NativeMethods.paFloat32,   
                SampleRate,
                0,                          
                _callback,
                (nint)newStreamId));

            ThrowIfError(NativeMethods.Pa_SetStreamFinishedCallback(paStream, _finishedCallback));

            WaveFormat = new WaveFormat(SampleRate, 32, Channels, AudioEncoding.IeeeFloat);

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

                ThrowIfError(NativeMethods.Pa_StartStream(_stream));
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

        NativeMethods.Pa_AbortStream(s);

        var closeResult = NativeMethods.Pa_CloseStream(s);
        if (closeResult != (int)PaErrorCode.paNoError)
        {
            _log?.Error("PORTAUDIO ERROR: Pa_CloseStream failed with code {CloseResult}", closeResult);
        }
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

            var totalSamples = (int)frameCount * Channels;
            var byteCount = totalSamples * sizeof(float);

            if (_buffer == null || _buffer.Length < byteCount)
            {
                _buffer = new byte[byteCount];
            }

            var inputSpan = new ReadOnlySpan<byte>(input, byteCount);
            inputSpan.CopyTo(_buffer);

            DataAvailable?.Invoke(this, new DataAvailableEventArgs(_buffer, 0, byteCount, WaveFormat));

            return (int)PaStreamCallbackResult.paContinue;
        }
        catch (Exception e)
        {
            _log?.Error(e, "ERROR IN THE PORTAUDIO INPUT CALLBACK THREAD");
            return (int)PaStreamCallbackResult.paAbort;
        }
    }

    public unsafe void OnStreamFinished(void* userData)
    {
        int callbackStreamId = (int)(nint)userData;
        if (callbackStreamId != Volatile.Read(ref _currentStreamId))
        {
            return;
        }

        RecordingState = RecordingState.Stopped;
        Stopped?.Invoke(this, new RecordingStoppedEventArgs());
    }

    private static unsafe void ThrowIfError(
        int err,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int num = 0)
    {
        if (err != (int)PaErrorCode.paNoError)
        {
            var errorText = Marshal.PtrToStringAnsi((nint)NativeMethods.Pa_GetErrorText(err));
            throw new PortAudioException(
                $"PortAudio failed with code {err} ({errorText}) in {callerName} [{path}:{num}]");
        }
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

        if (_isPAInit)
        {
            PortAudioLifetimeManager.Terminate(_log);
            _isPAInit = false;
        }
    }

    ~PortAudioSoundIn()
    {
        Dispose(false);
    }
}