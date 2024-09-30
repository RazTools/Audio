using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record MusicParameter : IBankReadable
{
    public bool OverrideParentMidiTempo { get; set; }
    public bool OverrideParentMidiTarget { get; set; }
    public bool MidiTargetTypeBus { get; set; }
    public BaseParameter Parameters { get; set; }
    public FNVID<uint>[] ChildrenIDs { get; set; } = [];
    public MeterInfo MeterInfo { get; set; }
    public bool IsOverrideMeter { get; set; }
    public Stinger[] Stingers { get; set; } = [];

    public MusicParameter()
    {
        Parameters = new();
        MeterInfo = new();
    }

    public void Read(BankReader reader)
    {
        BitVector32 vector = new(reader.ReadByte());

        OverrideParentMidiTempo = vector.Get(1);
        OverrideParentMidiTarget = vector.Get(2);
        MidiTargetTypeBus = vector.Get(3);

        Parameters.Read(reader);

        int childrenCount = reader.ReadInt32();
        ChildrenIDs = new FNVID<uint>[childrenCount];
        for (int i = 0; i < childrenCount; i++)
        {
            ChildrenIDs[i] = reader.ReadUInt32();
        }

        MeterInfo.Read(reader);
        IsOverrideMeter = reader.ReadByte() != 0;

        int stingerCount = reader.ReadInt32();
        Stingers = new Stinger[stingerCount];
        for (int i = 0; i < stingerCount; i++)
        {
            Stingers[i] = new();
            Stingers[i].Read(reader);
        }
    }
}