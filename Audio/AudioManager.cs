using Audio.Chunks;
using Audio.Chunks.Types.HIRC;
using Audio.Entries;
using Audio.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Audio;

public class AudioManager
{
    private readonly List<Chunk> _loadedChunks = [];

    public bool Convert = false;
    public bool Playlist = false;

    public List<EventInfo> Events = [];

    public IEnumerable<AKPK> LoadedPackages => _loadedChunks.OfType<AKPK>();
    public IEnumerable<Entry> Entries
    {
        get
        {
            foreach(AKPK akpk in LoadedPackages)
            {
                foreach(Entry entry in akpk.Entries)
                {
                    yield return entry;
                }
            }
        }
    }
    public IEnumerable<HIRC> Hierarchies
    {
        get
        {
            foreach(Bank bank in Entries.OfType<Bank>())
            {
                if (bank.BKHD?.GetChunk(out HIRC? hirc) == true)
                {
                    hirc.Manager ??= this;
                    yield return hirc;
                }
            }
        }
    }

    public void Clear()
    { 
        _loadedChunks.Clear();
        Events.Clear();
        FNVID<uint>.Clear();
        FNVID<ulong>.Clear();
    }
    public void LoadFiles(string[] paths)
    {
        Logger.Info($"Loading {paths.Length} files...");

        int counter = 0;
        foreach (string path in paths)
        {
            if (TryLoadFile(path, out Chunk? chunk))
            {
                Logger.Progress($"Loaded {Path.GetFileName(path)}", ++counter, paths.Length);
                _loadedChunks.Add(chunk);
            }
        }
    }

    public static bool TryLoadFile(string path, [NotNullWhen(true)] out Chunk? chunk)
    {
        chunk = null;
        try
        {
            using FileStream fs = File.OpenRead(path);
            using BankReader reader = new(fs);
            
            if (Chunk.TryParse(reader, out chunk))
            {
                if (chunk is AKPK akpk)
                {
                    akpk.Source = path;
                    akpk.LoadBanks();
                    return true;
                }
                else if (chunk is BKHD bkhd)
                {
                    bkhd.Source = path;
                    return true;
                }
            }          
        }
        catch (Exception ex)
        {
            Logger.Error($"Error while loading file {path}: {ex}");
        }

        Logger.Warning($"Unable to load file {path} !!");

        return false;
    }
    public int UpdatedEvents(string[] events)
    {
        int matched = 0;

        for (int i = 0; i < events.Length; i++)
        {
            string eventName = events[i];
            if (FNVID<uint>.TryMatch(eventName, out FNVID<uint>? match))
            {
                Logger.Progress($"Found match {match.Value.Value} with {eventName}", ++matched, events.Length);
            }
            else
            {
                Logger.Warning($"Unable to find a match for {eventName}");
            }
        }

        Logger.Info($"Matched {matched} out of {FNVID<uint>.Count()} IDs !!");

        return matched;
    }
    public int UpdateExternals(string[] externalsPaths)
    {

        int externalsCount = Entries.OfType<External>().Count();

        int matched = 0;
        for (int i = 0; i < externalsPaths.Length; i++)
        {
            string externalName = externalsPaths[i];
            if (FNVID<ulong>.TryMatch(externalName, out FNVID<ulong>? match))
            {
                Logger.Progress($"Found match {match.Value.Value} with {externalName}", ++matched, externalsCount);
            }
            else
            {
                Logger.Warning($"Unable to find a match for {externalName}");
            }
        }

        Logger.Info($"Matched {matched} out of {externalsCount} externals !!");

        return matched;
    }
    public void ProcessEvents()
    {
        IEnumerable<Event> events = Hierarchies.SelectMany(x => x.Objects.OfType<Event>());
        int count = events.Count();

        int resolved = 0;
        foreach (Event evt in events)
        {
            EventInfo eventInfo = new(evt.ID);
            evt.HIRC?.ResolveObject(evt, eventInfo);
            Events.Add(eventInfo);

            Logger.Progress($"Resolved {evt.ID} with {eventInfo.TargetIDs.Count()} target audio files and {eventInfo.TagsByID.SelectMany(x => x.Value).ToHashSet().Count} tags", ++resolved, count);
        }
    }
    public void DumpHierarchies(string outputDirectory)
    {
        int count = Entries.OfType<Bank>().Count(x => x.BKHD?.GetChunk<HIRC>(out _) == true);

        int dumped = 0;
        foreach (Bank bank in Entries.OfType<Bank>())
        {
            if (bank.BKHD?.GetChunk(out HIRC? hirc) == true && !string.IsNullOrEmpty(bank.Location))
            {
                hirc.Manager ??= this;
                string jsonPath = Path.ChangeExtension(bank.Location, ".json");
                try
                {
                    string outputPath = Path.Combine(outputDirectory, "Hierarchies", jsonPath);
                    hirc.Dump(outputPath);

                    Logger.Progress($"Dumped {jsonPath}", ++dumped, count);
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to dump {jsonPath}, {e}");
                }
            }
        }

        Logger.Info($"Dumped {dumped} out of {count} hierarchies !!");
    }
    public void DumpEntries(string outputDirectory, IEnumerable<EntryType> types)
    {
        Entry[] entries = Entries.OfType<Entry>().Where(x => types.Contains(x.Type)).ToArray();

        PlaylistWriter? writer = null;
        if (Playlist && types.Any(x => x != EntryType.Bank))
        {
            string playlistOutputPath = Path.Combine(outputDirectory, "Audio");
            Directory.CreateDirectory(playlistOutputPath);
            writer = new(playlistOutputPath);
        }

        int dumped = 0;
        foreach (Entry? entry in entries)
        {
            string outputPath = Path.Combine(outputDirectory, entry.Type == EntryType.Bank ? "Bank" : "Audio");

            if (entry.TryDump(outputPath, Convert, out string? entryLocation))
            {
                Logger.Progress($"Dumped {entryLocation}", ++dumped, entries.Length);

                if (Playlist && entry.Type != EntryType.Bank && entry is TaggedEntry<uint> taggedEntry)
                {
                    foreach (EventInfo evt in Events)
                    {
                        StringBuilder sb = new();
                        if (evt.TargetIDs.Contains(taggedEntry.ID))
                        {
                            foreach (EventTag tag in evt.GetValues(taggedEntry.ID))
                            {
                                sb.Append($"[{tag.Type}={tag.Value}]");
                            }

                            writer?.WriteTrack(entryLocation, evt.ID.String, sb.ToString());
                        }
                    }
                }
            }
            else
            {
                Logger.Warning($"Unable to dump {entryLocation}");
            }
        }

        writer?.Dispose();

        Logger.Info($"Dumped {dumped} out of {entries.Length} entries !!");
    }
    internal void ResolveObject(FNVID<uint> id, EventInfo eventInfo)
    {
        foreach (HIRC hirc in Hierarchies)
        {
            hirc.ResolveObject(id, eventInfo);
        }
    }
}