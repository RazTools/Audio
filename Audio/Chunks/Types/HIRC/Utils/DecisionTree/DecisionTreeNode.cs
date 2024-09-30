using System.Runtime.InteropServices;

namespace Audio.Chunks.Types.HIRC;

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct DecisionTreeNode
{
    [FieldOffset(0)]
    public FNVID<uint> Key;
    [FieldOffset(4)]
    public FNVID<uint> NodeID;
    [FieldOffset(4)]
    public short NodeIndex;
    [FieldOffset(6)]
    public short NodeCount;
    [FieldOffset(8)]
    public ushort Weight;
    [FieldOffset(10)]
    public ushort Probability;
}