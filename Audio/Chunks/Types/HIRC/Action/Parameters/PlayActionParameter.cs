namespace Audio.Chunks.Types.HIRC;
public record PlayActionParameter : ValueActionParameter
{
    public FNVID<uint> BankID { get; set; }

    public PlayActionParameter()
    {
        BankID = 0;
    }

    public override void ReadParameters(BankReader reader)
    {
        BankID = reader.ReadUInt32();
    }
}