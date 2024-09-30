using Audio.Entries;
using System.Runtime.InteropServices;

namespace Audio.Chunks.Types.HIRC;
public record DecisionTree : IBankReadable
{
    public int TreeDepth { get; set; }
    public int TreeDataSize { get; set; }
    public DecisionTreeMode Mode { get; set; }
    public List<GameSync> Arguments { get; set; } = [];
    public Memory<DecisionTreeNode> Tree { get; set; }

    public void Read(BankReader reader)
    {
        TreeDepth = reader.ReadInt32();
        for (int i = 0; i < TreeDepth; i++)
        {
            Arguments.Add(new());
        }

        for (int i = 0; i < TreeDepth; i++)
        {
            Arguments[i].Group = reader.ReadUInt32();
        }

        for (int i = 0; i < TreeDepth; i++)
        {
            Arguments[i].GroupType = (GroupType)reader.ReadByte();
        }

        TreeDataSize = reader.ReadInt32();
        Mode = (DecisionTreeMode)reader.ReadByte();
        Tree = new DecisionTreeNode[TreeDataSize / Marshal.SizeOf<DecisionTreeNode>()];
        reader.Read(MemoryMarshal.Cast<DecisionTreeNode, byte>(Tree.Span));
    }

    public IEnumerable<FNVID<uint>> Resolve(EventInfo eventInfo)
    {
        foreach(FNVID<uint> id in ResolveNode(Tree.Span[0], eventInfo))
        {
            yield return id;
        }
    }

    private IEnumerable<FNVID<uint>> ResolveNode(DecisionTreeNode root, EventInfo eventInfo, int depth = 0)
    {
        if (root.NodeIndex > 0 && root.NodeCount > 0 && root.NodeIndex + root.NodeCount < Tree.Length)
        {
            Memory<DecisionTreeNode> children = Tree.Slice(root.NodeIndex, root.NodeCount);
            for (int i = 0; i < children.Length; i++)
            {
                DecisionTreeNode child = children.Span[i];
                eventInfo.Tags.Push(new(Arguments[depth].Group, child.Key != 0 ? child.Key : new("Any")));

                if (child.NodeCount > Tree.Length)
                {
                    yield return child.NodeID;
                }
                else
                {
                    foreach(FNVID<uint> id in ResolveNode(child, eventInfo, depth + 1))
                    {
                        yield return id;
                    }
                }

                eventInfo.Tags.Pop();
            }
        }
    }
}
