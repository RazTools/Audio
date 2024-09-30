namespace Audio.Chunks.Types.HIRC;
public record ExceptionActionParameter : IActionParameter
{
    public ActionException[] Exceptions { get; set; } = [];

    public virtual void Read(BankReader reader)
    {
        int exceptionCount = reader.Read7BitEncodedInt();
        Exceptions = new ActionException[exceptionCount];
        for (int i = 0; i < exceptionCount; i++)
        {
            Exceptions[i] = new();
            Exceptions[i].Read(reader);
        }
    }
}
