using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record MusicTrack : HIRCObject
{
    public bool OverrideParentMidiTempo { get; set; }
    public bool OverrideParentMidiTarget { get; set; }
    public bool MidiTargetTypeBus { get; set; }
    public BankSourceData[] BankSources { get; set; } = [];
    public TrackSrcInfo[] Playlist { get; set; } = [];
    public uint SubTrackCount { get; set; }
    public ClipAutomation[] ClipAutomations { get; set; } = [];
    public BaseParameter Parameters { get; set; }
    public MusicTrackType TrackType { get; set; }
    public TrackSwitchInfo SwitchInfo { get; set; }
    public TrackTransInfo TransInfo { get; set; }
    public int LookAheadTime { get; set; }

    public MusicTrack(HeaderInfo header) : base(header)
    {
        Parameters = new();
        SwitchInfo = new();
        TransInfo = new();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        BitVector32 vector = new(reader.ReadByte());

        OverrideParentMidiTempo = vector.Get(1);
        OverrideParentMidiTarget = vector.Get(2);
        MidiTargetTypeBus = vector.Get(3);

        int sourceCount = reader.ReadInt32();
        BankSources = new BankSourceData[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            BankSources[i] = new();
            BankSources[i].Read(reader);
        }

        int playlistCount = reader.ReadInt32();
        if (playlistCount > 0)
        {
            Playlist = new TrackSrcInfo[playlistCount];
            for (int i = 0; i < playlistCount; i++)
            {
                Playlist[i] = new();
                Playlist[i].Read(reader);
            }

            SubTrackCount = reader.ReadUInt32();
        }

        int clipAutomationCount = reader.ReadInt32();
        ClipAutomations = new ClipAutomation[clipAutomationCount];
        for (int i = 0; i < clipAutomationCount; i++)
        {
            ClipAutomations[i] = new();
            ClipAutomations[i].Read(reader);
        }

        Parameters.Read(reader);
        TrackType = (MusicTrackType)reader.ReadByte();
        if (TrackType == MusicTrackType.Switch)
        {
            SwitchInfo.Read(reader);
            TransInfo.Read(reader);
        }

        LookAheadTime = reader.ReadInt32();
    }
}