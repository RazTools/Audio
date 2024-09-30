namespace Audio.Chunks.Types.HIRC;
public record BypassEffectActionParameter : ExceptionActionParameter
{
    public bool IsBypass { get; set; }
    public byte TargetMask { get; set; }

    public BypassEffectActionParameter() { }

    public override void Read(BankReader reader)
    {
        IsBypass = reader.ReadByte() != 0;
        TargetMask = reader.ReadByte();

        base.Read(reader);
    }
}