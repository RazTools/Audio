namespace Audio.Extensions;
public static class StreamExtensions
{
    public static int CopyTo(this Stream source, Stream destination, long size)
    {
        Span<byte> buffer = stackalloc byte[0x400];

        int writtenCount = 0;
        do
        {
            int toRead = Math.Min((int)size - writtenCount, buffer.Length);
            int readCount = source.Read(buffer[..toRead]);
            if (readCount == 0) break;
            destination.Write(buffer[..readCount]);
            writtenCount += readCount;
        } while (writtenCount < size);

        return writtenCount;
    }
}
