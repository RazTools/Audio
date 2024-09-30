namespace Audio.Chunks.Types.HIRC;

public enum BelowThresholdBehavior
{
    ContinueToPlay,
    KillVoice,
    SetAsVirtualVoice,
    KillIfOneShotElseVirtual
}