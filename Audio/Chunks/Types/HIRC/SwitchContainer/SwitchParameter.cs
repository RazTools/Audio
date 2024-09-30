using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record SwitchParameter : IBankReadable
{
    public FNVID<uint> NodeID { get; set; }
    public bool IsFirstOnly { get; set; }
    public bool ContinuePlayback { get; set; }
    public SwitchMode OnSwitchMode { get; set; }
    public int FadeOutTime { get; set; }
    public int FadeInTime { get; set; }

    public SwitchParameter()
    {
        NodeID = 0;
    }

    public void Read(BankReader reader)
    {
        NodeID = reader.ReadUInt32();

        BitVector32 vector = new(reader.ReadByte());

        IsFirstOnly = vector.Get(0);
        ContinuePlayback = vector.Get(1);

        vector = new(reader.ReadByte());
        OnSwitchMode = vector.Mask<SwitchMode>();

        FadeOutTime = reader.ReadInt32();
        FadeInTime = reader.ReadInt32();
    }
}