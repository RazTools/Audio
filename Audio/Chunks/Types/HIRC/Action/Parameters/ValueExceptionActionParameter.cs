namespace Audio.Chunks.Types.HIRC;
public record ValueExceptionActionParameter : ExceptionActionParameter
{
    public CurveInterpolation FadeCurve { get; set; }

    public ValueExceptionActionParameter() { }

    public override void Read(BankReader reader)
    {
        FadeCurve = (CurveInterpolation)reader.ReadByte() & CurveInterpolation.Mask;
        ReadParameters(reader);
        base.Read(reader);
    }

    public virtual void ReadParameters(BankReader reader) { }
}
