namespace Audio.Chunks.Types.HIRC;
public record Plugin : IBankReadable
{
    public PluginID ID { get; set; }
    public PluginCompany Company { get; set; }
    public PluginType Type { get; set; }
    public uint Size { get; set; }

    public void Read(BankReader reader)
    {
        uint value = reader.ReadUInt32();
        Type = (PluginType)value & PluginType.Mask;
        Company = (PluginCompany)(value >> 4) & PluginCompany.Mask;
        ID = (PluginID)(value >> 16);
    }

    public void ReadParamaters(BankReader reader)
    {
        Size = reader.ReadUInt32();
        if (Size == 0) return;
        throw new Exception();
    }
}
