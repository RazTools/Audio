namespace Audio.Chunks.Types.HIRC;

public record TrackTransInfo : IBankReadable
{
    public MusicFade SourceFade { get; set; }
    public SyncType SyncType { get; set; }
    public uint CueFilterHash { get; set; }
    public MusicFade DestinationFade { get; set; }

    public TrackTransInfo()
    {
        SourceFade = new();
        DestinationFade = new();
    }

    public void Read(BankReader reader)
    {
        SourceFade.Read(reader);
        SyncType = (SyncType)reader.ReadUInt32();
        CueFilterHash = reader.ReadUInt32();
        DestinationFade.Read(reader);
    }
}