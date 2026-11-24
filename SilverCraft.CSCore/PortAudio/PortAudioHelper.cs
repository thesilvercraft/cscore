using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SilverCraft.CSCore.PortAudio.Native;

namespace SilverCraft.CSCore.PortAudio;

internal static class PortAudioHelper
{
    public const string DefaultDeviceLabel = "Default";

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

    public static unsafe string[] GetDeviceNames(bool isOutput, ILogger? log)
    {
        var result = new List<string> { DefaultDeviceLabel };
        var initialized = false;
        try
        {
            PortAudioLifetimeManager.Initialize();
            initialized = true;
            var count = NativeMethods.Pa_GetDeviceCount();
            for (var i = 0; i < count; i++)
            {
                var info = NativeMethods.Pa_GetDeviceInfo(i);
                if (info == null) continue;
                var channelCount = isOutput ? info->maxOutputChannels : info->maxInputChannels;
                if (channelCount <= 0) continue;

                var label = FormatDeviceLabel(i, info);
                if (!result.Contains(label))
                {
                    result.Add(label);
                }
            }
        }
        catch (Exception ex)
        {
            log?.LogWarning(ex, "Failed to enumerate PortAudio {Direction} devices", isOutput ? "output" : "input");
        }
        finally
        {
            if (initialized)
            {
                PortAudioLifetimeManager.Terminate(log);
            }
        }

        return [.. result];
    }

    public static unsafe int ResolveDeviceIndex(string? preferredDevice, bool isOutput, ILogger? log)
    {
        var defaultDevice = isOutput
            ? NativeMethods.Pa_GetDefaultOutputDevice()
            : NativeMethods.Pa_GetDefaultInputDevice();

        if (string.IsNullOrWhiteSpace(preferredDevice) ||
            string.Equals(preferredDevice, DefaultDeviceLabel, StringComparison.OrdinalIgnoreCase))
        {
            return defaultDevice;
        }

        var count = NativeMethods.Pa_GetDeviceCount();
        for (var i = 0; i < count; i++)
        {
            var info = NativeMethods.Pa_GetDeviceInfo(i);
            if (info == null) continue;
            var channelCount = isOutput ? info->maxOutputChannels : info->maxInputChannels;
            if (channelCount <= 0) continue;

            var deviceName = Marshal.PtrToStringAnsi((nint)info->name) ?? $"Device {i}";
            var label = FormatDeviceLabel(i, info, deviceName);
            if (string.Equals(label, preferredDevice, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(deviceName, preferredDevice, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        log?.LogWarning("Configured PortAudio {Direction} device '{Device}' not found, falling back to default device",
            isOutput ? "output" : "input", preferredDevice);
        return defaultDevice;
    }

    private static unsafe string FormatDeviceLabel(int index, PaDeviceInfo* info, string? deviceName = null)
    {
        deviceName ??= Marshal.PtrToStringAnsi((nint)info->name) ?? $"Device {index}";
        var hostInfo = NativeMethods.Pa_GetHostApiInfo(info->hostApi);
        var hostName = hostInfo != null
            ? Marshal.PtrToStringAnsi((nint)hostInfo->name) ?? "Unknown"
            : "Unknown";
        return $"{deviceName} ({hostName})";
    }

    public static unsafe void AbortAndCloseStream(PaStream* stream, ILogger? log)
    {
        _ = NativeMethods.Pa_AbortStream(stream);
        var closeResult = NativeMethods.Pa_CloseStream(stream);
        if (closeResult != (int)PaErrorCode.paNoError)
        {
            log?.LogError("PORTAUDIO ERROR: Pa_CloseStream failed with code {CloseResult}", closeResult);
        }
    }

    public static unsafe void ThrowIfError(
        int err,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int num = 0)
    {
        if (err != (int)PaErrorCode.paNoError)
        {
            throw new PortAudioException(
                $"PortAudio failed, error code {err} ({Marshal.PtrToStringAnsi((nint)NativeMethods.Pa_GetErrorText(err))}) in method {callerName} in {path}:{num}");
        }
    }
}
