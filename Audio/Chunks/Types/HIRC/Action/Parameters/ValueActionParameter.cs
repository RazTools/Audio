namespace Audio.Chunks.Types.HIRC;
public record ValueActionParameter : IActionParameter
{
    public CurveInterpolation FadeCurve { get; set; }

    public ValueActionParameter() { }

    public void Read(BankReader reader)
    {
        FadeCurve = (CurveInterpolation)reader.ReadByte() & CurveInterpolation.Mask;
        ReadParameters(reader);
    }

    public virtual void ReadParameters(BankReader reader) { }
}
