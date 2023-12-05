using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Audio.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;

namespace Audio.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    private string _lastOpenDirectory = "";
    public MainView()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        AddHandler(DragDrop.DragEnterEvent, MainView_DragEnter);
        AddHandler(DragDrop.DropEvent, MainView_Drop);
        AddHandler(UnloadedEvent, MainView_Unloaded);
    }
    private void MainView_Unloaded(object? sender, RoutedEventArgs e)
    {
        ViewModel.Dispose();
    }
    private void MainView_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.GetDataFormats().Contains("Files"))
        {
            e.DragEffects = DragDropEffects.Move;
        }
    }
    private void MainView_Drop(object? sender, DragEventArgs e)
    {
        var files = e.Data.Get("Files");
        if (files is IEnumerable<IStorageItem> storageFiles)
        {
            var paths = storageFiles.Select(x => x.TryGetLocalPath()).ToArray();
            if (paths.Length == 1 && Directory.Exists(paths[0]))
            {
                ViewModel.LoadFolder(paths[0]);
            }
            if (paths.Length > 0 && !paths.Any(Directory.Exists))
            {
                ViewModel.LoadFiles(paths);
            }
        }
    }
    private async Task<string[]> PickFileInternal(bool allowMultiple = false, FilePickerFileType[] types = null)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = allowMultiple,
            FileTypeFilter = types ?? Array.Empty<FilePickerFileType>(),
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(_lastOpenDirectory)
        });

        if (files.Count > 0)
        {
            var dir = await files.First().GetParentAsync();
            _lastOpenDirectory = dir.ToString();

            return files.Select(x => x.TryGetLocalPath()).ToArray();
        }

        return Array.Empty<string>();
    }
    private async Task<string> PickFolder(string title = "")
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = title,
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(_lastOpenDirectory)
        });

        if (folders.Count == 1)
        {
            var dir = folders.First().TryGetLocalPath();
            _lastOpenDirectory = dir;

            return dir;
        }

        return "";
    }
    private async Task<string> SaveFile(string name = "", string extension = "", string title = "", FilePickerFileType[] types = null)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = name,
            DefaultExtension = extension,
            ShowOverwritePrompt = true,
            FileTypeChoices = types ?? Array.Empty<FilePickerFileType>(),
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(_lastOpenDirectory)
        });

        if (file != null)
        {
            var dir = await file.GetParentAsync();
            _lastOpenDirectory = dir.ToString();

            return file.TryGetLocalPath();
        }

        return "";
    }
    private async Task<string[]> PickFiles(FilePickerFileType[] types = null) => await PickFileInternal(true, types);
    private async Task<string> PickFile(FilePickerFileType[] types = null)
    {
        var files = await PickFileInternal(false, types);
        return files.ElementAtOrDefault(0);
    }

    private async void LoadFile_Click(object? sender, RoutedEventArgs e)
    {
        var files = await PickFiles();

        if (files.Length > 0)
        {
            ViewModel.LoadFiles(files);
        }
    }

    private async void LoadFolder_Click(object? sender, RoutedEventArgs e)
    {
        var folder = await PickFolder("Select Folder");

        if (!string.IsNullOrEmpty(folder))
        {
            ViewModel.LoadFolder(folder);
        }
    }
    private async void LoadDIFF_Click(object? sender, RoutedEventArgs e)
    {
        var src = await PickFolder("Select Source Folder");

        if (!string.IsNullOrEmpty(src))
        {
            var dst = await PickFolder("Select Destination Folder");

            if (!string.IsNullOrEmpty(dst))
            {
                ViewModel.LoadDIFF(src, dst);
            }
        }
    }
    private async void LoadVO_Click(object? sender, RoutedEventArgs e)
    {
        var file = await PickFile([FilePickerFileTypes.TextPlain]);

        if (!string.IsNullOrEmpty(file))
        {
            ViewModel.LoadVO(file);
        }
    }
    private async void SetWWiserPath_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.WWiserPath = await PickFile([new FilePickerFileType("wwiser") { Patterns = new[] { "wwiser.py" } }]);
    }
    private async void SetVGMStreamPath_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.VGMStreamPath = await PickFile([new FilePickerFileType("vgmstream") { Patterns = new[] { "*.*" }}]);
    }
    private async void ExportAudios_Click(object? sender, RoutedEventArgs e)
    {
        var folder = await PickFolder("Select Output Folder");

        if (!string.IsNullOrEmpty(folder))
        {
            ViewModel.ExportAudios(folder);
        }
    }

    private async void ExportBanks_Click(object? sender, RoutedEventArgs e)
    {
        var folder = await PickFolder("Select Output Folder");

        if (!string.IsNullOrEmpty(folder))
        {
            ViewModel.ExportBanks(folder);
        }
    }

    private async void ExportAll_Click(object? sender, RoutedEventArgs e)
    {
        var folder = await PickFolder("Select Output Folder");

        if (!string.IsNullOrEmpty(folder))
        {
            ViewModel.ExportAll(folder);
        }
    }

    private void EntryDataGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsRightButtonPressed && point.Pointer.Captured is TextBlock textBlock)
        {
            ViewModel.ClipboardText = textBlock.Text;
        }
    }

    private async void EntryDataGridCopyText_Click(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        await clipboard.SetTextAsync(ViewModel.ClipboardText);
    }

    private async void EntryDataGridExportSeleted_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedEntries.Count > 0)
        {
            var folder = await PickFolder("Select Output Folder");

            if (!string.IsNullOrEmpty(folder))
            {
                ViewModel.ExportSelectedEntries(folder);
            }
        }
    }

    private void SearchText_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ViewModel.Search();
        }
    }

    private void EntryDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers.Equals(KeyModifiers.Control) && e.Key == Key.A)
        {
            ViewModel.SelectAll();
        }
    }
    private async void GenerateTXTP_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ViewModel.WWiserPath))
        {
            ViewModel.StatusText = "WWiser path must be set first !!";
            return;
        }

        var file = await PickFile([FilePickerFileTypes.TextPlain]);

        if (!string.IsNullOrEmpty(file))
        {
            ViewModel.GenerateTXTP(file);
        }
    }
    private async void DumpInfo_Click(object? sender, RoutedEventArgs e)
    {
        var output = await SaveFile("Packages", "json", "Select Folder", [new FilePickerFileType("Packages Info") { Patterns = new[] { "*.json" } }]);

        if (!string.IsNullOrEmpty(output))
        {
            ViewModel.DumpInfo(output);
        }
    }
    private void Slider_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Slider slider)
        {
            ViewModel.Seek(slider.Value);
        }
    }

    private void TreeDataGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Pointer.Captured is TextBlock _)
        {
            ViewModel.LoadAudio();
        }
    }
}
