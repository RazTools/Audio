using Audio.Conversion.Utils;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static Audio.Conversion.Utils.BitHelper;

namespace Audio.Conversion;
public class WWiseCodebook
{
    private const uint Signature = 0x564342;

    private readonly Memory<byte> _codebookData;
    private readonly Memory<int> _codebookOffsets;
    private readonly int _codebookSize;

    public WWiseCodebook(Stream stream)
    {
        stream.Seek(-4, SeekOrigin.End);

        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        _codebookSize = BinaryPrimitives.ReadInt32LittleEndian(buffer);

        _codebookData = new byte[_codebookSize];
        _codebookOffsets = new int[(int)(stream.Length - _codebookSize) / 4];

        stream.Position = 0;
        stream.ReadExactly(_codebookData.Span);
        stream.ReadExactly(MemoryMarshal.AsBytes(_codebookOffsets.Span));
    }

    public Span<byte> GetCodebook(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _codebookOffsets.Length);

        int offset = _codebookOffsets.Span[index];
        int size = _codebookData.Span.Length - offset;
        if (index < _codebookOffsets.Length - 1)
        {
            size = _codebookOffsets.Span[index + 1] - offset;
        }

        return _codebookData.Span.Slice(offset, size);
    }

    public void RebuildPCB(int index, OGGStream oggStream)
    {
        Span<byte> codebook = GetCodebook(index);
        using MemoryStream ms = new(codebook.ToArray());
        using BitStream pcbStream = new(ms);

        RebuildStream(pcbStream, oggStream);

        if (pcbStream.Position < pcbStream.Length - BitsInByte)
        {
            throw new InvalidOperationException("Stream not entirely consumed !!");
        }
    }

    public static void RebuildStream(BitStream bitStream, OGGStream oggStream)
    {
        BitValue dimensions = bitStream.Read(4);
        BitValue entries = bitStream.Read(14);

        oggStream.Write(new BitValue(24, Signature));
        oggStream.Write(new BitValue(16, dimensions));
        oggStream.Write(new BitValue(24, entries));

        BitValue ordered = bitStream.Read(1);
        oggStream.Write(ordered);

        if (ordered != 0)
        {
            oggStream.Write(bitStream.Read(5)); // InitialLength

            uint currentEntry = 0;
            while (currentEntry < entries)
            {
                BitValue count = bitStream.Read(ILog(entries - currentEntry));
                oggStream.Write(count);
                currentEntry += count;
            }

            if (currentEntry > entries)
            {
                throw new InvalidOperationException("Current entry is out of range.");
            }
        }
        else
        {
            BitValue codewordSizeBitcount = bitStream.Read(3);
            BitValue sparse = bitStream.Read(1);

            if (codewordSizeBitcount <= 0 || codewordSizeBitcount > 5)
            {
                throw new InvalidOperationException("Invalid Codeword sizes count.");
            }

            oggStream.Write(sparse);

            for (uint i = 0; i < entries; i++)
            {
                BitValue present = new(1, 1);

                if (sparse != 0)
                {
                    present = bitStream.Read(1);
                    oggStream.Write(present);
                }

                if (present != 0)
                {
                    BitValue codewordSize = bitStream.Read((int)(uint)codewordSizeBitcount);
                    oggStream.Write(new BitValue(5, codewordSize));
                }
            }
        }

        BitValue lookupType = bitStream.Read(1);
        oggStream.Write(new BitValue(4, lookupType));
        if (lookupType == 1)
        {
            BitValue min = bitStream.Read(32);
            BitValue max = bitStream.Read(32);
            BitValue bitCount = bitStream.Read(4);
            BitValue sequanceFlag = bitStream.Read(1);

            oggStream.Write(min);
            oggStream.Write(max);
            oggStream.Write(bitCount);
            oggStream.Write(sequanceFlag);

            uint qCount = QuantCount(entries, dimensions);
            for (uint i = 0; i < qCount; i++)
            {
                oggStream.Write(bitStream.Read(bitCount + 1));
            }
        }
        else if (lookupType != 0)
        {
            throw new InvalidOperationException($"Unknown lookup type {lookupType}");
        }
    }

    public static void Copy(BitStream bitStream, OGGStream oggStream)
    {
        BitValue signature = bitStream.Read(24);
        BitValue dimensions = bitStream.Read(16);
        BitValue entries = bitStream.Read(24);

        if (signature != Signature)
        {
            throw new InvalidDataException($"Expected {Signature}, got {signature} instead !!");
        }

        oggStream.Write(new BitValue(24, signature));
        oggStream.Write(new BitValue(16, dimensions));
        oggStream.Write(new BitValue(24, entries));

        BitValue ordered = bitStream.Read(1);
        oggStream.Write(ordered);

        if (ordered != 0)
        {
            oggStream.Write(bitStream.Read(5)); // InitialLength

            uint currentEntry = 0;
            while (currentEntry < entries)
            {
                BitValue count = bitStream.Read(ILog(entries - currentEntry));
                oggStream.Write(count);
                currentEntry += count;
            }

            if (currentEntry > entries)
            {
                throw new InvalidOperationException("Current entry is out of range.");
            }
        }
        else
        {
            BitValue sparse = bitStream.Read(1);
            oggStream.Write(sparse);

            for (uint i = 0; i < entries; i++)
            {
                BitValue present = new(1, 1);

                if (sparse != 0)
                {
                    present = bitStream.Read(1);
                    oggStream.Write(present);
                }

                if (present != 0)
                {
                    oggStream.Write(bitStream.Read(5)); // codewordLength
                }
            }
        }

        BitValue lookupType = bitStream.Read(4);
        oggStream.Write(lookupType);
        if (lookupType == 1)
        {
            BitValue min = bitStream.Read(32);
            BitValue max = bitStream.Read(32);
            BitValue bitCount = bitStream.Read(4);
            BitValue sequanceFlag = bitStream.Read(1);

            oggStream.Write(min);
            oggStream.Write(max);
            oggStream.Write(bitCount);
            oggStream.Write(sequanceFlag);

            uint qCount = QuantCount(entries, dimensions);
            for (uint i = 0; i < qCount; i++)
            {
                oggStream.Write(bitStream.Read(bitCount + 1));
            }
        }
        else if (lookupType != 0)
        {
            throw new InvalidOperationException($"Unknown lookup type {lookupType}");
        }
    }

    private static uint QuantCount(uint entries, uint dimensions)
    {
        int bits = ILog(entries);
        uint vals = entries >> (int)((bits - 1) * (dimensions - 1) / dimensions);

        uint acc, nextAcc;
        while (true)
        {
            acc = 1;
            nextAcc = 1;

            for (int i = 0; i < dimensions; i++)
            {
                acc *= vals;
                nextAcc *= vals + 1;
            }

            if (acc <= entries && nextAcc > entries)
            {
                return vals;
            }
            else
            {
                if (acc > entries)
                {
                    vals--;
                }
                else
                {
                    vals++;
                }
            }
        }
    }

    public static bool TryOpen(string path, [NotNullWhen(true)] out WWiseCodebook? codebook)
    {
        try
        {
            using Stream stream = File.OpenRead(path);
            codebook = new WWiseCodebook(stream);
            return true;
        }
        catch (Exception) { }

        codebook = null;
        return false;
    }
}