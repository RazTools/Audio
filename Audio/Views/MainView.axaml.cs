﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Audio.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    private string _lastOpenDirectory = "";
    public MainView()
    {
        InitializeComponent();
        ViewModel ??= new MainViewModel();
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
    private async Task<string> PickFolder()
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
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
        var folder = await PickFolder();

        if (!string.IsNullOrEmpty(folder))
        {
            ViewModel.LoadFolder(folder);
        }
    }
    private async void LoadVO_Click(object? sender, RoutedEventArgs e)
    {
        var file = await PickFile(new FilePickerFileType[] { FilePickerFileTypes.TextPlain });

        if (!string.IsNullOrEmpty(file))
        {
            ViewModel.LoadVO(file);
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
            var folder = await PickFolder();

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
}