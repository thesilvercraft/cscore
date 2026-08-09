using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Serilog;
using SilverCraft.CSCore.PortAudio.Native;
using SilverCraft.CSCore.SoundOut;

namespace SilverCraft.CSCore.PortAudio;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int StreamCallBack(void* input, void* output, ulong frameCount,
    PaStreamCallbackTimeInfo* timeInfo, PaStreamCallbackFlags statusFlags, nint userData);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void PaStreamFinishedCallback(void* userData);

//https://www.portaudio.com/docs/v19-doxydocs/writing_a_callback.html
public class PortAudioSoundOut : ISoundOut
{
    private readonly object streamLock = new();
    private StreamCallBack? _callback;
    private PaStreamFinishedCallback? _finishedCallback;
    private readonly ILogger? _log;
    private float[]? buffer = new float[2];
    private int channels;
    private bool isDisposed;
    private bool isPAInit;
    private unsafe PaStream* stream;
    private int _currentStreamId;

    static PortAudioSoundOut()
    {
        try
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, NativeMethods.DllImportResolver);
        }
        catch (InvalidOperationException)
        {
        }
    }

    public PortAudioSoundOut()
    {
        _log = LogLocation.GetLogger(typeof(PortAudioSoundOut));
    }

    private ISampleSource? SampleSource { get; set; }
    private float _volume = 1.0f;

    public float Volume
    {
        get => Volatile.Read(ref _volume);
        set => Volatile.Write(ref _volume, value);
    }

    public IWaveSource? WaveSource { get; set; }
    public PlaybackState PlaybackState { get; set; } = PlaybackState.Stopped;

    public event EventHandler<PlaybackStoppedEventArgs>? Stopped;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public unsafe void Initialize(IWaveSource source)
    {
        DebugTrace();

        if (!isPAInit)
        {
            PortAudioLifetimeManager.Initialize();
            isPAInit = true;
        }

        CloseActiveStreamInternal();

        if (source != WaveSource) WaveSource?.Dispose();
        SampleSource?.Dispose();

        WaveSource = source;
        SampleSource = source.ToSampleSource();
        channels = SampleSource.WaveFormat.Channels;

        var initialBufferLength = Math.Max(4096 * channels, 8192);
        buffer = new float[initialBufferLength];

        _callback = CallBackMethod;
        _finishedCallback = OnStreamFinished;

        int newStreamId = Interlocked.Increment(ref _currentStreamId);

        PaStream* paStream;
        ThrowIfError(NativeMethods.Pa_OpenDefaultStream(
            &paStream, 0,
            SampleSource.WaveFormat.Channels,
            NativeMethods.paFloat32,
            SampleSource.WaveFormat.SampleRate,
            0, _callback, newStreamId));

        ThrowIfError(NativeMethods.Pa_SetStreamFinishedCallback(paStream, _finishedCallback));

        lock (streamLock)
        {
            stream = paStream;
            PlaybackState = PlaybackState.Stopped;
        }
    }

    public unsafe void Play()
    {
        DebugTrace();
        if (PlaybackState == PlaybackState.Playing) return;

        PaStream* s;
        lock (streamLock)
        {
            ObjectDisposedException.ThrowIf(stream == null, nameof(stream));
            PlaybackState = PlaybackState.Playing;
            s = stream;
        }

        ThrowIfError(NativeMethods.Pa_StartStream(s));
    }

    public void Resume()
    {
        if (PlaybackState != PlaybackState.Paused) return;
        Play();
    }

    public unsafe void Pause()
    {
        DebugTrace();
        if (PlaybackState != PlaybackState.Playing) return;

        PaStream* s;
        lock (streamLock)
        {
            ObjectDisposedException.ThrowIf(stream == null, nameof(stream));
            PlaybackState = PlaybackState.Paused;
            s = stream;
        }

        ThrowIfError(NativeMethods.Pa_StopStream(s));
    }

    public void Stop()
    {
        if (PlaybackState == PlaybackState.Stopped) return;
        DebugTrace();

        CloseActiveStreamInternal();
        Stopped?.Invoke(this, new PlaybackStoppedEventArgs());
    }

    public unsafe void OnStreamFinished(void* userData)
    {
        int callbackStreamId = (int)(nint)userData;

        if (callbackStreamId != Volatile.Read(ref _currentStreamId))
        {
            DebugTrace($"Ignoring finished callback for obsolete stream ID {callbackStreamId}");
            return;
        }

        if (PlaybackState != PlaybackState.Playing)
        {
            DebugTrace($"Ignoring stream finished callback because state is {PlaybackState}");
            return;
        }

        if (PlaybackState == PlaybackState.Stopped) return;
        Task.Run(OnStop);
    }

    private void OnStop()
    {
        if (PlaybackState == PlaybackState.Stopped) return;
        DebugTrace();

        CloseActiveStreamInternal();
        Stopped?.Invoke(this, new PlaybackStoppedEventArgs());
    }

    /// <summary>
    /// Safely aborts and closes the active PortAudio stream without locking up native callback joins.
    /// </summary>
    private unsafe void CloseActiveStreamInternal()
    {
        PaStream* s;
        lock (streamLock)
        {
            if (stream == null)
            {
                PlaybackState = PlaybackState.Stopped;
                return;
            }

            s = stream;
            stream = null;
            PlaybackState = PlaybackState.Stopped;
        }

        NativeMethods.Pa_AbortStream(s);

        var closeResult = NativeMethods.Pa_CloseStream(s);
        if (closeResult != (int)PaErrorCode.paNoError)
        {
            _log?.Error("PORTAUDIO ERROR: Pa_CloseStream failed with code {CloseResult}", closeResult);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed) return;
        isDisposed = true;

        DebugTrace();
        CloseActiveStreamInternal();

        if (disposing)
        {
            SampleSource?.Dispose();
            WaveSource?.Dispose();
            buffer = null;
        }

        _callback = null;
        _finishedCallback = null;

        if (isPAInit)
        {
            PortAudioLifetimeManager.Terminate(_log);
            isPAInit = false;
        }
    }

    ~PortAudioSoundOut()
    {
        Dispose(false);
    }

    public static bool IsSupported()
    {
        try
        {
            _ = NativeMethods.Pa_GetVersion();
            return true;
        }
        catch
        {
            return false;
        }
    }

    [Conditional("DEBUG")]
    private static void DebugTrace(string message = null, [CallerMemberName] string method = null)
    {
        Debug.WriteLine($"PORTAUDIO {method} {message}");
    }

    private static unsafe void ThrowIfError(int err, [CallerMemberName] string callerName = "",
        [CallerFilePath] string path = "", [CallerLineNumber] int num = 0)
    {
        if (err != (int)PaErrorCode.paNoError)
            throw new PortAudioException(
                $"PortAudio failed, error code {err} ({Marshal.PtrToStringAnsi((nint)NativeMethods.Pa_GetErrorText(err))}) in method {callerName} in {path}:{num}");
    }

    private unsafe int CallBackMethod(void* input, void* output, ulong frameCount, PaStreamCallbackTimeInfo* timeInfo,
        PaStreamCallbackFlags statusFlags, nint userData)
    {
        try
        {
            int callbackStreamId = (int)userData;
            if (callbackStreamId != Volatile.Read(ref _currentStreamId))
            {
                return (int)PaStreamCallbackResult.paAbort;
            }

            var localSampleSource = SampleSource;
            if (localSampleSource == null)
            {
                return (int)PaStreamCallbackResult.paAbort;
            }

            var outputBuffer = (float*)output;
            if (frameCount > (ulong)(int.MaxValue / channels))
            {
                _log?.Error("Unusually large frameCount in callback method {frameCount}", frameCount);
                return (int)PaStreamCallbackResult.paAbort;
            }
            var bufferLength = (int)frameCount * channels;

            if (buffer == null || buffer.Length < bufferLength)
            {
                new Span<float>(outputBuffer, bufferLength).Clear();
                Array.Resize(ref buffer, bufferLength);
                return (int)PaStreamCallbackResult.paContinue;
            }

            var read = localSampleSource.Read(buffer, 0, bufferLength);
            if (read > bufferLength)
            {
                read=bufferLength;
            }
            else if (read < 0)
            {
                read = 0;
            }

            fixed (float* bufferStart = buffer)
            {
                var bufferI = bufferStart;
                var bufferEnd = bufferStart + read;
                var outputI = outputBuffer;
                var currentVolume = Volume;

                if (currentVolume == 1.0f)
                {
                    Unsafe.CopyBlockUnaligned(outputI, bufferI, (uint)(read * sizeof(float)));
                    outputI += read;
                    bufferI += read; // Advance pointer so common zero-fill block handles any underruns
                }
                else if (currentVolume == 0.0f)
                {
                    Unsafe.InitBlockUnaligned(outputI, 0, (uint)(read * sizeof(float)));
                    outputI += read;
                    bufferI += read;
                }
                else if (Avx.IsSupported)
                {
                    var vol = Vector256.Create(currentVolume);
                    while (bufferI + 8 <= bufferEnd)
                    {
                        var floatInput =
                            Avx.LoadVector256(bufferI); //NOT LoadAlignedVector256, our buffer is not aligned
                        var result = Avx.Multiply(floatInput, vol);
                        Avx.Store(outputI, result);
                        bufferI += 8;
                        outputI += 8;
                    }
                }
                else if (Sse.IsSupported)
                {
                    var vol128 = Vector128.Create(currentVolume);
                    while (bufferI + 4 <= bufferEnd)
                    {
                        var floatInput =
                            Sse.LoadVector128(bufferI); //NOT LoadAlignedVector128, our buffer is not aligned
                        var result = Sse.Multiply(floatInput, vol128);
                        Sse.Store(outputI, result);
                        bufferI += 4;
                        outputI += 4;
                    }
                }

                // Handle remaining samples (scalar fallback)
                while (bufferI < bufferEnd) *outputI++ = *bufferI++ * currentVolume;
                // do not produce noise if uninitialized
                var remainingSamples = bufferLength - (int)(outputI - outputBuffer);
                if (remainingSamples > 0)
                    Unsafe.InitBlockUnaligned(outputI, 0, (uint)(remainingSamples * sizeof(float)));
            }

            return read > 0
                ? (int)PaStreamCallbackResult.paContinue
                : (int)PaStreamCallbackResult.paComplete;
        }
        catch (Exception e)
        {
            _log?.Error(e, "ERROR IN THE CALLBACK THREAD");
            return (int)PaStreamCallbackResult.paAbort;
        }
    }
}