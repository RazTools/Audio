namespace Audio.Chunks.Types.HIRC;

public record MusicSwitch : HIRCObject
{
    public MusicParameter Parameters { get; set; }
    public MusicTransitionRule[] TransitionRules { get; set; } = [];
    public bool IsContinuePlayback { get; set; }
    public DecisionTree Tree { get; set; }

    public MusicSwitch(HeaderInfo header) : base(header)
    {
        Parameters = new();
        Tree = new();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        Parameters.Read(reader);

        int transitionRuleCount = reader.ReadInt32();
        TransitionRules = new MusicTransitionRule[transitionRuleCount];
        for (int i = 0; i < transitionRuleCount; i++)
        {
            TransitionRules[i] = new();
            TransitionRules[i].Read(reader);
        }

        IsContinuePlayback = reader.ReadByte() != 0;
        Tree.Read(reader);
    }
}