using Audio.Entries;
using Audio.GUI.Models;
using Audio.GUI.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Data;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Audio.GUI.ViewModels;

public partial class TreeViewModel : ViewModelBase
{
    private static readonly char[] _separators = [
        Path.DirectorySeparatorChar,
        Path.AltDirectorySeparatorChar
    ];

    private readonly AudioManager _audioManager;
    private readonly EntryViewModel _entryViewModel;

    private IPlatformServiceProvider? _platformServiceProvider;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private ObservableCollection<TreeNode> nodes;

    [ObservableProperty]
    private Entry? selectedEntry;

    private IPlatformServiceProvider PlatformServiceProvider
    {
        get
        {
            return _platformServiceProvider ??= Ioc.Default.GetRequiredService<IPlatformServiceProvider>();
        }
    }

    public HierarchicalTreeDataGridSource<TreeNode> Source { get; private set; }
    public IEnumerable<TreeNode> Checked
    {
        get
        {
            foreach(TreeNode node in Nodes)
            {
                foreach(TreeNode checkedNode in node.Checked)
                {
                    yield return checkedNode;
                }
            }
        }
    } 

    [DesignOnly(true)]
    public TreeViewModel() { }

    public TreeViewModel(AudioManager audioManager, EntryViewModel entryViewModel)
    {
        _audioManager = audioManager;
        _entryViewModel = entryViewModel;

        Nodes = [];
        Source = new HierarchicalTreeDataGridSource<TreeNode>(Nodes)
        {
            Columns =
            {
                new CheckBoxColumn<TreeNode>(null, x => x.IsChecked, (o, v) => o.IsChecked = v, new GridLength(0.1, GridUnitType.Star), new()
                {
                    CanUserResizeColumn = false,
                }),
                new HierarchicalExpanderColumn<TreeNode>(
                    new TextColumn<TreeNode, string>(null, x => x.Name, new GridLength(1, GridUnitType.Star), new() 
                    { 
                        IsTextSearchEnabled = true,
                        CanUserResizeColumn = false
                    }),
                    x => x.Nodes,
                    x => x.Nodes.Count > 0,
                    x => x.IsExpanded
                ),
            },
        };
    }
    [RelayCommand]
    public void Expand(TappedEventArgs e)
    {
        if (Source.RowSelection!.SelectedIndex > -1)
        {
            if (Source.RowSelection!.SelectedItem is EntryTreeNode entryTreeNode)
            {
                _entryViewModel.Entry = entryTreeNode.Entry;
            }

            if (Source.RowSelection!.SelectedItem?.IsExpanded == true)
            {
                Source.Collapse(Source.RowSelection!.SelectedIndex);
            }
            else
            {
                Source.Expand(Source.RowSelection!.SelectedIndex);
            }
        }
    }

    [RelayCommand]
    public void PreviewEntry(TappedEventArgs e)
    {
        if (e.Source is StyledElement styledElement && styledElement.DataContext is EntryTreeNode entryTreeNode)
        {
            SelectedEntry = entryTreeNode.Entry;
        } 
    }

    [RelayCommand]
    public void Refresh(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Update();
        }
    }

    [RelayCommand]
    public async Task Copy(KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.C)
        {
            if (PlatformServiceProvider.Clipboard != null && Source.RowSelection!.SelectedItem != null)
            {
                await PlatformServiceProvider.Clipboard.SetTextAsync(Source.RowSelection!.SelectedItem.Name);
            }
        }
    }

    public void Update()
    {
        Nodes.Clear();

        BuildTree(_audioManager.Entries);
    }
    private void BuildTree(IEnumerable<Entry> entries, TreeNode? parent = null, int index = 0)
    {
        foreach (IGrouping<string?, Entry> group in entries.Where(x => x.Location?.Split(_separators).Length > index).GroupBy(x => Path.ChangeExtension(x.Location?.Split(_separators)[index], null)))
        {
            if (!string.IsNullOrEmpty(group.Key))
            {
                TreeNode? node = null;

                if (!group.Any(x => Path.ChangeExtension(x.Name, null)?.EndsWith(group.Key) == true))
                {
                    node = new TreeNode() { Name = group.Key };
                }
                else
                {
                    node = new EntryTreeNode(group.First()) { Name = group.Key };
                    if (!node.HasMatch(SearchText))
                    {
                        node = null;
                    }
                }

                if (node != null)
                {
                    if (parent == null)
                    {
                        Nodes.Add(node);
                    }
                    else
                    {
                        parent.Nodes.Add(node);
                    }

                    BuildTree(group, node, index + 1);

                    for (int i = node.Nodes.Count - 1; i >= 0; i--)
                    {
                        if (node.Nodes[i] is TreeNode child && child is not EntryTreeNode && child.Nodes.Count == 0)
                        {
                            node.Nodes.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
    partial void OnSearchTextChanging(string? oldValue, string newValue)
    {
        if (!string.IsNullOrEmpty(newValue))
        {
            try
            {
                Regex.Match("", newValue, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                throw new DataValidationException("Not a valid Regex value");
            }
        }
    }
}