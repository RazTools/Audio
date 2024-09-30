namespace Audio.Chunks.Types.HIRC;

public record StateInfo : IBankReadable
{
    public FNVID<uint> StateID { get; set; }
    public FNVID<uint> StateInstanceID { get; set; }

    public StateInfo()
    {
        StateID = 0;
        StateInstanceID = 0;
    }

    public void Read(BankReader reader)
    {
        StateID = reader.ReadUInt32();
        StateInstanceID = reader.ReadUInt32();
    }
}