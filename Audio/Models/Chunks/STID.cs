using Audio.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace Audio.Models.Chunks;
public record STID : Chunk
{
    public uint StringType { get; set; }
    public Dictionary<uint, string> BankIDToName { get; set; }

    public STID(Chunk chunk) : base(chunk)
    {
        BankIDToName = new Dictionary<uint, string>();
    }

    public new void Parse(BinaryReader reader)
    {
        StringType = reader.ReadUInt32();
        var count = reader.ReadInt32();
        BankIDToName.EnsureCapacity(count);
        for (int i = 0; i < count; i++)
        {
            var id = reader.ReadUInt32();
            var nameLength = reader.ReadByte();
            var name = reader.ReadRawString(nameLength);
            BankIDToName[id] = name;
        }
    }
}