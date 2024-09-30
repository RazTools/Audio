namespace Audio.Chunks.Types.HIRC;
public record SetGameParameterActionParameter : ValueExceptionActionParameter
{
    public bool BypassTransition { get; set; }
    public ValueMeaning ValueMeaning { get; set; }
    public RandomizerModifier Modifier { get; set; }
    public SetGameParameterActionParameter()
    {
        Modifier = new();
    }

    public override void ReadParameters(BankReader reader)
    {
        BypassTransition = reader.ReadByte() != 0;
        ValueMeaning = (ValueMeaning)reader.ReadByte();
        Modifier.Read(reader);
    }
}