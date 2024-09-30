namespace Audio.Chunks.Types.HIRC;
public record BankSourceData : IBankReadable
{
    public Plugin Plugin { get; set; }
    public SourceType StreamType { get; set; }
    public MediaInformation MediaInformation { get; set; }

    public BankSourceData()
    {
        Plugin = new();
        MediaInformation = new();
    }

    public void Read(BankReader reader)
    {
        Plugin.Read(reader);
        StreamType = (SourceType)reader.ReadByte();
        MediaInformation.Read(reader);
    }
}
