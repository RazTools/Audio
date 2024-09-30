namespace Audio.Chunks.Types.HIRC;

public record TrackSwitchInfo : IBankReadable
{
    public GroupType GroupType { get; set; }
    public FNVID<uint> GroupID { get; set; }
    public uint DefaultSwitch { get; set; }
    public FNVID<uint>[] SwitchAssociations { get; set; } = [];

    public TrackSwitchInfo()
    {
        GroupID = 0;
    }

    public void Read(BankReader reader)
    {
        GroupType = (GroupType)reader.ReadByte();
        GroupID = reader.ReadUInt32();
        DefaultSwitch = reader.ReadUInt32();

        int switchAssociationCount = reader.ReadInt32();
        SwitchAssociations = new FNVID<uint>[switchAssociationCount];
        for (int i = 0; i < switchAssociationCount; i++)
        {
            SwitchAssociations[i] = reader.ReadUInt32();
        }
    }
}