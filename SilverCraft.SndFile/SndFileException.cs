namespace SilverCraft.CSCore.SndFile;

public class SndFileException : Exception
{
    public SndFileException(string message) : base(message)
    {
    }

    public SndFileException()
    {
    }

    public SndFileException(string message, Exception innerException) : base(message, innerException)
    {
    }
}