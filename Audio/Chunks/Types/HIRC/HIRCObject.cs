using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Audio.Chunks.Types.HIRC;

public abstract record HIRCObject : IBankReadable
{
    private readonly static Dictionary<HIRCType, Func<HeaderInfo, HIRCObject>> s_objects = new()
    {
        { HIRCType.State, header => new State(header) },
        { HIRCType.Sound, header => new Sound(header) },
        { HIRCType.Action, header => new Action(header) },
        { HIRCType.Event, header => new Event(header) },
        { HIRCType.SequenceContainer, header => new SequenceContainer(header) },
        { HIRCType.SwitchContainer, header => new SwitchContainer(header) },
        { HIRCType.ActorMixer, header => new ActorMixer(header) },
        { HIRCType.AudioBus, header => new AudioBus(header) },
        { HIRCType.LayerContainer, header => new LayerContainer(header) },
        { HIRCType.MusicSegment, header => new MusicSegment(header) },
        { HIRCType.MusicTrack, header => new MusicTrack(header) },
        { HIRCType.MusicSwitch, header => new MusicSwitch(header) },
        { HIRCType.MusicSequence, header => new MusicSequence(header) },
        { HIRCType.Attenuation, header => new Attenuation(header) },
        { HIRCType.DialogueEvent, header => new DialogueEvent(header) },
        // { HIRCType.FxShareSet, x => new HIRCObject(x) },
        // { HIRCType.FxCustom, x => new HIRCObject(x) },
        // { HIRCType.AuxiliaryBus, x => new HIRCObject(x) },
        // { HIRCType.LFO, x => new HIRCObject(x) },
        // { HIRCType.Envelope, x => new HIRCObject(x) },
        // { HIRCType.AudioDevice, x => new HIRCObject(x) },
        // { HIRCType.TimeMod, x => new HIRCObject(x) },
    };

    public HeaderInfo Header { get; private set; }
    public Chunks.HIRC? HIRC { get; set; }
    public FNVID<uint> ID { get; set; }

    public HIRCObject(HeaderInfo header)
    {
        ID = 0;
        Header = header;
    }

    public virtual void Read(BankReader reader)
    {
        ID = reader.ReadUInt32();
    }

    private void Align(BankReader reader)
    {
        if (s_objects.ContainsKey(Header.Type))
        {
            Debug.Assert(Header.Length - (reader.BaseStream.Position - Header.Offset) == 0);
        }

        Header.Align(reader);
    }

    public static bool TryParse(BankReader reader, [MaybeNullWhen(false)] out HIRCObject hircObject)
    {
        HeaderInfo header = new();
        header.Read(reader);

        if (s_objects.TryGetValue(header.Type, out Func<HeaderInfo, HIRCObject>? hircObjectAction))
        {
            hircObject = hircObjectAction(header);
            hircObject.Read(reader);
            hircObject.Align(reader);
            return true;
        }

        header.Align(reader);
        hircObject = null;
        return false;
    }

    public record HeaderInfo : IBankReadable
    {
        public HIRCType Type { get; set; }
        public long Offset { get; set; }
        public uint Length { get; set; }

        public void Read(BankReader reader)
        {
            Type = (HIRCType)reader.ReadByte();
            Length = reader.ReadUInt32();
            Offset = reader.BaseStream.Position;
        }

        public void Align(BankReader reader)
        {
            reader.BaseStream.Position += Length - (reader.BaseStream.Position - Offset);
        }
    }
}