using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Generic;
using System;
using ReactiveUI;
using System.Threading.Tasks;
using System.Linq;
using Audio.Models.Chunks;
using Audio.Models.Entries;
using DynamicData;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;
using System.Text.RegularExpressions;
using Avalonia.Data;
using Audio.Models.Utils;

namespace Audio.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private ReadOnlyObservableCollection<Entry> _filteredEntries;
    private string _searchText;
    private string _statusText;
    public ReadOnlyObservableCollection<Entry> FilteredEntries => _filteredEntries;
    public IReadOnlyList<Entry> SelectedEntries => EntrySource.RowSelection!.SelectedItems;
    public string SearchText
    {
        get => _searchText;
        set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    Regex.Match("", value, RegexOptions.IgnoreCase);
                }
                catch (ArgumentException)
                {
                    throw new DataValidationException("Not a valid Regex value");
                }
            }

            this.RaiseAndSetIfChanged(ref _searchText, value);
        }
    }
    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }
    public Dictionary<ulong, string> VOMap { get; set; }
    public List<Package> Packages { get; set; }
    public SourceList<Entry> Entries { get; set; }
    public FlatTreeDataGridSource<Entry> EntrySource { get; set; }
    public string ClipboardText { get; set; }
    

    public MainViewModel()
    {
        SearchText = "";
        ClipboardText = "";
        StatusText = "";

        Packages = new List<Package>();
        Entries = new SourceList<Entry>();
        EntrySource = new FlatTreeDataGridSource<Entry>(Array.Empty<Entry>())
        {
            Columns =
            {
                new TextColumn<Entry, EntryType>("Type", x => x.Type, new GridLength(0.6, GridUnitType.Star)),
                new TextColumn<Entry, ulong>("ID", x => x.ID, new GridLength(0.8, GridUnitType.Star)),
                new TextColumn<Entry, string>("Name", x => x.Name, new GridLength(1.5, GridUnitType.Star)),
                new TextColumn<Entry, string>("Folder", x => x.FolderName, new GridLength(0.8, GridUnitType.Star)),
                new TextColumn<Entry, string>("Source", x => x.Source, new GridLength(3, GridUnitType.Star)),
                new TextColumn<Entry, uint>("Offset", x => x.Offset, new GridLength(0.5, GridUnitType.Star)),
                new TextColumn<Entry, uint>("Size", x => x.Size, new GridLength(0.5, GridUnitType.Star))
                ,
            }
        };
        EntrySource.RowSelection!.SingleSelect = false;

        VOMap = new Dictionary<ulong, string>();
    }

    public bool EntryFilter(Entry entry)
    {
        if (string.IsNullOrEmpty(SearchText))
            return true;

        var regex = new Regex(SearchText, RegexOptions.IgnoreCase);
        return regex.IsMatch(entry.Type.ToString())
                || regex.IsMatch(entry.ID.ToString())
                || regex.IsMatch(entry.Name.ToString())
                || regex.IsMatch(entry.Source.ToString())
                || regex.IsMatch(entry.Offset.ToString())
                || regex.IsMatch(entry.Size.ToString())
                || regex.IsMatch(entry.FolderName.ToString());
    }

    public async Task<Package> ParsePackage(string path)
    {
        var package = await Task.Run(() => Package.Parse(path));
        return package;
    }

    public void ParseChunks(Bank bank)
    {
        bank.ParseChunks();
        Entries.AddRange(bank.EmbeddedSounds);
        foreach(var kv in bank.BankIDToName)
        {
            Package.BankIDToNames.TryAdd(kv.Key, kv.Value);
        }
    }

    private async void LoadPaths(string[] paths)
    {
        StatusText = $"Loading {paths.Length} files...";

        Entries.Clear();
        Packages.Clear();
        Package.BankIDToNames.Clear();
        foreach (var path in paths)
        {
            var package = await ParsePackage(path);
            Packages.Add(package);
        }

        var banks = Packages.SelectMany(x => x.Banks).Cast<Entry>().ToList();
        foreach(Bank bank in banks)
        {
            ParseChunks(bank);
        }
        foreach(Bank bank in banks)
        {
            if (Package.BankIDToNames.TryGetValue(bank.ID, out var name))
            {
                bank.Name = name;
            }
        }

        var sounds = Packages.SelectMany(x => x.Sounds).Cast<Entry>().ToList();
        var externals = Packages.SelectMany(x => x.Externals).Cast<Entry>().ToList();
        var entries = banks.Concat(sounds).Concat(externals).ToArray();
        foreach(var entry in entries)
        {
            Entries.Add(entry);
        }

        Refresh();
        StatusText = "Loaded !!";
    }

    public async void LoadFiles(string[] files)
    {
        await Task.Run(() => LoadPaths(files));
    }
    public async void LoadFolder(string folder)
    {
        var paths = Directory.GetFiles(folder, "*.pck", SearchOption.AllDirectories);
        await Task.Run(() => LoadPaths(paths));
    }
    public async void ExportSelectedEntries(string outputDir) => await Task.Run(() => Export(SelectedEntries, outputDir));
    public async void ExportAll(string outputDir) => await Task.Run(() => Export(EntrySource.Items, outputDir));

    public void Export(IEnumerable<Entry> items, string outputDir)
    {
        var count = items.Count();

        StatusText = $"Exporting {count} files...";

        foreach(var entry in items)
        {
            var outputPath = Path.Combine(outputDir, entry.Location);
            DumpEntry(entry, outputPath);
        }

        StatusText = $"{count} files exported successfully !!";
    }
    
    private void DumpEntry(Entry entry, string outputPath)
    {
        var bytes = entry.GetData();

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        using var fs = File.OpenWrite(outputPath);
        fs.Write(bytes);
    }

    public void LoadVO(string path)
    {
        StatusText = "Parsing VO file...";

        VOMap.Clear();
        var vos = File.ReadAllLines(path);
        for (int i = 0; i < vos.Length; i++)
        {
            var vo = vos[i];
            VOMap.TryAdd(FNV1.Compute64(vo), vo);
        }

        var matched = 0;
        var externals = Entries.Items.OfType<External>().ToArray();
        foreach (var external in externals)
        {
            if(VOMap.TryGetValue(external.ID, out var name))
            {
                external.Name = name;
                matched++;
            }
        }

        StatusText = $"VO file {Path.GetFileName(path)} Loaded Successfully, Matched {matched} out of {externals.Length} !!";
        Refresh();
    }

    public void Refresh()
    {
        Entries.Connect().Filter(EntryFilter).Bind(out _filteredEntries).Subscribe();
        Dispatcher.UIThread.Invoke(() => EntrySource.Items = FilteredEntries);
    }

    public void SelectAll()
    {
        for (int i = 0; i < EntrySource.Rows.Count; i++)
        {
            EntrySource.RowSelection!.Select(i);
        }
    }

    public void Search()
    {
        Refresh();
        StatusText = "Updated !!";
    }
}
