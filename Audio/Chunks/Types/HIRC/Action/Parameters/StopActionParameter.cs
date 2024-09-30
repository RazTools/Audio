using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;
public record StopActionParameter : ValueExceptionActionParameter
{
    public bool ApplyToStateTransitions { get; set; }
    public bool ApplyToDynamicSequence { get; set; }

    public StopActionParameter() { }

    public override void ReadParameters(BankReader reader)
    {
        BitVector32 vector = new(reader.ReadByte());

        ApplyToStateTransitions = vector.Get(1);
        ApplyToDynamicSequence = vector.Get(2);
    }
}
