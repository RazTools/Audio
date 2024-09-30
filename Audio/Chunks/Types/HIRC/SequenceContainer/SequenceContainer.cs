using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record SequenceContainer : HIRCObject
{
    public BaseParameter Parameters { get; set; }
    public ushort LoopCount { get; set; }
    public ushort LoopModMin { get; set; }
    public ushort LoopModMax { get; set; }
    public float TransitionTime { get; set; }
    public float TransitionTimeModMin { get; set; }
    public float TransitionTimeModMax { get; set; }
    public ushort AvoidRepeatCount { get; set; }
    public TransitionMode TransitionMode { get; set; }
    public RandomMode RandomMode { get; set; }
    public ContainerMode ContainerMode { get; set; }
    public bool IsUsingWeight { get; set; }
    public bool ResetPlayListAtEachPlay { get; set; }
    public bool IsRestartBackward { get; set; }
    public bool IsContinuous { get; set; }
    public bool IsGlobal { get; set; }
    public FNVID<uint>[] ChildrenIDs { get; set; } = [];
    public Playlist[] Playlists { get; set; } = [];

    public SequenceContainer(HeaderInfo header) : base(header)
    {
        Parameters = new();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        Parameters.Read(reader);

        LoopCount = reader.ReadUInt16();
        LoopModMin = reader.ReadUInt16();
        LoopModMax = reader.ReadUInt16();

        TransitionTime = reader.ReadSingle();
        TransitionTimeModMin = reader.ReadSingle();
        TransitionTimeModMax = reader.ReadSingle();
        AvoidRepeatCount = reader.ReadUInt16();

        TransitionMode = (TransitionMode)reader.ReadByte();
        RandomMode = (RandomMode)reader.ReadByte();
        ContainerMode = (ContainerMode)reader.ReadByte();

        BitVector32 vector = new(reader.ReadByte());

        IsUsingWeight = vector.Get(0);
        ResetPlayListAtEachPlay = vector.Get(1);
        IsRestartBackward = vector.Get(2);
        IsContinuous = vector.Get(3);
        IsGlobal = vector.Get(4);

        int childrenCount = reader.ReadInt32();
        ChildrenIDs = new FNVID<uint>[childrenCount];
        for (int i = 0; i < childrenCount; i++)
        {
            ChildrenIDs[i] = reader.ReadUInt32();
        }

        int playlistCount = reader.ReadInt16();
        Playlists = new Playlist[playlistCount];
        for (int i = 0; i < playlistCount; i++)
        {
            Playlists[i] = new();
            Playlists[i].Read(reader);
        }
    }
}