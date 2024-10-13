namespace Audio;
public static class Logger
{
    private static readonly HashSet<ILogger?> _loggers = [];

    public static LogLevel LogLevel { get; set; } = LogLevel.All;

    public static void Clear() => _loggers.Clear();
    public static bool TryRegister(ILogger? logger) => _loggers.Add(logger);
    public static bool TryUnregister(ILogger? logger) => _loggers.Remove(logger);

    public static void Trace(string message)
    {
        if (LogLevel.HasFlag(LogLevel.Trace))
        {
            foreach (ILogger? logger in _loggers)
            {
                logger?.Log(LogLevel.Trace, message);
            }
        }
    }
    public static void Debug(string message)
    {
        if (LogLevel.HasFlag(LogLevel.Debug))
        {
            foreach (ILogger? logger in _loggers)
            {
                logger?.Log(LogLevel.Debug, message);
            }
        }
    }
    public static void Info(string message)
    {
        if (LogLevel.HasFlag(LogLevel.Info))
        {
            foreach (ILogger? logger in _loggers)
            {
                logger?.Log(LogLevel.Info, message);
            }
        }
    }
    public static void Warning(string message)
    {
        if (LogLevel.HasFlag(LogLevel.Warning))
        {
            foreach (ILogger? logger in _loggers)
            {
                logger?.Log(LogLevel.Warning, message);
            }
        }
    }
    public static void Error(string message)
    {
        if (LogLevel.HasFlag(LogLevel.Error))
        {
            foreach (ILogger? logger in _loggers)
            {
                logger?.Log(LogLevel.Error, message);
            }
        }
    }
    public static void Progress(string message, int current, int total)
    {
        if (LogLevel.HasFlag(LogLevel.Info))
        {
            foreach (ILogger? logger in _loggers)
            {
                logger?.Log(LogLevel.Info, $"{message} [{current}/{total}]");
            }
        }
    }
}