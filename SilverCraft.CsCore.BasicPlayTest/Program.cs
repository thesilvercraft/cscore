using SilverCraft.CSCore;
using SilverCraft.CSCore.Codecs.FLAC;
using SilverCraft.CSCore.PortAudio;

args = ["/home/silver/source/cscore/SilverCraft.CsCore.BasicPlayTest/music.flac"];
while (true)
{
     FlacFile f = new(args.Length>0 ? args[0] : "../../../music.flac");
    
    PortAudioSoundOut soundOut = new();
    soundOut.Initialize(f);
    soundOut.Play();
    soundOut.Stopped+= (sender, eventArgs) => Console.WriteLine("Sound Out Stopped"); 
    soundOut.WaitForStopped();
    soundOut.Dispose();
    f.Dispose();
}
