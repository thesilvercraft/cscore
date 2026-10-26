namespace SilverCraft.CSCore.PortAudio.Native;

[NativeTypeName("unsigned int")]
public enum PaStreamCallbackResult : uint
{
    paContinue = 0,
    paComplete = 1,
    paAbort = 2,
}
