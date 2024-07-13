using System;
using Serilog;

namespace SilverCraft.CSCore;

public static class LogLocation
{
    public static ILogger? GetLogger(Type type)
    {
        return GetFunc?.Invoke(type);
    }
    public static Func<Type, ILogger> GetFunc;
}