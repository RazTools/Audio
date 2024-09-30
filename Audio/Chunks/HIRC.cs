using Audio.Chunks.Types.HIRC;
using Audio.Entries;
using Audio.Extensions;

namespace Audio.Chunks;
public record HIRC : Chunk
{
    public new const string Signature = "HIRC";

    private readonly Dictionary<FNVID<uint>, HIRCObject> _objectDict = [];

    public AudioManager? Manager { get; set; }
    public IEnumerable<HIRCObject> Objects => _objectDict.Values;

    public HIRC(HeaderInfo header) : base(header) { }

    public override void Read(BankReader reader)
    {
        int objectCount = reader.ReadInt32();
        for (int i = 0; i < objectCount; i++)
        {
            if (HIRCObject.TryParse(reader, out HIRCObject? hircObject))
            {
                hircObject.HIRC = this;
                _objectDict.Add(hircObject.ID, hircObject);
            }
        }
    }

    public void Dump(string outputPath)
    {
        if (_objectDict.Count > 0)
        {
            string? outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string json = _objectDict.Serialize();
            File.WriteAllText(outputPath, json);
        }
    }

    public void ResolveObject(FNVID<uint> id, EventInfo eventInfo)
    {
        if (_objectDict.TryGetValue(id, out HIRCObject? obj))
        {
            ResolveObject(obj, eventInfo);
        }
    }

    public void ResolveObject(HIRCObject obj, EventInfo eventInfo)
    {
        List<FNVID<uint>> targets = [];

        switch (obj)
        {
            case Event evt:
                targets.AddRange(evt.ActionsIDs);
                break;
            case ActorMixer actorMixer:
                targets.AddRange(actorMixer.ChildrenIDs);
                break;
            case LayerContainer layerContainer:
                targets.AddRange(layerContainer.ChildrenIDs);
                break;
            case SequenceContainer seqContainer:
                targets.AddRange(seqContainer.ChildrenIDs);
                break;
            case SwitchContainer switchContainer:
                eventInfo.Tags.Push(new(switchContainer.GroupID, switchContainer.DefaultSwitch));
                foreach (SwitchGroup group in switchContainer.SwitchGroups)
                {
                    eventInfo.Tags.Push(new(switchContainer.GroupID, group.SwitchID));
                    foreach(FNVID<uint> node in group.Nodes)
                    {
                        ResolveObject(node, eventInfo);
                    }

                    eventInfo.Tags.Pop();
                }

                eventInfo.Tags.Pop();
                break;
            case MusicSequence musicSeq:
                if (musicSeq.PlaylistCount > 0)
                {
                    foreach(MusicRanSeqPlaylist playlist in musicSeq.Playlist[0].Children)
                    {
                        targets.Add(playlist.SegmentID);
                    }
                }

                break;
            case MusicSwitch musicSwitch:
                foreach (FNVID<uint> id in musicSwitch.Tree.Resolve(eventInfo))
                {
                    ResolveObject(id, eventInfo);
                }

                break;
            case DialogueEvent dialogueEvent:
                foreach (FNVID<uint> id in dialogueEvent.Tree.Resolve(eventInfo))
                {
                    ResolveObject(id, eventInfo);
                }

                break;
            case MusicSegment musicSegment:
                targets.AddRange(musicSegment.Parameters.ChildrenIDs);
                break;
            case MusicTrack musicTrack:
                foreach (BankSourceData bankSourceData in musicTrack.BankSources)
                {
                    if (bankSourceData.MediaInformation.IsLanguageSpecific && musicTrack.HIRC?.Parent is BKHD bkhd)
                    {
                        eventInfo.Tags.Push(new(new("Language"), new(bkhd.LangaugeID)));
                        eventInfo.AddTarget(bankSourceData.MediaInformation.ID);
                        eventInfo.Tags.Pop();
                    }
                    else
                    {
                        eventInfo.AddTarget(bankSourceData.MediaInformation.ID);
                    }
                }

                break;
            case Types.HIRC.Action action:
                if (action.Type == ActionType.SetState && action.Parameters is SetStateActionParameter setStateActionParameter)
                {
                    foreach (HIRC hirc in Manager?.Hierarchies ?? [])
                    {
                        foreach (MusicSwitch musicSwitch in hirc.Objects.OfType<MusicSwitch>())
                        {
                            if (musicSwitch.Tree.Arguments.Any(x => x.Group == setStateActionParameter.StateGroupID))
                            {
                                hirc.ResolveObject(musicSwitch, eventInfo);
                            }
                        }
                    }

                    foreach (HIRC hirc in Manager?.Hierarchies ?? [])
                    {
                        foreach (SwitchContainer switchContainer in hirc.Objects.OfType<SwitchContainer>())
                        {
                            if (switchContainer.GroupID == setStateActionParameter.StateGroupID)
                            {
                                hirc.ResolveObject(switchContainer, eventInfo);
                            }
                        }
                    }
                }
                else
                {
                    Manager?.ResolveObject(action.GameObject, eventInfo);
                }

                break;
            case Types.HIRC.Sound sound:
                {
                    if (sound.SourceData.MediaInformation.IsLanguageSpecific && sound.HIRC?.Parent is BKHD bkhd)
                    {
                        eventInfo.Tags.Push(new(new("Language"), new(bkhd.LangaugeID)));
                        eventInfo.AddTarget(sound.SourceData.MediaInformation.ID);
                        eventInfo.Tags.Pop();
                    }
                    else
                    {
                        eventInfo.AddTarget(sound.SourceData.MediaInformation.ID);
                    }
                }

                break;
        }

        foreach (FNVID<uint> id in targets)
        {
            ResolveObject(id, eventInfo);
        }
    }
}