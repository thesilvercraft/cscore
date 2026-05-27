namespace SilverCraft.CSCore.PortAudio;

public class PortAudioException : Exception
{
    public PortAudioException(string message) : base(message)
    {
    }

    public PortAudioException()
    {
    }

    public PortAudioException(string message, Exception innerException) : base(message, innerException)
    {
    }
}