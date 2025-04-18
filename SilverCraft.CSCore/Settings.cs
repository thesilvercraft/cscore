namespace SilverCraft.CSCore;

public static class Settings
{
    public static bool DecodeImages { get; set; } = false;
    public static bool AllowExternalImages { get; set; } = false;
    /// <summary>
    /// HttpClient to use for gathering external image
    /// </summary>
    public static HttpClient? HttpClient
    {
        get;
        set;
    } = null;
}