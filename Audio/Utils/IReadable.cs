namespace Audio;
public interface IReadable<T> where T : BinaryReader
{
    abstract void Read(T reader);
}
