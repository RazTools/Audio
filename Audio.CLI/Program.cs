using Audio.CLI.Components;
using Audio.Entries;

namespace Audio.CLI;

public class Program
{
    public static void Main(string[] args) => CommandLine.Init(args);
    public static void Run(Options o)
    {
        Logger.Instance = new ConsoleLogger();

        AudioManager manager = new() { Convert = o.Convert, Playlist = o.Playlist };

        List<string> files = [];
        if (Directory.Exists(o.Input))
        {
            files.AddRange(Directory.GetFiles(o.Input, "*.*", SearchOption.AllDirectories));
        }
        else if (!string.IsNullOrEmpty(o.Input))
        {
            files.Add(o.Input);
        }

        manager.LoadFiles([.. files]);

        if (o.Externals != null)
        {
            manager.UpdateExternals(File.ReadAllLines(o.Externals.FullName));
        }

        if (o.Events != null)
        {
            manager.UpdatedEvents(File.ReadAllLines(o.Events.FullName));
        }

        if (o.Tags)
        {
            manager.ProcessEvents();
        }

        if (o.Output != null)
        {
            o.Output.Create();

            if (o.Hierarchy)
            {
                manager.DumpHierarchies(o.Output.FullName);
            }

            if (o.Bank || o.Audio)
            {
                List<EntryType> types = [];

                if (o.Bank)
                {
                    types.Add(EntryType.Bank);
                }

                if (o.Audio)
                {
                    types.Add(EntryType.Sound);
                    types.Add(EntryType.External);
                    types.Add(EntryType.EmbeddedSound);
                }

                manager.DumpEntries(Path.Combine(o.Output.FullName, "Audio"), types);
            }
        }

        manager.Clear();
    }
}