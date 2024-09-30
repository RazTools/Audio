namespace Audio.Chunks.Types.HIRC;

public enum SyncType
{
    Immediate,
    NextGrid,
    NextBar,
    NextBeat,
    NextMarker,
    NextUserMarker,
    EntryMarker,
    ExitMarker,
    ExitNever,
    LastExitPosition
}