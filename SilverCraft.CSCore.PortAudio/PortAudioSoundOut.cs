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


//https://www.portaudio.com/docs/v19-doxydocs/writing_a_callback.html

public class PortAudioSoundOut : ISoundOut
{
    private readonly object streamLock = new();
    private float[]? buffer = new float[2];
    private StreamCallBack? _callback;
    private int channels;
    private bool isDisposed;
    private bool isPAInit;
    private unsafe PaStream* stream;
    private ILogger? _log;
    static PortAudioSoundOut()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, NativeMethods.DllImportResolver);
    }

    public PortAudioSoundOut()
    {
        _log=LogLocation.GetLogger(typeof(PortAudioSoundOut));
    }
    private ISampleSource? SampleSource { get; set; }
    public float Volume { get; set; } = 1;
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

        if (source != WaveSource) WaveSource?.Dispose();
        SampleSource?.Dispose();
        WaveSource = source;
        SampleSource = source.ToSampleSource();
        channels = SampleSource.WaveFormat.Channels;
        var initialBufferLength = Math.Max(4096 * channels, 8192);
        buffer = new float[initialBufferLength];
        if (stream != null)
        {
            Stop();
            lock (streamLock)
            {
                ThrowIfError(NativeMethods.Pa_CloseStream(stream));
                stream = null;
            }
        }
        lock (streamLock)
        {
            PaStream* paStream;
            ThrowIfError(NativeMethods.Pa_OpenDefaultStream(&paStream,
                0,
                SampleSource.WaveFormat.Channels,
                NativeMethods.paFloat32,
                SampleSource.WaveFormat.SampleRate,
                0,
                _callback,
                0));
            stream = paStream;
        }
    }


    public unsafe void Play()
    {
        DebugTrace();
        if (PlaybackState == PlaybackState.Playing) return;
        ObjectDisposedException.ThrowIf(stream==null, nameof(stream));
        
        PlaybackState = PlaybackState.Playing;
        Task.Run(() =>
        {
            ThrowIfError(NativeMethods.Pa_StartStream(stream));
        });
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
        ObjectDisposedException.ThrowIf(stream==null, nameof(stream));

        ThrowIfError(NativeMethods.Pa_StopStream(stream));
        PlaybackState = PlaybackState.Paused;
    }

    public unsafe void Stop()
    {
        if (PlaybackState == PlaybackState.Stopped) return;
        DebugTrace();
        if (PlaybackState == PlaybackState.Paused)
        {
            PlaybackState = PlaybackState.Stopped;
            OnStop();
            return;
        }
        ObjectDisposedException.ThrowIf(stream==null, nameof(stream));

        ThrowIfError(NativeMethods.Pa_StopStream(stream));
        OnStop();
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
        DebugTrace();
        if (isDisposed) return;
        _callback= null;

        if (isPAInit)
        {
            PortAudioLifetimeManager.Terminate(_log);
            isPAInit = false;
        }

        isDisposed = true;
    }


    ~PortAudioSoundOut()
    {
        Dispose(false);
    }

    private void OnStop()
    {
        DebugTrace();
        PlaybackState = PlaybackState.Stopped;

        lock (streamLock)
        {
            unsafe
            {
                if (stream != null)
                {
                    var closeResult = NativeMethods.Pa_CloseStream(stream);
                    if (closeResult != (int)PaErrorCode.paNoError)
                    {
                        _log?.Error("PORTAUDIO ERROR: Pa_CloseStream failed with code {CloseResult}", closeResult);
                        Stopped?.Invoke(this,
                            new PlaybackStoppedEventArgs(
                                new PortAudioException($"PortAudio stream closure failed: {closeResult}")));
                        stream = null;
                        buffer = null;
                        return;
                    }

                    stream = null;
                }
            }
        }

        Stopped?.Invoke(this, new PlaybackStoppedEventArgs());
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
                // THIS IS BAD, like really bad
                for (int i = 0; i < bufferLength; i++) outputBuffer[i] = 0;
                return (int)PaStreamCallbackResult.paContinue;
            }
            var read = SampleSource?.Read(buffer, 0, bufferLength);
            read ??= 0;
            fixed (float* bufferStart = buffer) //multiplying is not that more expensive then just copying from one buffer to another and branch overhead is a thing ig
            {
                var bufferI = bufferStart;
                var bufferEnd = bufferStart + read.Value;
                var bufferExpectedEnd = outputBuffer + bufferLength;

                var outputI = outputBuffer;
                var currentVolume = Volume;
                if (Avx.IsSupported)
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
                while (bufferI < bufferEnd)
                {
                    *outputI++ = *bufferI++ * currentVolume;
                }
                // do not produce noise if uninitialized
                while (outputI < bufferExpectedEnd) *outputI++ = 0;
            }

            if (read >= bufferLength) return (int)PaStreamCallbackResult.paContinue;
            Task.Run(OnStop); // TODO: replace with PaStreamFinishedCallback
            return (int)PaStreamCallbackResult.paComplete;
        }
        catch (Exception e)
        {
            _log?.Error(e,"ERROR IN THE CALLBACK THREAD");
            return (int)PaStreamCallbackResult.paAbort;
        }
    }
}