namespace Audio.Chunks.Types.HIRC;

public record Layer : IBankReadable
{
    public FNVID<uint> ID { get; set; }
    public RTPC RTPC { get; set; }
    public FNVID<uint> RTCPID { get; set; }
    public RTPCType RTCPType { get; set; }
    public AssociatedChildData[] AssociatedChildren { get; set; } = [];

    public Layer()
    {
        ID = 0;
        RTCPID = 0;
        RTPC = new();
    }

    public void Read(BankReader reader)
    {
        ID = reader.ReadUInt32();
        RTPC.Read(reader);
        RTCPID = reader.ReadUInt32();
        RTCPType = (RTPCType)reader.ReadByte();

        int associatedChildrenCount = reader.ReadInt32();
        AssociatedChildren = new AssociatedChildData[associatedChildrenCount];
        for (int i = 0; i < associatedChildrenCount; i++)
        {
            AssociatedChildren[i] = new();
            AssociatedChildren[i].Read(reader);
        }
    }
}