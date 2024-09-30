namespace Audio.Chunks.Types.HIRC;
public record SetStateActionParameter : IActionParameter
{
    public FNVID<uint> StateGroupID { get; set; }
    public FNVID<uint> TargetStateID { get; set; }

    public SetStateActionParameter()
    {
        StateGroupID = 0;
        TargetStateID = 0;
    }

    public void Read(BankReader reader)
    {
        StateGroupID = reader.ReadUInt32();
        TargetStateID = reader.ReadUInt32();
    }
}