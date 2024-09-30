namespace Audio.Chunks.Types.HIRC;
public record Sound : HIRCObject
{
    public BaseParameter Parameters { get; set; }
    public BankSourceData SourceData { get; set; }

    public Sound(HeaderInfo header) : base(header)
    {
        Parameters = new();
        SourceData = new();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        SourceData.Read(reader);

        if (SourceData.Plugin.Type == PluginType.Source)
        {
            SourceData.Plugin.ReadParamaters(reader);
        }

        Parameters.Read(reader);
    }
}