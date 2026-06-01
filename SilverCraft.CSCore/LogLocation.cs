using Serilog;

namespace SilverCraft.CSCore;

/// <summary>
/// Provides static methods for obtaining an <see cref="Serilog.ILogger"/> instance associated with a specific type.
/// </summary>
public static class LogLocation
{
    /// <summary>
    /// Gets the logger instance for a specific type.
    /// </summary>
    /// <param name="type">The Type used to get the associated logger.</param>
    /// <returns>An ILogger instance scoped to the provided type.</returns>
    public static ILogger? GetLogger(Type type)
    {
        return GetFunc?.Invoke(type);
    }

    /// <summary>
    /// Gets or sets the function used to resolve and create an ILogger instance based on a given type.
    /// </summary>
    /// <remarks>
    /// This delegate must return a functional logger configured for use within the application's scope.
    /// </remarks>
    public static Func<Type, ILogger> GetFunc;
}