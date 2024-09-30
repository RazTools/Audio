using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record Bus : IBankReadable
{
    public ActionProperty[] Properties { get; set; } = [];
    public Position Position { get; set; }
    public Auxiliary Auxiliary { get; set; }
    public bool KillNewest { get; set; }
    public bool UseVirtualBehavior { get; set; }
    public bool IsMaxNumInstIgnoreParent { get; set; }
    public bool IsBackgroundMusic { get; set; }
    public ushort MaxNumInstance { get; set; }
    public ChannelConfig ChannelConfig { get; set; }
    public bool IsHdrBus { get; set; }
    public bool HdrReleaseModeExponential { get; set; }

    public Bus()
    {
        Position = new();
        Auxiliary = new();
        ChannelConfig = new();
    }

    public void Read(BankReader reader)
    {
        int propertyCount = reader.ReadByte();
        Properties = new ActionProperty[propertyCount];
        for (int i = 0; i < propertyCount; i++)
        {
            Properties[i] = new();
        }

        for (int i = 0; i < propertyCount; i++)
        {
            Properties[i].ID = (PropertyID)reader.ReadByte();
        }

        for (int i = 0; i < propertyCount; i++)
        {
            Properties[i].Value = reader.ReadUInt32();
        }

        Position.Read(reader);
        Auxiliary.Read(reader);

        BitVector32 vector = new(reader.ReadByte());

        KillNewest = vector.Get(0);
        UseVirtualBehavior = vector.Get(1);
        IsMaxNumInstIgnoreParent = vector.Get(3);
        IsBackgroundMusic = vector.Get(4);

        MaxNumInstance = reader.ReadUInt16();
        ChannelConfig.Read(reader);

        vector = new(reader.ReadByte());

        IsHdrBus = vector.Get(0);
        HdrReleaseModeExponential = vector.Get(1);
    }
}