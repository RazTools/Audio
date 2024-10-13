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

    public IEnumerable<Entry> Entries
    {
        get
        {
            foreach(Chunk chunk in _loadedChunks)
            {
                switch (chunk)
                {
                    case AKPK akpk:
                        foreach (Entry entry in akpk.Entries)
                        {
                            yield return entry;
                        }
                        
                        break;
                    case BKHD bkhd:
                        Bank bank = new(bkhd);

                        foreach (Entry entry in bank.Entries)
                        {
                            yield return entry;
                        }

                        break;
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
        FNVID<uint>.Clear();
        FNVID<ulong>.Clear();
    }
    public int LoadFiles(string[] paths)
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

        return _loadedChunks.Count;
    }

    public static bool TryLoadFile(string path, [NotNullWhen(true)] out Chunk? chunk)
    {
        chunk = null;
        try
        {
            if (Path.Exists(path))
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

        List<EventInfo> eventInfos = [];

        int resolved = 0;
        foreach(Event evt in events)
        {
            EventInfo eventInfo = new(evt.ID);
            evt.HIRC?.ResolveObject(evt, eventInfo);
            eventInfos.Add(eventInfo);

            Logger.Progress($"Resolved {evt.ID} with {eventInfo.TargetIDs.Count()} target audio files", ++resolved, count);
        }

        Logger.Info("Mapping events to entries !!");

        foreach (TaggedEntry<uint> taggedEntry in Entries.OfType<TaggedEntry<uint>>())
        {
            foreach (EventInfo eventInfo in eventInfos)
            {
                if (eventInfo.TagsByID.TryGetValue(taggedEntry.ID, out HashSet<EventTag>? eventTags))
                {
                    if (!taggedEntry.Events.TryGetValue(eventInfo.ID, out HashSet<EventTag>? tags))
                    {
                        taggedEntry.Events[eventInfo.ID] = tags = [];
                    }

                    foreach (EventTag tag in eventTags)
                    {
                        tags.Add(tag);
                    }
                }
            }
        }

        Logger.Info("Done Processing !!");
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
        DumpEntries(outputDirectory, entries);
    }
    public void DumpEntries(string outputDirectory, IEnumerable<Entry> entries)
    {
        PlaylistWriter? writer = null;
        if (Playlist && entries.Any(x => x.Type != EntryType.Bank))
        {
            string playlistOutputPath = Path.Combine(outputDirectory, "Audio");
            Directory.CreateDirectory(playlistOutputPath);
            writer = new(playlistOutputPath);
        }

        int dumped = 0;
        int count = entries.Count();
        foreach (Entry? entry in entries)
        {
            string outputPath = Path.Combine(outputDirectory, entry.Type == EntryType.Bank ? "Bank" : "Audio");

            if (entry.TryDump(outputPath, Convert, out string? entryLocation))
            {
                Logger.Progress($"Dumped {entryLocation}", ++dumped, count);

                if (Playlist && entry.Type != EntryType.Bank && entry is TaggedEntry<uint> taggedEntry)
                {
                    foreach(KeyValuePair<FNVID<uint>, HashSet<EventTag>> kvp in taggedEntry.Events)
                    {
                        StringBuilder sb = new();

                        foreach (EventTag tag in kvp.Value)
                        {
                            sb.Append($"[{tag.Type}={tag.Value}]");
                        }

                        writer?.WriteTrack(entryLocation, kvp.Key.String, sb.ToString());
                    }
                }
            }
            else
            {
                Logger.Warning($"Unable to dump {entryLocation}");
            }
        }

        writer?.Dispose();

        Logger.Info($"Dumped {dumped} out of {count} entries !!");
    }
    internal void ResolveObject(FNVID<uint> id, EventInfo eventInfo)
    {
        foreach (HIRC hirc in Hierarchies)
        {
            hirc.ResolveObject(id, eventInfo);
        }
    }
}