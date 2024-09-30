namespace Audio.Chunks.Types.STMG;
public record AcousticTexture : IBankReadable
{
    public FNVID<uint> ID { get; set; }
    public float AbsorptionOffset { get; set; }
    public float AbsorptionLow { get; set; }
    public float AbsorptionMidLow { get; set; }
    public float AbsorptionMidHigh { get; set; }
    public float AbsorptionHigh { get; set; }
    public float Scattering { get; set; }

    public AcousticTexture()
    {
        ID = 0;
    }

    public void Read(BankReader reader)
    {
        ID = reader.ReadUInt32();
        AbsorptionOffset = reader.ReadSingle();
        AbsorptionLow = reader.ReadSingle();
        AbsorptionMidLow = reader.ReadSingle();
        AbsorptionMidHigh = reader.ReadSingle();
        AbsorptionHigh = reader.ReadSingle();
        Scattering = reader.ReadSingle();
    }
}
