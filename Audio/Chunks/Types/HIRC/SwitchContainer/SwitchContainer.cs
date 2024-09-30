namespace Audio.Chunks.Types.HIRC;

public record SwitchContainer : HIRCObject
{
    public BaseParameter Parameters { get; set; }
    public GroupType GroupType { get; set; }
    public FNVID<uint> GroupID { get; set; }
    public FNVID<uint> DefaultSwitch { get; set; }
    public bool IsContinuousValidation { get; set; }
    public FNVID<uint>[] ChildrenIDs { get; set; } = [];
    public SwitchGroup[] SwitchGroups { get; set; } = [];
    public SwitchParameter[] SwitchParameters { get; set; } = [];

    public SwitchContainer(HeaderInfo header) : base(header)
    {
        GroupID = 0;
        DefaultSwitch = 0;
        Parameters = new();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        Parameters.Read(reader);
        GroupType = (GroupType)reader.ReadByte();
        GroupID = reader.ReadUInt32();
        DefaultSwitch = reader.ReadUInt32();
        IsContinuousValidation = reader.ReadByte() != 0;

        int childrenCount = reader.ReadInt32();
        ChildrenIDs = new FNVID<uint>[childrenCount];
        for (int i = 0; i < childrenCount; i++)
        {
            ChildrenIDs[i] = reader.ReadUInt32();
        }

        int switchGroupCount = reader.ReadInt32();
        SwitchGroups = new SwitchGroup[switchGroupCount];
        for (int i = 0; i < switchGroupCount; i++)
        {
            SwitchGroups[i] = new();
            SwitchGroups[i].Read(reader);
        }

        int switchParameterCount = reader.ReadInt32();
        SwitchParameters = new SwitchParameter[switchParameterCount];
        for (int i = 0; i < switchParameterCount; i++)
        {
            SwitchParameters[i] = new();
            SwitchParameters[i].Read(reader);
        }
    }
}