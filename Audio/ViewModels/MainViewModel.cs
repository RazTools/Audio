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
using System.Diagnostics;

namespace Audio.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private ReadOnlyObservableCollection<Entry> _filteredEntries;
    private string _searchText;
    private string _statusText;
    private double _progressValue;
    public ReadOnlyObservableCollection<Entry> FilteredEntries => _filteredEntries;
    public List<Entry> SelectedEntries => EntrySource.RowSelection!.SelectedItems.ToList();
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
    public double ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }
    public List<Package> Packages { get; set; }
    public SourceList<Entry> Entries { get; set; }
    public ObservableCollection<Folder> Folders { get; set; }
    public FlatTreeDataGridSource<Entry> EntrySource { get; set; }
    public string ClipboardText { get; set; }
    public MainViewModel()
    {
        SearchText = "";
        ClipboardText = "";
        StatusText = "";

        ProgressHelper.Instance = new Progress<double>(value => ProgressValue = value);

        Packages = new List<Package>();
        Entries = new SourceList<Entry>();
        Folders = new ObservableCollection<Folder>();
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
            }
        };
        EntrySource.RowSelection!.SingleSelect = false;

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
    public async void ExportAudios(string outputDir) => await Task.Run(() => Export(Entries.Items.Where(x => x is not Bank).ToList(), outputDir));
    public async void ExportBanks(string outputDir) => await Task.Run(() => Export(Entries.Items.Where(x => x is Bank).ToList(), outputDir));
    public async void ExportAll(string outputDir) => await Task.Run(() => Export(Entries.Items.ToList(), outputDir));
    public async void LoadVO(string path) => await Task.Run(() => LoadVOInternal(path));
    public async void GenerateTXTP(string wwiser, string file) => await Task.Run(() => GenerateTXTPInternal(wwiser, file));
    public void SelectAll()
    {
        for (int i = 0; i < EntrySource.Rows.Count; i++)
        {
            EntrySource.RowSelection!.Select(i);
        }
    }
    public async void Search()
    {
        await Task.Run(Refresh);
        StatusText = "Updated !!";
    }
    private async void LoadPaths(string[] paths)
    {
        Entries.Clear();
        Packages.Clear();
        Folders.Clear();
        Package.BankIDToNames.Clear();

        StatusText = $"Loading {paths.Length} files...";

        ProgressHelper.Reset();
        for (int i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            var package = await ParsePackage(path);
            Packages.Add(package);
            ProgressHelper.Report(i, paths.Length);
        }

        Folders.AddRange(Packages.SelectMany(x => x.Folders).DistinctBy(x => x.Name).ToList());

        var banks = Packages.SelectMany(x => x.Banks).Cast<Bank>().ToList();

        StatusText = $"Processing {banks.Count} banks...";

        ProgressHelper.Reset();
        for (int i = 0; i < banks.Count; i++)
        {
            var bank = banks[i];
            Entries.AddRange(bank.EmbeddedSounds);
            foreach (var kv in bank.BankIDToName)
            {
                Package.BankIDToNames.TryAdd(kv.Key, kv.Value);
            }
            ProgressHelper.Report(i, banks.Count);
        }

        StatusText = $"Mapping {Package.BankIDToNames.Count} found bank names...";

        ProgressHelper.Reset();
        for (int i = 0; i < banks.Count; i++)
        {
            var bank = banks[i];
            if (Package.BankIDToNames.TryGetValue(bank.ID, out var name))
            {
                bank.Name = name;
            }
            ProgressHelper.Report(i, banks.Count);
        }

        var sounds = Packages.SelectMany(x => x.Sounds).Cast<Entry>().ToList();
        var externals = Packages.SelectMany(x => x.Externals).Cast<Entry>().ToList();
        var entries = banks.Concat(sounds).Concat(externals).ToArray();

        StatusText = $"Listing {entries.Length} entries...";

        ProgressHelper.Reset();
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            Entries.Add(entry);
            ProgressHelper.Report(i, entries.Length);
        }

        Refresh();
        StatusText = "Loaded !!";
    }
    private async void LoadVOInternal(string path)
    {
        StatusText = "Parsing VO file...";

        var voMap = new Dictionary<ulong, string>();

        var vos = await File.ReadAllLinesAsync(path);
        ProgressHelper.Reset();
        for (int i = 0; i < vos.Length; i++)
        {
            var vo = vos[i];
            voMap.TryAdd(FNV1.Compute64(vo), vo);
            ProgressHelper.Report(i, vos.Length);
        }

        StatusText = $"Matching {voMap.Count} vo entries with externals...";

        var matched = 0;
        var externals = Entries.Items.OfType<External>().ToArray();
        ProgressHelper.Reset();
        for (int i = 0; i < externals.Length; i++)
        {
            var external = externals[i];
            if (voMap.TryGetValue(external.ID, out var name))
            {
                external.Name = name;
                matched++;
            }
            ProgressHelper.Report(i, externals.Length);
        }

        Refresh();
        StatusText = $"VO file {Path.GetFileName(path)} Loaded Successfully, Matched {matched} out of {externals.Length} externals !!";
    }
    private void Export(List<Entry> entries, string outputDir)
    {
        var count = entries.Count();

        StatusText = $"Exporting {count} files...";

        ProgressHelper.Reset();
        for (int i = 0; i < count; i++)
        {
            var entry = entries[i];
            var outputPath = Path.Combine(outputDir, entry.Location);
            DumpEntry(entry, outputPath);
            ProgressHelper.Report(i, count);
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
    private void DumpTXTH(Entry entry, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        File.CreateText(outputPath).Close();

        using var fs = File.OpenWrite($"{outputPath}.txth");
        using var writer = new StreamWriter(fs);
        writer.WriteLine($"body_file = {Path.GetRelativePath(outputPath, entry.Source)}");
        writer.WriteLine($"subfile_offset = {entry.Offset}");
        writer.WriteLine($"subfile_size = {entry.Size}");
        writer.WriteLine("subfile_extension = wem");
        writer.Close();
    }
    private bool EntryFilter(Entry entry)
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
    private void Refresh()
    {
        Entries.Connect().Filter(EntryFilter).Bind(out _filteredEntries).Subscribe();
        Dispatcher.UIThread.Invoke(() => EntrySource.Items = FilteredEntries);
    }
    private async Task<Package> ParsePackage(string path)
    {
        var package = await Task.Run(() => Package.Parse(path));
        return package;
    }
    private async void GenerateTXTPInternal(string wwiser, string file)
    {
        StatusText = "Exporting banks temporarly to temp folder...";

        var tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
        Directory.CreateDirectory(tempDir);

        var banks = SelectedEntries.OfType<Bank>().ToList();
        if (banks.Count == 0)
        {
            StatusText = "No selected banks, processing all banks...";
            banks = Entries.Items.OfType<Bank>().ToList();
        }
        if (banks.Count == 0)
        {
            StatusText = "No banks found !!";
            return;
        }
        foreach (var bank in banks)
        {
            var outputPath = Path.Combine(tempDir, bank.Location);
            DumpEntry(bank, outputPath);
        }

        if (Folders.Any(x => x.IsChecked))
        {
            foreach (var folder in Folders)
            {
                if (!folder.IsChecked)
                    continue;

                StatusText = $"Invoking wwiser for language {folder.Name}...";

                var txtpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "txtp", folder.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(txtpDir));

                var startInfo = new ProcessStartInfo();
                startInfo.FileName = "python";
                startInfo.Arguments = string.Join(' ', new string[] { wwiser, Path.Combine(tempDir, "**/*.bnk"), "-g", "-gbs", "-te", "-nl", file, "-gl", folder.Name, "-go", txtpDir });
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                using var process = Process.Start(startInfo);
                process.WaitForExit();

                ExportTXTP(banks, txtpDir);
            }
        }
        else
        {
            StatusText = "Invoking wwiser...";

            var txtpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "txtp");

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "python";
            startInfo.Arguments = string.Join(' ', new string[] { wwiser, Path.Combine(tempDir, "**/*.bnk"), "-g", "-gbs", "-te", "-nl", file });
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            using var process = Process.Start(startInfo);
            process.WaitForExit();

            ExportTXTP(banks, txtpDir);
        }

        Directory.Delete(tempDir, true);

        StatusText = "TXTP generated successfully !!";
    }

    private void ExportTXTP(List<Bank> banks, string txtpDir)
    {
        var sounds = Packages.SelectMany(x => x.Sounds).Cast<Sound>().ToList();

        var wemDir = Path.Combine(txtpDir, "wem");

        if (!Directory.Exists(txtpDir))
        {
            StatusText = "No TXTP found !!";
            return;
        }

        var files = Directory.GetFiles(txtpDir, "*.txtp");
        StatusText = $"Found {files.Length} TXTP, proccessing...";
        foreach (var f in files)
        {
            var lines = File.ReadAllLines(f);
            foreach (var line in lines)
            {
                if (line.StartsWith('#'))
                    break;

                var match = Regex.Match(line, "wem/(\\d+).wem");
                if (match.Success)
                {
                    var soundIDString = match.Groups[1].Value;
                    if (ulong.TryParse(soundIDString, out var soundID))
                    {
                        var sound = sounds.FirstOrDefault(x => x.ID == soundID);
                        if (sound != null)
                        {
                            var outputPath = Path.Combine(wemDir, $"{soundID}.wem");
                            DumpTXTH(sound, outputPath);
                            continue;
                        }
                    }

                    var embeddedIDString = match.Groups[1].Value;
                    if (ulong.TryParse(embeddedIDString, out var embeddedID))
                    {
                        var bank = banks.FirstOrDefault(x => x.EmbeddedSounds.Any(x => x.ID == embeddedID));
                        if (bank != null)
                        {
                            var embeddedSound = bank.EmbeddedSounds.FirstOrDefault(x => x.ID == embeddedID);
                            if (embeddedSound != null)
                            {
                                var outputPath = Path.Combine(wemDir, $"{embeddedID}.wem");
                                DumpTXTH(embeddedSound, outputPath);
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }
}
