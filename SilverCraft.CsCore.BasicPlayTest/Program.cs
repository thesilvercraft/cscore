using SilverCraft.CSCore;
using SilverCraft.CSCore.Codecs.FLAC;
using SilverCraft.CSCore.Codecs.WAV;
using SilverCraft.CSCore.OpenMPT;
using SilverCraft.CSCore.PortAudio;
using SilverCraft.CSCore.SoundOut;

if (args.Length > 0)
{
    if (args[0].EndsWith(".flac"))
    {
        FlacFile f = new(args.Length>0 ? args[0] : "../../../music.flac");
        Console.WriteLine("ALSOUNDOUT");
        ALSoundOut soundOut = new();
        soundOut.Initialize(f);
        soundOut.Play();
        while (soundOut.PlaybackState == PlaybackState.Playing)
        {
            Thread.Sleep(1000);
        }
    }
    else  if (args[0].EndsWith(".wav"))
    {
        WaveFileReader w = new(args.Length>0 ? args[0] : "../../../music.wav");
        Console.WriteLine("portaudio");
        PortAudioSoundOut soundOut = new();
        soundOut.Initialize(w);
        soundOut.Play();
        while (soundOut.PlaybackState == PlaybackState.Playing)
        {
            Thread.Sleep(1000);
        }
    }
    else if (args[0].EndsWith(".mo3")  ||  args[0].EndsWith(".xm") || args[0].EndsWith(".mod"))
    {
        using var fs = File.OpenRead(args[0]);
        OpenMptWaveStream openMptWaveStream = new(fs);
        Console.WriteLine("ALSOUNDOUT + OPENMPT");
        ALSoundOut soundOut = new();
        var towvs = openMptWaveStream.ToWaveSource();
        soundOut.Initialize(towvs);
        soundOut.Play();
        while (soundOut.PlaybackState == PlaybackState.Playing)
        {
            Thread.Sleep(1000);
        }
    }
}
