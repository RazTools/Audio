namespace Audio.Chunks.Types.HIRC;
public record SeekActionParameter : ExceptionActionParameter
{
    public bool IsSeekRelativeToDuration { get; set; }
    public bool SnapToNearestMarker { get; set; }
    public RandomizerModifier Modifier { get; set; }

    public SeekActionParameter()
    {
        Modifier = new();
    }

    public override void Read(BankReader reader)
    {
        IsSeekRelativeToDuration = reader.ReadByte() != 0;
        Modifier.Read(reader);
        SnapToNearestMarker = reader.ReadByte() != 0;

        base.Read(reader);
    }
}