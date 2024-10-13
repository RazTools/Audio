using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Platform.Storage;

namespace Audio.GUI.Services;
public interface IPlatformServiceProvider
{
    IClipboard? Clipboard { get; }
    IFocusManager? FocusManager { get; }
    IInsetsManager? InsetsManager { get; }
    IPlatformSettings? PlatformSettings { get; }
    IStorageProvider? StorageProvider { get; }
}
