using Audio.Chunks;
using System.Text;

namespace Audio;

public class BankReader : BinaryReader
{
    private uint _version;

    public BKHD? Root { get; set; }
    public uint Version
    {
        get
        {
            if (_version == 0 && Root is BKHD bkhd)
            {
                _version = bkhd.Version;
            }

            return _version;
        }
    }

    public BankReader(BinaryReader reader, bool leaveOpen = true) : this(reader.BaseStream, leaveOpen) { }
    public BankReader(Stream stream, bool leaveOpen = true) : base(stream, Encoding.UTF8, leaveOpen) { }
}