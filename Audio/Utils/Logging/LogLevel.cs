namespace Audio;
[Flags]
public enum LogLevel
{
    None,
    Trace   = 1 << 0,
    Debug   = 1 << 1,
    Info    = 1 << 2,
    Warning = 1 << 3,
    Error   = 1 << 4,
    All = Trace | Debug | Info | Warning | Error
}
