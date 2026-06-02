using SilverCraft.CSCore;
using SilverCraft.CSCore.Codecs.FLAC;
using SilverCraft.CSCore.DSP.Resampler;
using SilverCraft.CSCore.PortAudio;
using SilverCraft.CSCore.SoundOut;
using SilverCraft.CSCore.Streams.SampleConverter;
using SilverCraft.CSCore.VGMStream;
VGMStreamWaveSource f =
    new(("/home/silver/Downloads/Sega Bass Fishing (Europe) (En,Fr,De,Es,It)/stream/DC_RESULT.brstm"));
float volume = 1f;

try
{
    using var o = new PortAudioSoundOut();
    o.Initialize(new Pcm16BitToSample(f).ToWaveSource());
    o.Volume=volume;
    o.Play();
    while (o.PlaybackState != PlaybackState.Stopped)
    {
        if (Console.KeyAvailable)
        {
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.RightArrow:
                    f.SetPosition(f.GetPosition()+ TimeSpan.FromSeconds(3));
                    break;
                case ConsoleKey.LeftArrow:
                    f.SetPosition(f.GetPosition()- TimeSpan.FromSeconds(3));
                    break;
                case ConsoleKey.UpArrow:
                    o.Volume =volume= MathF.Min(1, o.Volume + 0.1f);
                    Console.WriteLine(volume);
                    break;
                case ConsoleKey.DownArrow:
                    o.Volume =volume= MathF.Max(0, o.Volume - 0.1f);
                    Console.WriteLine(volume);
                    break;
            }
        }
        Thread.Sleep(100);
    }
}
finally
{
    f.Dispose();
}


/*
args = ["/home/silver/source/cscore/SilverCraft.CsCore.BasicPlayTest/music.flac"];
float volume = 1f;
while (true)
{
     //FlacFile f = new(args.Length>0 ? args[0] : "../../../music.flac");
    SndFileWaveStream f= new(new FileStream(args[0], FileMode.Open));
    PortAudioSoundOut soundOut = new();
    soundOut.Volume = volume;
    var x = f.ToWaveSource();
    
    soundOut.Initialize(x);
    soundOut.Play();
    soundOut.Stopped+= (sender, eventArgs) => Console.WriteLine("Sound Out Stopped");
    while (soundOut.PlaybackState != PlaybackState.Stopped)
    {
        if (Console.KeyAvailable)
        {
            switch (Console.ReadKey().Key)
            {
              case ConsoleKey.RightArrow:
                  x.SetPosition(x.GetPosition()+ TimeSpan.FromSeconds(3));
                  break;
                  case ConsoleKey.LeftArrow:
                      x.SetPosition(x.GetPosition()- TimeSpan.FromSeconds(3));
                      break;
                  case ConsoleKey.UpArrow:
                      soundOut.Volume =volume= MathF.Min(1, soundOut.Volume + 0.1f);
                      break;
                  case ConsoleKey.DownArrow:
                      soundOut.Volume =volume= MathF.Max(0, soundOut.Volume - 0.1f);
                      break;
            }
        }
        Thread.Sleep(100);
    }
    soundOut.Dispose();
    f.Dispose();
}
*/