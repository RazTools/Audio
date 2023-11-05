using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Audio.Models.Entries;

namespace Audio.Models.Chunks;
public record Package : Chunk
{
    public static Dictionary<ulong, string> BankIDToNames { get; private set; }

    public string Path { get; set; }
    public bool IsLittleEndian { get; set; }
    public long FolderListSize { get; set; }
    public long BankTableSize { get; set; }
    public long SoundTableSize { get; set; }
    public long ExternalTableSize { get; set; }
    public Dictionary<uint, string> Folders { get; set; }
    public Bank[] Banks { get; set; }
    public Sound[] Sounds { get; set; }
    public External[] Externals { get; set; }

    static Package()
    {
        BankIDToNames = new Dictionary<ulong, string>();
    }

    public Package(Chunk chunk) : base(chunk) { }

    public static Package Parse(string path)
    {
        using var fs = File.OpenRead(path);
        using var reader = new BinaryReader(fs);

        var package = ParseChunk(reader) as Package;
        package.Path = path;
        return package;
    }

    public new void Parse(BinaryReader reader)
    {
        IsLittleEndian = Convert.ToBoolean(reader.ReadInt32());
        if (!IsLittleEndian)
            throw new Exception("Package must be in Little-Endian format. Big-Endian format is nott supported");

        FolderListSize = reader.ReadInt32();
        BankTableSize = reader.ReadInt32();
        SoundTableSize = reader.ReadInt32();
        ExternalTableSize = reader.ReadInt32();

        ParseFolders(reader);
        ParseBanks(reader);
        ParseSounds(reader);
        ParseExternals(reader);
    }

    private void ParseFolders(BinaryReader reader)
    {
        var offset = reader.BaseStream.Position;

        var count = reader.ReadInt32();
        Folders = new Dictionary<uint, string>(count);
        for (var i = 0; i < count; i++)
        {
            var folder = Folder.Parse(reader, offset);
            try
            {
                Folders.Add(folder.ID, folder.Name);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Duplicated Entry: Name: {folder.Name}, ID: {folder.ID}");
            }
        }

        reader.BaseStream.Position = offset + FolderListSize;
    }

    private void ParseBanks(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        Banks = new Bank[count];
        for (var i = 0; i < count; i++)
        {
            Banks[i] = Bank.Parse(reader, this);
        }
    }

    private void ParseSounds(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        Sounds = new Sound[count];
        for (var i = 0; i < count; i++)
        {
            Sounds[i] = Sound.Parse(reader, this);
        }
    }
    private void ParseExternals(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        Externals = new External[count];
        for (var i = 0; i < count; i++)
        {
            Externals[i] = External.Parse(reader, this);
        }
    }
}
