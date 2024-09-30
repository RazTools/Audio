namespace Audio.Chunks.Types.STMG;
public record StateTransition : IBankReadable
{
    public FNVID<uint> StateFrom { get; set; }
    public FNVID<uint> StateTo { get; set; }
    public uint TransitionTime { get; set; }

    public StateTransition()
    {
        StateFrom = 0;
        StateTo = 0;
    }

    public void Read(BankReader reader)
    {
        StateFrom = reader.ReadUInt32();
        StateTo = reader.ReadUInt32();
        TransitionTime = reader.ReadUInt32();
    }
}
