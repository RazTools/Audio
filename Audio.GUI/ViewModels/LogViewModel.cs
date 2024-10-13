using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace Audio.GUI.ViewModels;

public partial class LogViewModel : ViewModelBase, ILogger, IDisposable
{
    [ObservableProperty]
    private ObservableCollection<string> logs = [];

    public LogViewModel()
    {
        Logger.TryRegister(this);
    }

    public void Log(LogLevel logLevel, string message)
    {
        Logs.Add($"[{logLevel}]: {message}");
    }
    public void Dispose()
    {
        Logger.TryUnregister(this);
        GC.SuppressFinalize(this);
    }
}