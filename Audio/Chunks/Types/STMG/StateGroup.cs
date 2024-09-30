namespace Audio.Chunks.Types.STMG;
public record StateGroup : IBankReadable
{
    public FNVID<uint> StateGroupID { get; set; }
    public uint DefaultTransitionTime { get; set; }
    public StateTransition[] Transitions { get; set; } = [];

    public StateGroup()
    {
        StateGroupID = 0;
    }

    public void Read(BankReader reader)
    {
        StateGroupID = reader.ReadUInt32();
        DefaultTransitionTime = reader.ReadUInt32();

        int transitionCount = reader.ReadInt32();
        Transitions = new StateTransition[transitionCount];
        for (int i = 0; i < transitionCount; i++)
        {
            Transitions[i] = new();
            Transitions[i].Read(reader);
        }
    }
}
