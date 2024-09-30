namespace Audio.Chunks.Types.HIRC;

public record StateChunk : IBankReadable
{
    public StateChunkProperty[] Properties { get; set; } = [];
    public StateGroupChunk[] StateGroups { get; set; } = [];

    public void Read(BankReader reader)
    {
        int stateChunkCount = reader.Read7BitEncodedInt();
        Properties = new StateChunkProperty[stateChunkCount];
        for (int i = 0; i < stateChunkCount; i++)
        {
            Properties[i] = new();
            Properties[i].Read(reader);
        }

        int stateGroupCount = reader.Read7BitEncodedInt();
        StateGroups = new StateGroupChunk[stateGroupCount];
        for (int i = 0; i < stateGroupCount; i++)
        {
            StateGroups[i] = new();
            StateGroups[i].Read(reader);
        }
    }
}