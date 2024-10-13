using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Audio.GUI.Models;
public partial class TreeNode : ObservableObject
{
    [ObservableProperty]
    private bool isChecked;

    [ObservableProperty]
    private bool isExpanded;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private ObservableCollection<TreeNode> nodes = [];

    public IEnumerable<TreeNode> Checked
    {
        get
        {
            if (IsChecked)
            {
                yield return this;
            }

            foreach (TreeNode node in Nodes)
            {
                foreach (TreeNode checkedNode in node.Checked)
                {
                    yield return checkedNode;
                }
            }
        }
    }

    partial void OnIsCheckedChanged(bool value)
    {
        foreach (TreeNode node in Nodes)
        {
            node.IsChecked = value;
        }
    }

    public virtual bool HasMatch(string? searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            return true;
        }

        Regex regex = new(searchText, RegexOptions.IgnoreCase);
        return regex.IsMatch(Name);
    }
}
