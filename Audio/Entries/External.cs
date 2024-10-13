﻿namespace Audio.Entries;
public record External : TaggedEntry<ulong>
{
    public override string? Location => string.IsNullOrEmpty(ID.String) ? $"{FolderName}/{Name}.wem" : Name + (Path.GetExtension(Name) == ".wem" ? "" : ".wem");

    public External() : base(EntryType.External) { }
}
