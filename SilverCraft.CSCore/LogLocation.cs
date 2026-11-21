using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SilverCraft.CSCore;

/// <summary>
/// Provides static methods for obtaining an <see cref="Microsoft.Extensions.Logging.ILogger"/> instance.
/// </summary>
public static class LogLocation
{
    // Default to a null logger factory to prevent crashes if uninitialized
    private static ILoggerFactory _factory = NullLoggerFactory.Instance;

    internal static ILoggerFactory Factory => Volatile.Read(ref _factory);

    /// <summary>
    /// Called by the consuming application at startup to provide a real logger factory.
    /// </summary>
    public static void Initialize(ILoggerFactory factory)
    {
        Volatile.Write(ref _factory, factory ?? throw new ArgumentNullException(nameof(factory)));
    }

    /// <summary>
    /// Gets the logger instance for a specific type.
    /// </summary>
    public static ILogger GetLogger(Type type)
    {
        return _factory.CreateLogger(type);
    }

    /// <summary>
    /// Generic helper for easier syntax.
    /// </summary>
    public static ILogger<T> GetLogger<T>()
    {
        return _factory.CreateLogger<T>();
    }
}