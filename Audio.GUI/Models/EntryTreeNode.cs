using Audio.Entries;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Audio.GUI.Models;
public partial class EntryTreeNode : TreeNode
{
    [ObservableProperty]
    private Entry entry;

    public EntryTreeNode(Entry entry)
    {
        this.entry = entry;
    }

    public override bool HasMatch(string? searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            return true;
        }

        Regex regex = new(searchText, RegexOptions.IgnoreCase);

        bool match = base.HasMatch(searchText);
        if (Entry is TaggedEntry<uint> uintTag)
        {
            match |= regex.IsMatch(uintTag.ID.ToString());
            if (uintTag.Events.Count > 0)
            {
                foreach (KeyValuePair<FNVID<uint>, HashSet<EventTag>> evt in uintTag.Events)
                {
                    foreach (EventTag tag in evt.Value)
                    {
                        match |= regex.IsMatch(tag.Type.ToString());
                        match |= regex.IsMatch(tag.Value.ToString());
                    }
                }
            }
        }
        else if (Entry is TaggedEntry<ulong> ulongTag)
        {
            match |= regex.IsMatch(ulongTag.ID.ToString());
        }

        match |= regex.IsMatch(Entry.Type.ToString());
        match |= regex.IsMatch(Entry.Location ?? "");
        match |= regex.IsMatch(Entry.Source);
        match |= regex.IsMatch(Entry.Offset.ToString());
        match |= regex.IsMatch(Entry.Size.ToString());

        return match;
    }
}
