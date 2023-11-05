using System.IO;

namespace Audio.Models.Utils;
public interface IParse
{
    public abstract void Parse(BinaryReader reader);
}
