using SilverCraft.CSCore.Codecs.FLAC;
using SilverCraft.CSCore.PortAudio;
using SilverCraft.CSCore.SoundOut;

args = ["/home/silver/source/cscore/SilverCraft.CsCore.BasicPlayTest/music.flac"];
using FlacFile f = new(args.Length>0 ? args[0] : "../../../music.flac");
using PortAudioSoundOut soundOut = new();
soundOut.Initialize(f);
soundOut.Play();
while (soundOut.PlaybackState == PlaybackState.Playing)
{
    Thread.Sleep(800);
}