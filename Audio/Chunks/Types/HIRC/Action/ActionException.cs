namespace Audio.Chunks.Types.HIRC;

public record ActionException : IBankReadable
{
    public FNVID<uint> ID { get; set; }
    public bool IsBus { get; set; }

    public ActionException()
    {
        ID = 0;
    }

    public void Read(BankReader reader)
    {
        ID = reader.ReadUInt32();
        IsBus = reader.ReadByte() == 1;
    }
}