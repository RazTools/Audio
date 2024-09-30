using Audio.Chunks.Types.HIRC;

namespace Audio.Chunks.Types.STMG;

public record SwitchGroupInfo : IBankReadable
{
    public FNVID<uint> SwitchGroupID { get; set; }
    public FNVID<uint> RTCPID { get; set; }
    public RTPCType RTCPType { get; set; }
    public uint CurveSize { get; set; }
    public RTPCGraphPoint[] GraphPoints { get; set; } = [];

    public SwitchGroupInfo()
    {
        SwitchGroupID = 0;
        RTCPID = 0;
    }

    public void Read(BankReader reader)
    {
        SwitchGroupID = reader.ReadUInt32();
        RTCPID = reader.ReadUInt32();
        RTCPType = (RTPCType)reader.ReadByte();
        CurveSize = reader.ReadUInt32();

        GraphPoints = new RTPCGraphPoint[CurveSize];
        for (int i = 0; i < CurveSize; i++)
        {
            GraphPoints[i] = new();
            GraphPoints[i].Read(reader);
        }
    }
}