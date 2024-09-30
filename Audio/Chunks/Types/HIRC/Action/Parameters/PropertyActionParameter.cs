namespace Audio.Chunks.Types.HIRC;
public record PropertyActionParameter : ValueExceptionActionParameter
{
    public ValueMeaning ValueMeaning { get; set; }
    public RandomizerModifier Modifier { get; set; }

    public PropertyActionParameter()
    {
        Modifier = new();
    }

    public override void ReadParameters(BankReader reader)
    {
        ValueMeaning = (ValueMeaning)reader.ReadByte();
        Modifier.Read(reader);
    }
}
