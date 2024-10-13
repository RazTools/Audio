using Audio.Entries;

namespace Audio.Chunks;
public record AKPK : Chunk
{
    public new const string Signature = "AKPK";

    public bool IsLittleEndian { get; set; }
    public long FolderListSize { get; set; }
    public long BankTableSize { get; set; }
    public long SoundTableSize { get; set; }
    public long ExternalTableSize { get; set; }
    public Dictionary<uint, string> FoldersDict { get; set; } = [];
    public Folder[] Folders { get; set; } = [];
    public Bank[] Banks { get; set; } = [];
    public Sound[] Sounds { get; set; } = [];
    public External[] Externals { get; set; } = [];

    public IEnumerable<Entry> Entries
    {
        get
        {
            foreach (Bank bank in Banks)
            {
                foreach (Entry entry in bank.Entries)
                {
                    yield return entry;
                }
            }

            foreach (Sound sound in Sounds)
            {
                yield return sound;
            }

            foreach (External external in Externals)
            {
                yield return external;
            }
        }
    }

    public AKPK(HeaderInfo header) : base(header) { }

    public override void Read(BankReader reader)
    {
        IsLittleEndian = Convert.ToBoolean(reader.ReadInt32());
        if (!IsLittleEndian)
            throw new IOException("Package must be in Little-Endian format. Big-Endian format is nott supported");

        FolderListSize = reader.ReadInt32();
        BankTableSize = reader.ReadInt32();
        SoundTableSize = reader.ReadInt32();
        ExternalTableSize = reader.ReadInt32();

        ReadFolders(reader);
        ReadBanks(reader);
        ReadSounds(reader);
        ReadExternals(reader);
    }

    public void LoadBanks()
    {
        foreach (Bank bank in Banks)
        {
            bank.Parse(this);
        }
    }

    private void ReadFolders(BinaryReader reader)
    {
        long offset = reader.BaseStream.Position;

        int count = reader.ReadInt32();
        Folders = new Folder[count];
        FoldersDict = new Dictionary<uint, string>(count);
        for (int i = 0; i < count; i++)
        {
            Folder folder = new(offset);
            try
            {
                folder.Read(reader);
                Folders[i] = folder;
                FoldersDict.Add(folder.ID, folder.Name ?? "");
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Duplicated Entry: Name: {folder.Name}, ID: {folder.ID}");
            }
        }

        reader.BaseStream.Position = offset + FolderListSize;
    }

    private void ReadBanks(BankReader reader)
    {
        int count = reader.ReadInt32();
        Banks = new Bank[count];
        for (int i = 0; i < count; i++)
        {
            Bank bank = new() { Parent = this };
            bank.Read(reader);
            Banks[i] = bank;
        }
    }

    private void ReadSounds(BankReader reader)
    {
        int count = reader.ReadInt32();
        Sounds = new Sound[count];
        for (int i = 0; i < count; i++)
        {
            Sound sound = new() { Parent = this };
            sound.Read(reader);
            Sounds[i] = sound;
        }
    }
    private void ReadExternals(BankReader reader)
    {
        int count = reader.ReadInt32();
        Externals = new External[count];
        for (int i = 0; i < count; i++)
        {
            External external = new() { Parent = this };
            external.Read(reader);
            Externals[i] = external;
        }
    }
}