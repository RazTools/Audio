using Audio.Conversion.Chunks;
using Audio.Conversion.Utils;
using System.Buffers.Binary;
using static Audio.Conversion.Utils.BitHelper;

namespace Audio.Conversion.Codecs;
public class Vorbis : WWiseRIFFFile
{
    private const string Codebook = "packed_codebooks_aoTuV_603.bin";

    public override string Extension => ".ogg";

    public Vorbis(WWiseRIFFHeader header) : base(header) { }

    public override bool TryWrite(Stream stream)
    {
        using OGGStream oggStream = new(stream);

        if (Header.GetChunk(out VORB? vorb) && Header.GetChunk(out DATA? data))
        {
            Header.Stream.Position = data.Header.Offset;

            if (vorb.SeekTableSize > vorb.VorbisDataOffset)
            {
                Logger.Warning("Seek table is corrupted !!");
                return false;
            }

            Span<byte> buffer = stackalloc byte[2]; 

            List<VorbisSeekEntry> seekTable = [];
            for (int i = 0; i < vorb.SeekTableSize / 4; i++)
            {
                Header.Stream.ReadExactly(buffer);
                uint frameOffset = BinaryPrimitives.ReadUInt16LittleEndian(buffer) + (seekTable.LastOrDefault()?.FrameOffset ?? 0);

                Header.Stream.ReadExactly(buffer);
                uint fileOffset = BinaryPrimitives.ReadUInt16LittleEndian(buffer) + (seekTable.LastOrDefault()?.FileOffset ?? 0);

                seekTable.Add(new(frameOffset, fileOffset));
            }

            if (TryGenerateHeader(oggStream, out bool[] blockFlags, out int modeBitCount))
            {
                Header.Stream.Position = data.Header.Offset + vorb.VorbisDataOffset;

                int prevBlockSize = 0;
                bool prevBlockFlag = false;
                bool needMod = blockFlags.Length > 0 && modeBitCount > 0;
                while (Header.Stream.Position < data.Header.Offset + data.Header.Length)
                {
                    int mode = 0;

                    VorbisPacket packet = new(Header.Stream, Header.Stream.Position);

                    if (Header.Stream.Position + packet.Size > data.Header.Offset + data.Header.Length)
                    {
                        Logger.Warning("Audio packet header truncated !!");
                        return false;
                    }

                    Header.Stream.Position = packet.Offset;

                    if (needMod)
                    {
                        oggStream.Write(new BitValue(1)); // packetType

                        using BitStream bitStream = new(Header.Stream, false, true);
                        bitStream.Position = packet.Offset * BitsInByte;

                        BitValue modeNumber = bitStream.Read(modeBitCount);
                        BitValue reminder = bitStream.Read(BitsInByte - modeBitCount);

                        oggStream.Write(modeNumber);

                        mode = Convert.ToInt32(blockFlags[modeNumber]);

                        if (blockFlags[modeNumber])
                        {
                            Header.Stream.Position = packet.Next;

                            bool nextBlockFlag = false;
                            if (packet.Next + 2 <= data.Header.Offset + data.Header.Length)
                            {
                                VorbisPacket nextPacket = new(Header.Stream, packet.Next);
                                if (nextPacket.Size > 0)
                                {
                                    Header.Stream.Position = nextPacket.Offset;

                                    using BitStream nextPacketBitstream = new(Header.Stream, false, true);
                                    nextPacketBitstream.Position = nextPacket.Offset * BitsInByte;

                                    BitValue nextModeNumber = nextPacketBitstream.Read(modeBitCount);
                                    nextBlockFlag = blockFlags[nextModeNumber];
                                }
                            }

                            oggStream.Write(new BitValue(1, Convert.ToUInt32(prevBlockFlag)));
                            oggStream.Write(new BitValue(1, Convert.ToUInt32(nextBlockFlag)));
                            Header.Stream.Position = packet.Offset + 1;
                        }

                        prevBlockFlag = blockFlags[modeNumber];

                        oggStream.Write(reminder);
                    }

                    for (int i = 0; i < packet.Size; i++)
                    {
                        if (i == 0 && needMod) continue;

                        int b = Header.Stream.ReadByte();
                        if (b < 0)
                        {
                            Logger.Warning("Audio packet truncated !!");
                            return false;
                        }

                        oggStream.Write(new BitValue(8, (uint)b));
                    }

                    int blockSize = 1 << vorb.BlockSizes[mode];
                    oggStream.Granule += prevBlockSize == 0 ? 0 : (prevBlockSize + blockSize) / 4;
                    prevBlockSize = blockSize;

                    Header.Stream.Position = packet.Next;

                    oggStream.Type &= ~OGGStream.PageType.Continued;
                    if (Header.Stream.Position == data.Header.Offset + data.Header.Length)
                    {
                        oggStream.Type |= OGGStream.PageType.Tail;
                    }
                    else
                    {
                        oggStream.Type &= ~OGGStream.PageType.Tail;
                    }

                    oggStream.FlushPacket(true);
                }

                if (Header.Stream.Position > data.Header.Offset + data.Header.Length)
                {
                    Logger.Warning("file truncated !!");
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    private bool TryGenerateHeader(OGGStream oggStream, out bool[] blockFlags, out int modeBitCount)
    {
        blockFlags = [];
        modeBitCount = 0;

        if (Header.GetChunk(out FMT? fmt) && Header.GetChunk(out VORB? vorb))
        {
            WriteHeader(oggStream, VorbisHeaderType.Info);
            oggStream.Write(new BitValue(32)); // version
            oggStream.Write(new BitValue(8, fmt.Channels));
            oggStream.Write(new BitValue(32, fmt.SampleRate));
            oggStream.Write(new BitValue(32)); // bitrate_max
            oggStream.Write(new BitValue(32, fmt.AverageBitrate * BitsInByte));
            oggStream.Write(new BitValue(32)); // bitrate_minimum
            oggStream.Write(new BitValue(4, vorb.BlockSizes[0]));
            oggStream.Write(new BitValue(4, vorb.BlockSizes[1]));
            oggStream.Write(new BitValue(1, 1)); // framing
            oggStream.FlushPage();

            WriteHeader(oggStream, VorbisHeaderType.Comment);
            WriteString(oggStream, "Converted from Audiokinetic Wwise by ww2ogg 0.24");
            if (false) // TODO: loops
            {
                oggStream.Write(new BitValue(32, 2));
                WriteString(oggStream, $"LoopStart={vorb.LoopInfo.LoopBeginExtra}");
                WriteString(oggStream, $"LoopEnd={vorb.LoopInfo.LoopEndExtra}");
            }
            else
            {
                oggStream.Write(new BitValue(32));
            }

            oggStream.Write(new BitValue(1, 1)); // framing
            oggStream.FlushPacket();

            if (Header.GetChunk(out DATA? data))
            {
                WriteHeader(oggStream, VorbisHeaderType.Books);
                VorbisPacket setupPacket = new(Header.Stream, data.Header.Offset + vorb.SeekTableSize);

                using BitStream bitStream = new(Header.Stream, leaveOpen: true);
                bitStream.Position = setupPacket.Offset * BitsInByte;

                BitValue prevCodebookCount = bitStream.Read(8);
                oggStream.Write(prevCodebookCount);
                uint codebookCount = (uint)prevCodebookCount + 1;

                if (WWiseCodebook.TryOpen(Codebook, out WWiseCodebook? codebook))
                {
                    try
                    {
                        for (int i = 0; i < codebookCount; i++)
                        {
                            BitValue codebookID = bitStream.Read(10);
                            codebook.RebuildPCB(codebookID, oggStream);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error while generating OGG header: {e}");
                        return false;
                    }
                }

                oggStream.Write(new BitValue(6)); // timeCountLess1
                oggStream.Write(new BitValue(16)); // ignoreTimeValue

                BitValue floorCountLess1 = bitStream.Read(6);
                oggStream.Write(floorCountLess1);

                uint floorCount = (uint)floorCountLess1 + 1;
                for (int i = 0; i < floorCount; i++)
                {
                    oggStream.Write(new BitValue(16, 1)); // floorType

                    BitValue floorPartitionsCount = bitStream.Read(5);
                    oggStream.Write(floorPartitionsCount);

                    uint[] floorPartitions = new uint[floorPartitionsCount];
                    for (int j = 0; j < floorPartitionsCount; j++)
                    {
                        BitValue floorPartition = bitStream.Read(4);
                        oggStream.Write(floorPartition);

                        floorPartitions[j] = floorPartition;
                    }

                    int maxPartition = (int)floorPartitions.DefaultIfEmpty().Max();
                    uint[] floorDimensions = new uint[maxPartition + 1];
                    for (int j = 0; j <= maxPartition; j++)
                    {
                        BitValue partitionDimensionLess1 = bitStream.Read(3);
                        oggStream.Write(partitionDimensionLess1);

                        floorDimensions[j] = (uint)partitionDimensionLess1 + 1;

                        BitValue subPartitions = bitStream.Read(2);
                        oggStream.Write(subPartitions);

                        if (subPartitions != 0)
                        {
                            BitValue masterBook = bitStream.Read(8);
                            oggStream.Write(masterBook);

                            if (masterBook >= codebookCount)
                            {
                                Logger.Warning($"Master book {masterBook} is out of range {codebookCount}");
                                return false;
                            }
                        }

                        for (int k = 0; k < 1 << subPartitions; k++)
                        {
                            BitValue nextSubPartitionBook = bitStream.Read(8);
                            oggStream.Write(nextSubPartitionBook);

                            int subPartitionBook = nextSubPartitionBook - 1;
                            if (subPartitions >= 0 && subPartitions >= codebookCount)
                            {
                                Logger.Warning($"Sub partition {subPartitions} is out of range {codebookCount}");
                                return false;
                            }
                        }
                    }

                    oggStream.Write(bitStream.Read(2)); // floorMultiplierLess1

                    BitValue rangeBitCount = bitStream.Read(4);
                    oggStream.Write(rangeBitCount);

                    for (int j = 0; j < floorPartitionsCount; j++)
                    {
                        uint currentPartition = floorPartitions[j];
                        for (int k = 0; k < floorDimensions[currentPartition]; k++)
                        {
                            oggStream.Write(bitStream.Read(rangeBitCount)); // X
                        }
                    }
                }

                BitValue residueCountLess1 = bitStream.Read(6);
                oggStream.Write(residueCountLess1);

                int residueCount = residueCountLess1 + 1;
                for (int i = 0; i < residueCount; i++)
                {
                    BitValue residueType = bitStream.Read(2);
                    oggStream.Write(new BitValue(16, residueType));

                    if (residueType > 2)
                    {
                        Logger.Warning($"Invalid residue type {residueType}");
                        return false;
                    }

                    oggStream.Write(bitStream.Read(24)); // residueBegin
                    oggStream.Write(bitStream.Read(24)); // residueEnd
                    oggStream.Write(bitStream.Read(24)); // residuePartitionSizeLess1

                    BitValue residueClassificationCountLess1 = bitStream.Read(6);
                    BitValue residueClassbook = bitStream.Read(8);

                    oggStream.Write(residueClassificationCountLess1);
                    oggStream.Write(residueClassbook);

                    if (residueClassbook >= codebookCount)
                    {
                        Logger.Warning($"Residue classbook {residueClassbook} is out of range {codebookCount}");
                        return false;
                    }

                    int residueClassificationCount = residueClassificationCountLess1 + 1;
                    uint[] residueCascade = new uint[residueClassificationCount];
                    for (int j = 0; j < residueClassificationCount; j++)
                    {
                        BitValue highBits = new(5);

                        BitValue lowBits = bitStream.Read(3);
                        oggStream.Write(lowBits);

                        BitValue bitFlag = bitStream.Read(1);
                        oggStream.Write(bitFlag);

                        if (bitFlag != 0)
                        {
                            highBits = bitStream.Read(5);
                            oggStream.Write(highBits);
                        }

                        residueCascade[j] = (uint)highBits * BitsInByte + lowBits;
                    }

                    for (int j = 0; j < residueClassificationCount; j++)
                    {
                        for (int k = 0; k < BitsInByte; k++)
                        {
                            if ((residueCascade[j] & 1 << k) != 0)
                            {
                                BitValue residueBook = bitStream.Read(8);
                                oggStream.Write(residueBook);

                                if (residueBook >= codebookCount)
                                {
                                    Logger.Warning($"Residue classbook {residueBook} is out of range {codebookCount}");
                                    return false;
                                }
                            }
                        }
                    }
                }

                BitValue mapCountLess1 = bitStream.Read(6);
                oggStream.Write(mapCountLess1);

                int mapCount = mapCountLess1 + 1;
                for (int i = 0; i < mapCount; i++)
                {
                    oggStream.Write(new BitValue(16)); // mapType

                    BitValue subMapFlag = bitStream.Read(1);
                    oggStream.Write(subMapFlag);

                    int subMapCount = 1;
                    if (subMapFlag != 0)
                    {
                        BitValue subMapCountLess1 = bitStream.Read(4);
                        oggStream.Write(subMapCountLess1);

                        subMapCount = subMapCountLess1 + 1;
                    }

                    BitValue squarePolarFlag = bitStream.Read(1);
                    oggStream.Write(squarePolarFlag);

                    if (squarePolarFlag != 0)
                    {
                        BitValue couplingStepsCountLess1 = bitStream.Read(8);
                        oggStream.Write(couplingStepsCountLess1);

                        int j = 0;
                        int couplingBitCount = ILog(fmt.Channels - 1u);
                        int couplingStepsCount = couplingStepsCountLess1 + 1;
                        while (j < couplingStepsCount)
                        {
                            BitValue magnitude = bitStream.Read(couplingBitCount);
                            BitValue angle = bitStream.Read(couplingBitCount);

                            oggStream.Write(magnitude);
                            oggStream.Write(angle);

                            if (angle == magnitude || magnitude >= fmt.Channels || angle >= fmt.Channels)
                            {
                                Logger.Warning($"Invalid coupling");
                                return false;
                            }

                            j++;
                        }
                    }

                    BitValue mapReserved = bitStream.Read(2);
                    oggStream.Write(mapReserved);

                    if (mapReserved != 0)
                    {
                        Logger.Warning($"Expected map reserved to be 0, got {mapReserved} !!");
                        return false;
                    }

                    if (subMapCount > 1)
                    {
                        for (int j = 0; j < fmt.Channels; j++)
                        {
                            BitValue mapMux = bitStream.Read(4);
                            oggStream.Write(mapMux);

                            if (mapMux >= subMapCount)
                            {
                                Logger.Warning($"Map mux {mapMux} is out of range {subMapCount}");
                                return false;
                            }
                        }
                    }

                    for (int j = 0; j < subMapCount; j++)
                    {
                        oggStream.Write(bitStream.Read(8)); // timeConfig

                        BitValue floorNumber = bitStream.Read(8);
                        oggStream.Write(floorNumber);

                        if (floorNumber >= floorCount)
                        {
                            Logger.Warning($"Floor number {floorNumber} is out of range {floorCount}");
                            return false;
                        }

                        BitValue residueNumber = bitStream.Read(8);
                        oggStream.Write(residueNumber);

                        if (residueNumber >= residueCount)
                        {
                            Logger.Warning($"Residue number {residueNumber} is out of range {residueCount}");
                            return false;
                        }
                    }
                }

                BitValue modeCountLess1 = bitStream.Read(6);
                oggStream.Write(modeCountLess1);

                modeBitCount = ILog(modeCountLess1);

                int modeCount = modeCountLess1 + 1;
                blockFlags = new bool[modeCount];
                for (int i = 0; i < modeCount; i++)
                {
                    BitValue blockFlag = bitStream.Read(1);
                    oggStream.Write(blockFlag);

                    blockFlags[i] = blockFlag != 0;

                    oggStream.Write(new BitValue(16)); // windowType
                    oggStream.Write(new BitValue(16)); // transformType

                    BitValue mapNumber = bitStream.Read(8);
                    oggStream.Write(mapNumber);

                    if (mapNumber >= mapCount)
                    {
                        Logger.Warning($"Map number {mapNumber} is out of range {mapCount}");
                        return false;
                    }
                }

                oggStream.Write(new BitValue(1, 1)); // framing
                oggStream.FlushPacket();
                oggStream.FlushPage();

                long align = BitsInByte - 1;
                long read = bitStream.Position - setupPacket.Offset * BitsInByte;

                read += align;
                read &= ~align;

                if (read != setupPacket.Size * BitsInByte)
                {
                    Logger.Warning($"Expected {setupPacket.Size * BitsInByte} bits, got {bitStream.Position}");
                    return false;
                }

                if (data.Header.Offset + vorb.VorbisDataOffset != setupPacket.Next)
                {
                    Logger.Warning("No audio packets found after setup packet !!");
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    private static void WriteHeader(OGGStream oggStream, VorbisHeaderType type)
    {
        oggStream.Write(new BitValue(8, (uint)type));

        foreach (byte c in "vorbis"u8)
        {
            oggStream.Write(new BitValue(8, c));
        }
    }

    private static void WriteString(OGGStream oggStream, ReadOnlySpan<char> value)
    {
        oggStream.Write(new BitValue(32, (uint)value.Length));

        foreach (char c in value)
        {
            oggStream.Write(new BitValue(8, c));
        }
    }
}
