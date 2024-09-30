using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;
public record PauseActionParameter : ValueExceptionActionParameter
{
    public bool IncludePendingResume { get; set; }
    public bool ApplyToStateTransitions { get; set; }
    public bool ApplyToDynamicSequence { get; set; }

    public PauseActionParameter() { }

    public override void ReadParameters(BankReader reader)
    {
        BitVector32 vector = new(reader.ReadByte());

        IncludePendingResume = vector.Get(0);
        ApplyToStateTransitions = vector.Get(1);
        ApplyToDynamicSequence = vector.Get(2);
    }
}