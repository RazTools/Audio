using Audio.Entries;
using Audio.GUI.Models;
using Audio.GUI.Services;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Audio.GUI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly AudioManager _audioManager;
    private readonly TreeViewModel _treeViewModel;
    private readonly EntryViewModel _entryViewModel;

    [ObservableProperty]
    private bool convert;
    [ObservableProperty]
    private bool playlist;

    private IPlatformServiceProvider? _platformServiceProvider;
    private IPlatformServiceProvider PlatformServiceProvider
    {
        get
        {
            return _platformServiceProvider ??= Ioc.Default.GetRequiredService<IPlatformServiceProvider>();
        }
    }

    public TreeViewModel TreeViewModel => _treeViewModel;
    public EntryViewModel EntryViewModel => _entryViewModel;
    public string? VOPath
    {
        get => ConfigManager.Instance.VOPath;
        set
        {
            ConfigManager.Instance.VOPath = value;
            ConfigManager.Instance.Save();
        }
    }
    public string? EventPath
    {
        get => ConfigManager.Instance.EventPath;
        set
        {
            ConfigManager.Instance.EventPath = value;
            ConfigManager.Instance.Save();
        }
    }

    public MainViewModel()
    {
        _audioManager = new();
        _entryViewModel = new();
        _treeViewModel = new(_audioManager, _entryViewModel);

        ConfigManager.Instance.Load();

        Convert = ConfigManager.Instance.Convert;
        Playlist = ConfigManager.Instance.Playlist;
    }

    [RelayCommand]
    public async Task LoadFile()
    {
        if (PlatformServiceProvider.StorageProvider !=  null)
        {
            IReadOnlyList<IStorageFile> files = await PlatformServiceProvider.StorageProvider.OpenFilePickerAsync(new() { Title = "Pick file(s)", AllowMultiple = true });
            IEnumerable<string> paths = files.Select(x => x.TryGetLocalPath() ?? "");
            if (paths.Any())
            {
                _audioManager.Clear();
                int loaded = await Task.Run(() => _audioManager.LoadFiles(paths.ToArray()));
                if (loaded > 0)
                {
                    _treeViewModel.Update();
                }
            }
        }
    }
    
    [RelayCommand]
    public async Task LoadFolder()
    {
        if (PlatformServiceProvider.StorageProvider != null)
        {
            IReadOnlyList<IStorageFolder> folders = await PlatformServiceProvider.StorageProvider.OpenFolderPickerAsync(new());

            List<string> files = [];
            foreach (IStorageFolder folder in folders)
            {
                string? path = folder.TryGetLocalPath();
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
                }
            }

            if (files.Count != 0)
            {
                _audioManager.Clear();
                int loaded = await Task.Run(() => _audioManager.LoadFiles([.. files]));
                if (loaded > 0)
                {
                    _treeViewModel.Update();
                }
            }
        }
    }
    
    [RelayCommand]
    public async Task ExportSound() => await ExportEntry([EntryType.Sound, EntryType.EmbeddedSound, EntryType.External]);

    [RelayCommand]
    public async Task ExportBank() => await ExportEntry([EntryType.Bank]);

    [RelayCommand]
    public async Task ExportAll() => await ExportEntry([EntryType.Bank, EntryType.Sound, EntryType.EmbeddedSound, EntryType.External]);

    private async Task ExportEntry(IEnumerable<EntryType> types)
    {
        if (PlatformServiceProvider.StorageProvider != null)
        {
            IReadOnlyList<IStorageFolder> folders = await PlatformServiceProvider.StorageProvider.OpenFolderPickerAsync(new() { AllowMultiple = false });

            IStorageFolder? folder = folders.FirstOrDefault();
            if (folder != null)
            {
                string? path = folder.TryGetLocalPath();
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    if (_treeViewModel.Checked.Any())
                    {
                        await Task.Run(() => _audioManager.DumpEntries(path, _treeViewModel.Checked.OfType<EntryTreeNode>().Select(x => x.Entry)));
                    }
                    else
                    {
                        await Task.Run(() => _audioManager.DumpEntries(path, types));
                    }
                }
            }
        }
    }
    
    [RelayCommand]
    public async Task ExportHierarchy()
    {
        if (PlatformServiceProvider.StorageProvider != null)
        {
            IReadOnlyList<IStorageFolder> folders = await PlatformServiceProvider.StorageProvider.OpenFolderPickerAsync(new() { AllowMultiple = false });

            IStorageFolder? folder = folders.FirstOrDefault();
            if (folder != null)
            {
                string? path = folder.TryGetLocalPath();
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    await Task.Run(() => _audioManager.DumpHierarchies(path));
                }
            }
        }
    }

    [RelayCommand]
    public async Task SetVOPath()
    {
        if (PlatformServiceProvider.StorageProvider != null)
        {
            IReadOnlyList<IStorageFile> files = await PlatformServiceProvider.StorageProvider.OpenFilePickerAsync(new() { Title = "Pick file" });
            IEnumerable<string> paths = files.Select(x => x.TryGetLocalPath() ?? "");
            if (paths.Any())
            {
                VOPath = paths.First();
            }
        }
    }
    
    [RelayCommand]
    public async Task SetEventPath()
    {
        if (PlatformServiceProvider.StorageProvider != null)
        {
            IReadOnlyList<IStorageFile> files = await PlatformServiceProvider.StorageProvider.OpenFilePickerAsync(new() { Title = "Pick file" });
            IEnumerable<string> paths = files.Select(x => x.TryGetLocalPath() ?? "");
            if (paths.Any())
            {
                EventPath = paths.First();
            }
        }
    }
    
    [RelayCommand]
    public async Task LoadVO()
    {
        if (string.IsNullOrEmpty(VOPath))
        {
            Logger.Warning("VO path must be set first !!");
            return;
        }

        await Task.Run(() => _audioManager.UpdateExternals(File.ReadAllLines(VOPath)));
        await Dispatcher.UIThread.InvokeAsync(_treeViewModel.Update);
    }
    
    [RelayCommand]
    public async Task LoadEvents()
    {
        if (string.IsNullOrEmpty(EventPath))
        {
            Logger.Warning("Event path must be set first !!");
            return;
        }

        await Task.Run(() => _audioManager.UpdatedEvents(File.ReadAllLines(EventPath)));
        await Task.Run(_audioManager.ProcessEvents);
        await Dispatcher.UIThread.InvokeAsync(_treeViewModel.Update);
    }

    partial void OnConvertChanged(bool value)
    {
        _audioManager.Convert = value;

        ConfigManager.Instance.Convert = value;
        ConfigManager.Instance.Save();
    }

    partial void OnPlaylistChanged(bool value)
    {
        _audioManager.Playlist = value;

        ConfigManager.Instance.Playlist = value;
        ConfigManager.Instance.Save();
    }
}
