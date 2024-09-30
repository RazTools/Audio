namespace Audio.Chunks.Types.HIRC;

public enum CurveInterpolation
{
    Log3,
    Sine,
    Log1,
    InvSCurve,
    Linear,
    SCurve,
    Exp1,
    SineRecip,
    Exp3,
    Constant,
    Mask = 0x1F
}