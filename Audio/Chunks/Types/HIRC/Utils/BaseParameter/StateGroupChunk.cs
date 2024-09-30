namespace Audio.Chunks.Types.HIRC;

public record StateGroupChunk : IBankReadable
{
    public FNVID<uint> StateGroupID { get; set; }
    public SyncType StateSyncType { get; set; }
    public StateInfo[] States { get; set; } = [];

    public StateGroupChunk()
    {
        StateGroupID = 0;
    }

    public void Read(BankReader reader)
    {
        StateGroupID = reader.ReadUInt32();
        StateSyncType = (SyncType)reader.ReadByte();

        int stateCount = reader.Read7BitEncodedInt();
        States = new StateInfo[stateCount];
        for (int i = 0; i < stateCount; i++)
        {
            States[i] = new();
            States[i].Read(reader);
        }
    }
}