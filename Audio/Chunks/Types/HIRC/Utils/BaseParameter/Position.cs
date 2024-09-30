
using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record Position : IBankReadable
{
    public bool PositioningInfoOverrideParent { get; set; }
    public bool HasListenerRelativeRouting { get; set; }
    public SpeakerPanningType PannerType { get; set; }
    public Position3DType PositionType { get; set; }
    public SpatializationMode SpatializationMode { get; set; }
    public bool EnableAttenuation { get; set; }
    public bool HoldEmitterPosAndOrient { get; set; }
    public bool HoldListenerOrient { get; set; }
    public bool EnableDiffraction { get; set; }
    public bool IsNotLooping { get; set; }
    public PathMode PathMode { get; set; }
    public int TransitionTime { get; set; }
    public PathVertex[] Vertices { get; set; } = [];
    public PathList[] Playlist { get; set; } = [];
    public Automation[] Automations { get; set; } = [];

    public void Read(BankReader reader)
    {
        BitVector32 vector = new(reader.ReadByte());

        PositioningInfoOverrideParent = vector.Get(0);
        HasListenerRelativeRouting = vector.Get(1);
        PannerType = vector.Mask<SpeakerPanningType>(2);
        PositionType = vector.Mask<Position3DType>(5);

        if (PositioningInfoOverrideParent && HasListenerRelativeRouting)
        {
            vector = new(reader.ReadByte());

            SpatializationMode = vector.Mask<SpatializationMode>();
            EnableAttenuation = vector.Get(3);
            HoldEmitterPosAndOrient = vector.Get(4);
            HoldListenerOrient = vector.Get(5);
            EnableDiffraction = vector.Get(6);
            IsNotLooping = vector.Get(7);

            if (PositionType != Position3DType.Emitter)
            {
                PathMode = (PathMode)reader.ReadByte();
                TransitionTime = reader.ReadInt32();

                int vertexCount = reader.ReadInt32();
                Vertices = new PathVertex[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    Vertices[i] = new();
                    Vertices[i].Read(reader);
                }

                int playlistCount = reader.ReadInt32();
                Playlist = new PathList[playlistCount];
                for (int i = 0; i < playlistCount; i++)
                {
                    Playlist[i] = new();
                    Playlist[i].Read(reader);
                }

                Automations = new Automation[playlistCount];
                for (int i = 0; i < playlistCount; i++)
                {
                    Automations[i] = new();
                    Automations[i].Read(reader);
                }
            }
        }
    }
}