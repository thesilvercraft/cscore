using SilverCraft.CSCore;
using SilverCraft.CSCore.Codecs.FLAC;
using SilverCraft.CSCore.PortAudio;
using SilverCraft.CSCore.SndFile;
using SilverCraft.CSCore.SoundOut;
using SilverCraft.CSCore.Streams.SampleConverter;

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
