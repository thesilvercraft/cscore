using SilverCraft.CSCore;
using SilverCraft.CSCore.Codecs.FLAC;
using SilverCraft.CSCore.DSP.Resampler;
using SilverCraft.CSCore.PortAudio;
using SilverCraft.CSCore.SoundOut;
using SilverCraft.CSCore.Streams;
using SilverCraft.CSCore.Streams.SampleConverter;
using SilverCraft.CSCore.VGMStream;
args = ["/home/silver/source/cscore/SilverCraft.CsCore.BasicPlayTest/music.flac"];
float volume = 1f;
/*PortAudioSoundOut soundOut = new();

while (true)
{
    FlacFile f = new(args.Length>0 ? args[0] : "../../../music.flac");
    soundOut.Initialize(f);
    soundOut.Volume = volume;
    soundOut.Play();
    soundOut.Stopped+= (sender, eventArgs) => Console.WriteLine("Sound Out Stopped");
    while (soundOut.PlaybackState != PlaybackState.Stopped)
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
                      soundOut.Volume =volume= MathF.Min(1, soundOut.Volume + 0.1f);
                      break;
                  case ConsoleKey.DownArrow:
                      soundOut.Volume =volume= MathF.Max(0, soundOut.Volume - 0.1f);
                      break;
                  default:
                      break;
            }
        }
        Thread.Sleep(100);
    }
    f.Dispose();
}
*/

var soundIn = new PortAudioSoundIn();
var soundOut = new PortAudioSoundOut();
soundIn.Initialize();
soundOut.Initialize(new SoundInSource(soundIn));
soundIn.Start();
soundOut.Play();
Thread.Sleep(-1);