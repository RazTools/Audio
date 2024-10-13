using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Platform.Storage;

namespace Audio.GUI.Services;
public class PlatformServiceProvider(TopLevel? TopLevel) : IPlatformServiceProvider
{
    public IClipboard? Clipboard => TopLevel?.Clipboard;
    public IFocusManager? FocusManager => TopLevel?.FocusManager;
    public IInsetsManager? InsetsManager => TopLevel?.InsetsManager;
    public IPlatformSettings? PlatformSettings => TopLevel?.PlatformSettings;
    public IStorageProvider? StorageProvider => TopLevel?.StorageProvider;
}
