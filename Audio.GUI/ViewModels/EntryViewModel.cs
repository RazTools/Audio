using Audio.Entries;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audio.GUI.ViewModels;

public partial class EntryViewModel : ViewModelBase
{
    [ObservableProperty]
    private string infoText = "";

    [ObservableProperty]
    private Entry? entry;

    partial void OnEntryChanged(Entry? value)
    {
        if (value != null)
        {
            StringBuilder sb = new();
            sb.AppendLine($"Name: {value.Name}");
            sb.AppendLine($"Type: {value.Type}");
            sb.AppendLine($"Offset: {value.Offset}");
            sb.AppendLine($"Size: {value.Size}");
            sb.AppendLine($"Location: {value.Location}");
            sb.AppendLine($"Source: {value.Source}");
            if (value is TaggedEntry<uint> taggedEntry && taggedEntry.Events.Count > 0)
            {
                sb.AppendLine($"Events: ");
                foreach(KeyValuePair<FNVID<uint>, HashSet<EventTag>> evt in taggedEntry.Events)
                {
                    sb.AppendLine($"\t{evt.Key}:");
                    foreach(IGrouping<FNVID<uint>, EventTag> group in evt.Value.GroupBy(x => x.Type))
                    {
                        sb.AppendLine($"\t\t{group.Key}: [{string.Join(',', group.Select(x => x.Value))}]");
                    }
                }
            }

            InfoText = sb.ToString();
        }
    }
}