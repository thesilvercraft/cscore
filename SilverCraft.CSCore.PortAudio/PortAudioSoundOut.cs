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

        _callback = CallBackMethod;
        _finishedCallback = OnStreamFinished;
        if (source != WaveSource) WaveSource?.Dispose();
        SampleSource?.Dispose();
        WaveSource = source;
        SampleSource = source.ToSampleSource();
        channels = SampleSource.WaveFormat.Channels;

        var initialBufferLength = Math.Max(4096 * channels, 8192);
        buffer = new float[initialBufferLength];

        PaStream* oldStream = null;
        lock (streamLock)
        {
            if (stream != null)
            {
                oldStream = stream;
                stream = null;
                PlaybackState = PlaybackState.Stopped;
            }
        }

        if (oldStream != null)
        {
            NativeMethods.Pa_AbortStream(oldStream);
            NativeMethods.Pa_CloseStream(oldStream);
        }

        PaStream* paStream;
        ThrowIfError(NativeMethods.Pa_OpenDefaultStream(
            &paStream, 0,
            SampleSource.WaveFormat.Channels,
            NativeMethods.paFloat32,
            SampleSource.WaveFormat.SampleRate,
            0, _callback, 0));
        ThrowIfError(NativeMethods.Pa_SetStreamFinishedCallback(paStream, _finishedCallback));

        lock (streamLock)
        {
            stream = paStream;
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

    public unsafe void Stop()
    {
        if (PlaybackState == PlaybackState.Stopped) return;
        DebugTrace();

        if (PlaybackState == PlaybackState.Paused)
        {
            lock (streamLock)
            {
                PlaybackState = PlaybackState.Stopped;
            }
            Task.Run(OnStop);
            return;
        }

        PaStream* s;
        lock (streamLock)
        {
            ObjectDisposedException.ThrowIf(stream == null, typeof(PaStream));
            s = stream;
        }
        ThrowIfError(NativeMethods.Pa_AbortStream(s));
    }


    public unsafe void OnStreamFinished(void* userData)
    {
        if (PlaybackState == PlaybackState.Stopped) return;
        Task.Run(OnStop);
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

    protected virtual void Dispose(bool disposing)
    {
        unsafe
        {
            DebugTrace();

            if (isDisposed) return;
            isDisposed = true;

            PaStream* s;
            lock (streamLock)
            {
                s = stream;
                stream = null;
                PlaybackState = PlaybackState.Stopped;
            }

            if (s != null)
            {
                NativeMethods.Pa_AbortStream(s);
            }

            if (disposing) buffer = null;
            _callback = null;

            if (!isPAInit) return;
            PortAudioLifetimeManager.Terminate(_log);
            isPAInit = false;
        }
    }


    ~PortAudioSoundOut()
    {
        Dispose(false);
    }

   
    private void OnStop()
    {
        unsafe
        {
            if (PlaybackState == PlaybackState.Paused) return;
            DebugTrace();

            PaStream* s;
            lock (streamLock)
            {
                if (stream == null) return;
                s = stream;
                stream = null;
                PlaybackState = PlaybackState.Stopped;
            }

            var closeResult = NativeMethods.Pa_CloseStream(s);
            if (closeResult != (int)PaErrorCode.paNoError)
            {
                _log?.Error("PORTAUDIO ERROR: Pa_CloseStream failed with code {CloseResult}", closeResult);
                Stopped?.Invoke(this,
                    new PlaybackStoppedEventArgs(
                        new PortAudioException($"PortAudio stream closure failed: {closeResult}")));
                return;
            }

            Stopped?.Invoke(this, new PlaybackStoppedEventArgs());
        }
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
            var outputBuffer = (float*)output;
            var bufferLength = (int)frameCount * channels;
            if (buffer == null || buffer.Length < bufferLength)
            {
                Debugger.Break();
                new Span<float>(outputBuffer, bufferLength).Clear();
                Array.Resize(ref buffer, bufferLength);
                return (int)PaStreamCallbackResult.paContinue;
            }

            var read = SampleSource.Read(buffer, 0, bufferLength);

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
                        var floatInput = Avx.LoadVector256(bufferI); 
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
                        var floatInput = Sse.LoadVector128(bufferI);
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