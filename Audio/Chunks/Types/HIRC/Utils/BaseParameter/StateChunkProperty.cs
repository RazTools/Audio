namespace Audio.Chunks.Types.HIRC;

public record StateChunkProperty : IBankReadable
{
    public StatePropertyID ID { get; set; }
    public AccumulationType Type { get; set; }
    public bool InDb { get; set; }

    public void Read(BankReader reader)
    {
        ID = (StatePropertyID)reader.Read7BitEncodedInt();
        Type = (AccumulationType)reader.ReadByte();
        InDb = reader.ReadByte() != 0;
    }
}