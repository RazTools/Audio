namespace Audio.Chunks.Types.HIRC;

public record GameSync
{
    public FNVID<uint> Group { get; set; }
    public GroupType GroupType { get; set; }

    public GameSync()
    {
        Group = 0;
    }
}