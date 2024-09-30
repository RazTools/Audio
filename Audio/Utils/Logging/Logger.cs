namespace Audio;
public static class Logger
{
    public static ILogger? Instance { get; set; }

    public static void Trace(string message) => Instance?.Log(LogLevel.Trace, message);
    public static void Debug(string message) => Instance?.Log(LogLevel.Debug, message);
    public static void Info(string message) => Instance?.Log(LogLevel.Info, message);
    public static void Warning(string message) => Instance?.Log(LogLevel.Warning, message);
    public static void Error(string message) => Instance?.Log(LogLevel.Error, message);
    public static void Progress(string message, int current, int total) => Instance?.Log(LogLevel.Info, $"{message} [{current}/{total}]");
}