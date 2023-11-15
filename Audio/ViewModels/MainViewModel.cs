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
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Security.Cryptography;

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
    public bool RawAudio { get; set; }
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
    public async void LoadDIFF(string src, string dst) => await Task.Run(() => LoadDIFFInternal(src, dst));
    public async void DumpInfo(string output) => await Task.Run(() => DumpInfoInternal(output));
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

        Packages = await LoadPackages(paths);

        Folders.AddRange(Packages.SelectMany(x => x.Folders).DistinctBy(x => x.Name).ToList());

        var banks = Packages.SelectMany(x => x.Banks).Cast<Bank>().ToList();
        var sounds = Packages.SelectMany(x => x.Sounds).Cast<Entry>().ToList();
        var externals = Packages.SelectMany(x => x.Externals).Cast<Entry>().ToList();
        var entries = banks.Concat(sounds).Concat(externals).ToArray();

        StatusText = $"Listing {entries.Length} entries...";

        ProgressHelper.Reset();
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry is Bank bank)
            {
                Entries.AddRange(bank.EmbeddedSounds);
            }
            Entries.Add(entry);
            ProgressHelper.Report(i, entries.Length);
        }

        Refresh();
        StatusText = "Loaded !!";
    }
    private async Task<List<Package>> LoadPackages(string[] paths)
    {
        var packages = new List<Package>();
        var bankIDToNames = new Dictionary<ulong, string>();

        StatusText = $"Loading {paths.Length} files...";

        ProgressHelper.Reset();
        for (int i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            var package = await ParsePackage(path);
            packages.Add(package);
            ProgressHelper.Report(i, paths.Length);
        }

        var banks = packages.SelectMany(x => x.Banks).Cast<Bank>().ToList();

        StatusText = $"Processing {banks.Count} banks...";

        ProgressHelper.Reset();
        for (int i = 0; i < banks.Count; i++)
        {
            var bank = banks[i];
            foreach (var kv in bank.BankIDToName)
            {
                bankIDToNames.TryAdd(kv.Key, kv.Value);
            }
            ProgressHelper.Report(i, banks.Count);
        }

        StatusText = $"Mapping {bankIDToNames.Count} found bank names...";

        ProgressHelper.Reset();
        for (int i = 0; i < banks.Count; i++)
        {
            var bank = banks[i];
            if (bankIDToNames.TryGetValue(bank.ID, out var name))
            {
                bank.Name = name;
            }
            ProgressHelper.Report(i, banks.Count);
        }

        return packages;
    }
    private async void LoadDIFFInternal(string src, string dst)
    {
        Entries.Clear();
        Packages.Clear();
        Folders.Clear();

        var srcPaths = Directory.GetFiles(src, "*.pck", SearchOption.AllDirectories);
        var dstPaths = Directory.GetFiles(dst, "*.pck", SearchOption.AllDirectories);

        var srcPackages = await LoadPackages(srcPaths);
        var dstPackages = await LoadPackages(dstPaths);

        var sounds = srcPackages.SelectMany(x => x.Sounds).Cast<Entry>();
        var externals = srcPackages.SelectMany(x => x.Externals).Cast<Entry>();
        var embeddedSounds = srcPackages.SelectMany(x => x.Banks).SelectMany(x => x.EmbeddedSounds).Cast<Entry>();

        var srcEntries = sounds.Concat(externals).Concat(embeddedSounds).ToList();

        sounds = dstPackages.SelectMany(x => x.Sounds).Cast<Entry>();
        externals = dstPackages.SelectMany(x => x.Externals).Cast<Entry>();
        embeddedSounds = dstPackages.SelectMany(x => x.Banks).SelectMany(x => x.EmbeddedSounds).Cast<Entry>();

        var dstEntries = sounds.Concat(externals).Concat(embeddedSounds).ToList();

        StatusText = "Comparing source and destination...";

        var diff = new List<Entry>();
        ProgressHelper.Reset();
        for (int i = 0; i < dstEntries.Count; i++)
        {
            var entry = dstEntries[i];
            ProgressHelper.Report(i, dstEntries.Count);

            var matched = false;
            var targets = srcEntries.Where(x => x.ID == entry.ID && x.Type == entry.Type);
            foreach (var target in targets)
            {
                if (entry.Size == target.Size)
                {
                    var entryHash = MD5.HashData(entry.GetData());
                    var targetHash = MD5.HashData(target.GetData());

                    if (entryHash.SequenceEqual(targetHash))
                    {
                        matched = true;
                        break;
                    }
                }
            }

            if (matched)
            {
                continue;
            }

            diff.Add(entry);
        }

        StatusText = $"Found {diff.Count} differences !!";

        Packages = diff.Select(x => x.Package).Distinct().ToList();
        Folders.AddRange(Packages.SelectMany(x => x.Folders).DistinctBy(x => x.Name).ToList());

        Entries.AddRange(diff);
        Refresh();
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
    private void GenerateTXTPInternal(string wwiser, string file)
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
                startInfo.ArgumentList.Add(wwiser);
                startInfo.ArgumentList.Add(Path.Combine(tempDir, "**/*.bnk"));
                startInfo.ArgumentList.Add("-g");
                startInfo.ArgumentList.Add("-gbs");
                startInfo.ArgumentList.Add("-te");
                startInfo.ArgumentList.Add("-nl");
                startInfo.ArgumentList.Add(file);
                startInfo.ArgumentList.Add("-gl");
                startInfo.ArgumentList.Add(folder.Name);
                startInfo.ArgumentList.Add("-go");
                startInfo.ArgumentList.Add(txtpDir);
                startInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                startInfo.UseShellExecute = true;
                using var process = Process.Start(startInfo);
                process.WaitForExit();

                Export(banks, txtpDir, RawAudio ? DumpEntry : DumpTXTH);
            }
        }
        else
        {
            StatusText = "Invoking wwiser...";

            var txtpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "txtp");

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "python";
            startInfo.ArgumentList.Add(wwiser);
            startInfo.ArgumentList.Add(Path.Combine(tempDir, "**/*.bnk"));
            startInfo.ArgumentList.Add("-g");
            startInfo.ArgumentList.Add("-gbs");
            startInfo.ArgumentList.Add("-te");
            startInfo.ArgumentList.Add("-nl");
            startInfo.ArgumentList.Add(file);
            startInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            startInfo.UseShellExecute = true;
            using var process = Process.Start(startInfo);
            process.WaitForExit();


            Export(banks, txtpDir, RawAudio ? DumpEntry : DumpTXTH);
        }

        Directory.Delete(tempDir, true);

        StatusText = "TXTP generated successfully !!";
    }
    private void Export(List<Bank> banks, string txtpDir, Action<Entry, string> exportAction)
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
                            exportAction(sound, outputPath);
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
                                exportAction(embeddedSound, outputPath);
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }
    private void DumpInfoInternal(string output)
    {
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        var str = JsonSerializer.Serialize(Entries.Items, options);

        File.WriteAllText(output, str);

        StatusText = $"Dumped to {output} successfully !!";
    }
}
