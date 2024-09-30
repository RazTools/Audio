namespace Audio.Chunks.Types.HIRC;
public record SetSwitchActionParameter : IActionParameter
{
    public FNVID<uint> SwitchGroupID { get; set; }
    public FNVID<uint> SwitchStateID { get; set; }

    public SetSwitchActionParameter()
    {
        SwitchGroupID = 0;
        SwitchStateID = 0;
    }

    public void Read(BankReader reader)
    {
        SwitchGroupID = reader.ReadUInt32();
        SwitchStateID = reader.ReadUInt32();
    }
}