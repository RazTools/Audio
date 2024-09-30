using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record Auxiliary : IBankReadable
{
    public bool OverrideUserAuxSends { get; set; }
    public bool HasAux { get; set; }
    public bool OverrideReflectionsAuxBus { get; set; }
    public FNVID<uint>[] AuxSendIDs { get; set; }
    public FNVID<uint> ReflectionsAuxBus { get; set; }

    public Auxiliary()
    {
        ReflectionsAuxBus = 0;
        AuxSendIDs = new FNVID<uint>[4];
    }

    public void Read(BankReader reader)
    {
        BitVector32 vector = new(reader.ReadByte());

        OverrideUserAuxSends = vector.Get(2);
        HasAux = vector.Get(3);
        OverrideReflectionsAuxBus = vector.Get(4);

        if (HasAux)
        {
            for (int i = 0; i < AuxSendIDs.Length; i++)
            {
                AuxSendIDs[i] = reader.ReadUInt32();
            }
        }

        if (reader.Version >= 135)
        {
            ReflectionsAuxBus = reader.ReadUInt32();
        }
    }
}