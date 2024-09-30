namespace Audio.Chunks.Types.HIRC;

public record MusicFade : IBankReadable
{
    public int TransitionTime { get; set; }
    public CurveInterpolation FadeCurve { get; set; }
    public int FadeOffset { get; set; }

    public void Read(BankReader reader)
    {
        TransitionTime = reader.ReadInt32();
        FadeCurve = (CurveInterpolation)reader.ReadUInt32();
        FadeOffset = reader.ReadInt32();
    }
}