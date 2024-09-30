using Audio.Chunks.Types.STMG;

namespace Audio.Chunks;
public record STMG : Chunk
{
    public new const string Signature = "STMG";

    public float VolumeThreshold { get; set; }
    public ushort MaxNumVoicesLimitInternal { get; set; }
    public ushort MaxNumDangerousVirtVoicesLimitInternal { get; set; }
    public StateGroup[] StateGroups { get; set; } = [];
    public SwitchGroupInfo[] SwitchGroups { get; set; } = [];
    public RTPCRamping[] Rampings { get; set; } = [];
    public AcousticTexture[] AcousticTextures { get; set; } = [];

    public STMG(HeaderInfo header) : base(header) { }

    public override void Read(BankReader reader)
    {
        VolumeThreshold = reader.ReadSingle();
        MaxNumVoicesLimitInternal = reader.ReadUInt16();
        MaxNumDangerousVirtVoicesLimitInternal = reader.ReadUInt16();

        int stateGroupCount = reader.ReadInt32();
        StateGroups = new StateGroup[stateGroupCount];
        for (int i = 0; i < stateGroupCount; i++)
        {
            StateGroups[i] = new();
            StateGroups[i].Read(reader);
        }

        int switchGroupCount = reader.ReadInt32();
        SwitchGroups = new SwitchGroupInfo[switchGroupCount];
        for (int i = 0; i < switchGroupCount; i++)
        {
            SwitchGroups[i] = new();
            SwitchGroups[i].Read(reader);
        }

        int rampingCount = reader.ReadInt32();
        Rampings = new RTPCRamping[rampingCount];
        for (int i = 0; i < rampingCount; i++)
        {
            Rampings[i] = new();
            Rampings[i].Read(reader);
        }

        int acousticTextureCount = reader.ReadInt32();
        AcousticTextures = new AcousticTexture[acousticTextureCount];
        for (int i = 0; i < acousticTextureCount; i++)
        {
            AcousticTextures[i] = new();
            AcousticTextures[i].Read(reader);
        }
    }
}
