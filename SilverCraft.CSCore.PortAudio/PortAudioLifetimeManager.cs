using SilverCraft.CSCore.PortAudio.Native;

namespace SilverCraft.CSCore.PortAudio;

public static class PortAudioLifetimeManager
{
    private static readonly object Lock = new();
    private static int _referenceCount = 0;
    private static bool DisableTerminate = false;

    /// <summary>
    /// Prevents the <see cref="PortAudioLifetimeManager"/> from automatically terminating PortAudio when instances are disposed.
    /// </summary>
    /// <remarks>
    /// This method sets an internal flag, effectively suspending the automatic termination logic of PortAudio.
    /// Call this if you intend to handle the global PortAudio cleanup manually.
    /// </remarks>
    public static void IPinkyPromiseToTerminatePortAudioMyself()
    {
        DisableTerminate = true;
    }

    /// <summary>
    /// Initializes PortAudio if it has not been initialized already, and increments the internal reference count.
    /// </summary>
    /// <remarks>
    /// This method ensures that the underlying PortAudio system resources are set up for use.
    /// It is safe to call multiple times, but subsequent calls only increment the reference count.
    /// </remarks>
    public static void Initialize()
    {
        lock (Lock)
        {
            if (_referenceCount == 0)
            {
                var err = NativeMethods.Pa_Initialize();
                if (err != (int)PaErrorCode.paNoError)
                {
                    throw new PortAudioException($"Failed to initialize PortAudio. Error code: {err}");
                }
            }
            _referenceCount++;
        }
    }

    /// <summary>
    /// Terminates the global PortAudio resources and cleans up associated native structures.
    /// </summary>
    /// <remarks>
    /// This method safely decrements an internal reference count. Termination only proceeds if the reference count reaches zero and the termination is not manually disabled via <see cref="IPinkyPromiseToTerminatePortAudioMyself"/>.
    /// If termination occurs, it calls the native PortAudio termination function (<see cref="NativeMethods.Pa_Terminate()"/>).
    /// </remarks>
    /// <param name="log">An optional logger used to record any errors encountered during the native PortAudio shutdown process.</param>
    public static void Terminate(Serilog.ILogger? log)
    {
        lock (Lock)
        {
            if (_referenceCount == 0) return;

            _referenceCount--;
            if (_referenceCount != 0 || DisableTerminate) return;
            var err = NativeMethods.Pa_Terminate();
            if (err != (int)PaErrorCode.paNoError)
            {
                log?.Error("PortAudio terminated with error code {Err}", err);
            }
        }
    }
}