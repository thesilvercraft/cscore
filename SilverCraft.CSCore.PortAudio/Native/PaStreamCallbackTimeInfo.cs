namespace SilverCraft.CSCore.PortAudio.Native;

public partial struct PaStreamCallbackTimeInfo
{
    [NativeTypeName("PaTime")]
    public double inputBufferAdcTime;

    [NativeTypeName("PaTime")]
    public double currentTime;

    [NativeTypeName("PaTime")]
    public double outputBufferDacTime;
}
