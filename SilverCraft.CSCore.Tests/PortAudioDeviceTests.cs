using SilverCraft.CSCore.PortAudio;
using SilverCraft.CSCore.SoundIn;
using SilverCraft.CSCore.SoundOut;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class PortAudioDeviceTests
{
    [Fact]
    public void PortAudioSoundOut_ConstructorsAndDeviceEnumeration_Work()
    {
        using var defaultOut = new PortAudioSoundOut();
        using var namedOut = new PortAudioSoundOut("NonExistentDevice");

        Assert.Null(defaultOut.PreferredDevice);
        Assert.Equal("NonExistentDevice", namedOut.PreferredDevice);
        Assert.Equal(PortAudioHelper.DefaultDeviceLabel, PortAudioSoundOut.DefaultDeviceLabel);

        _ = PortAudioSoundOut.IsSupported();
        var devices = PortAudioSoundOut.GetOutputDeviceNames();
        Assert.NotEmpty(devices);
        Assert.Equal(PortAudioSoundOut.DefaultDeviceLabel, devices[0]);
    }

    [Fact]
    public void PortAudioSoundIn_ConstructorsPreferredDeviceAndEnumeration_Work()
    {
        using var defaultIn = new PortAudioSoundIn();
        using var configuredIn = new PortAudioSoundIn(channels: 2, sampleRate: 48000, preferredDevice: "Mic 1");

        Assert.Null(defaultIn.PreferredDevice);
        Assert.Equal("Mic 1", configuredIn.PreferredDevice);
        Assert.Equal(2, configuredIn.Channels);
        Assert.Equal(48000, configuredIn.SampleRate);
        Assert.Equal(PortAudioHelper.DefaultDeviceLabel, PortAudioSoundIn.DefaultDeviceLabel);

        configuredIn.PreferredDevice = "Mic 2";
        Assert.Equal("Mic 2", configuredIn.PreferredDevice);

        _ = PortAudioSoundIn.IsSupported();
        var devices = PortAudioSoundIn.GetInputDeviceNames();
        Assert.NotEmpty(devices);
        Assert.Equal(PortAudioSoundIn.DefaultDeviceLabel, devices[0]);
    }

    [Fact]
    public void PortAudioSoundOut_InitializeNullOrDisposed_Throws()
    {
        var soundOut = new PortAudioSoundOut();
        Assert.Throws<ArgumentNullException>(() => soundOut.Initialize(null!));

        soundOut.Dispose();
        var source = new TestWaveSource(new byte[16], new WaveFormat(44100, 16, 2));
        Assert.Throws<ObjectDisposedException>(() => soundOut.Initialize(source));
    }

    [Fact]
    public void PortAudioSoundIn_InitializeDisposed_Throws()
    {
        var soundIn = new PortAudioSoundIn();
        soundIn.Dispose();

        Assert.Throws<ObjectDisposedException>(() => soundIn.Initialize());
    }
}
