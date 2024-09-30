namespace Audio.Chunks.Types.STMG;
public record RTPCRamping : IBankReadable
{
    public uint RTPCID { get; set; }
    public float Value { get; set; }
    public TransitionRampingType RampType { get; set; }
    public float RampUp { get; set; }
    public float RampDown { get; set; }
    public BuiltInParamType BuiltInParam { get; set; }

    public void Read(BankReader reader)
    {
        RTPCID = reader.ReadUInt32();
        Value = reader.ReadSingle();
        RampType = (TransitionRampingType)reader.ReadUInt32();
        RampUp = reader.ReadSingle();
        RampDown = reader.ReadSingle();
        BuiltInParam = (BuiltInParamType)reader.ReadByte();
    }
}
