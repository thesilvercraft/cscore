using SilverCraft.CSCore;
using SilverCraft.CSCore.Codecs.FLAC;
using SilverCraft.CSCore.DSP.Resampler;
using SilverCraft.CSCore.OpenMPT;
using SilverCraft.CSCore.PortAudio;
using SilverCraft.CSCore.SoundOut;
using SilverCraft.CSCore.Streams;
using SilverCraft.CSCore.Streams.SampleConverter;
using SilverCraft.CSCore.VGMStream;
args = [@"C:\Users\Filip Velichkovski\Music\TEE\05 Metall auf Metall.flac"];
float volume = 1f;

PortAudioSoundOut soundOut = new();
    soundOut.Stopped+= (sender, eventArgs) => Console.WriteLine("Sound Out Stopped");

while (true)
{
 
    FlacFile f = new(File.Open(args.Length > 0 ? args[0] : "../../../music.flac", FileMode.Open, FileAccess.Read));
    soundOut.Initialize(f);
    soundOut.Volume = volume;
    soundOut.Play();
    while (soundOut.PlaybackState != PlaybackState.Stopped)
    {
        Console.Write("\r"+f.GetPosition());
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
                      soundOut.Volume =volume= MathF.Min(1, soundOut.Volume + 0.1f);
                      break;
                  case ConsoleKey.DownArrow:
                      soundOut.Volume =volume= MathF.Max(0, soundOut.Volume - 0.1f);
                      break;
                  default:
                      break;
            }
        }
        Thread.Sleep(33);
    }
    f.Dispose();
}
/*
var x = new OpenMptWaveStream(File.OpenRead(@"C:\Users\Filip Velichkovski\Downloads\alphaline.mod"));
var soundIn = new PortAudioSoundIn();
var soundOut = new PortAudioSoundOut();
soundIn.Initialize();
soundOut.Initialize(new SoundInSource(soundIn, 44100));
soundIn.Start();
soundOut.Play();

Thread.Sleep(-1);
*/