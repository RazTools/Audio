namespace Audio.Chunks.Types.HIRC;

public record MusicSequence : HIRCObject
{
    public MusicParameter Parameters { get; set; }
    public MusicTransitionRule[] TransitionRules { get; set; } = [];
    public int PlaylistCount { get; set; }
    public MusicRanSeqPlaylist[] Playlist { get; set; } = [];

    public MusicSequence(HeaderInfo header) : base(header)
    {
        Parameters = new();
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

        PlaylistCount = reader.ReadInt32();
        Playlist = new MusicRanSeqPlaylist[1];
        for (int i = 0; i < Playlist.Length; i++)
        {
            Playlist[i] = new();
            Playlist[i].Read(reader);
        }
    }
}