using System.Text.Json.Serialization;

namespace Audio.Entries;

public record EventInfo(FNVID<uint> ID)
{
    public Dictionary<FNVID<uint>, HashSet<EventTag>> TagsByID { get; } = [];

    [JsonIgnore]
    public IEnumerable<FNVID<uint>> TargetIDs => TagsByID.Keys.AsEnumerable();
    [JsonIgnore]
    public Stack<EventTag> Tags { get; } = [];

    public void AddTarget(FNVID<uint> id)
    {
        if (!TagsByID.TryGetValue(id, out HashSet<EventTag>? tags))
        {
            TagsByID[id] = tags = [];
        }

        foreach (EventTag tag in Tags)
        {
            tags.Add(tag);
        }
    }
    public IEnumerable<EventTag> GetValues(FNVID<uint> id) => TagsByID.TryGetValue(id, out HashSet<EventTag>? tags) ? [ ..tags] : [];
    public bool HasValue(FNVID<uint> id, FNVID<uint> type, FNVID<uint> value) => TagsByID.ContainsKey(id) && TagsByID[id].Contains(new(type, value));
    public void SetValue(FNVID<uint> id, FNVID<uint> type, FNVID<uint> value)
    {
        if (!TagsByID.TryGetValue(id, out HashSet<EventTag>? tags))
        {
            TagsByID[id] = tags = [];
        }

        tags.Add(new(type, value));
    }
}

public record EventTag(FNVID<uint> Type, FNVID<uint> Value);