namespace Audio;
public interface ILogger
{
    void Log(LogLevel logType, string message);
}