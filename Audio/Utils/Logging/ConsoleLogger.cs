namespace Audio;
public class ConsoleLogger : ILogger
{
    public void Log(LogLevel logLevel, string message)
    {
        Console.WriteLine($"[{logLevel}]: {message}");
    }
}
