using Audio.Chunks.Types.HIRC;
using Audio.Entries;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Audio.Extensions;
public static class JsonExtensions
{
    private static readonly JsonSerializerOptions _options = new();
    static JsonExtensions()
    {
        _options.WriteIndented = true;
        _options.Converters.Add(new JsonStringEnumConverter());
        _options.Converters.Add(new FNVIDJsonConverter());
        _options.TypeInfoResolverChain.Add(EntryContext.Default);
        _options.TypeInfoResolverChain.Add(FNVIDContext.Default);
        _options.TypeInfoResolverChain.Add(ConfigManagerContext.Default);
        _options.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            .WithAddedModifier(PolymorphicTypeInfo<HIRCObject>)
            .WithAddedModifier(PolymorphicTypeInfo<IActionParameter>);
    }

    public static string Serialize<T>(this T? value) => JsonSerializer.Serialize(value, _options);

    public static T? Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _options); 

    private static void PolymorphicTypeInfo<T>(JsonTypeInfo jsonTypeInfo)
    {
        if (typeof(T).IsAssignableFrom(jsonTypeInfo.Type) && !jsonTypeInfo.Type.IsSealed)
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };

            IEnumerable<Type> types = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && jsonTypeInfo.Type.IsAssignableFrom(t));

            foreach (JsonDerivedType t in types.Select(t => new JsonDerivedType(t, t.Name)))
                jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(t);
        }
    }
}

[JsonSerializable(typeof(ConfigManager))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class ConfigManagerContext : JsonSerializerContext { }

[JsonSerializable(typeof(IEnumerable<Entry>))]
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
public partial class EntryContext : JsonSerializerContext { }

[JsonSerializable(typeof(FNVID<uint>))]
[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(FNVIDJsonConverter)], GenerationMode = JsonSourceGenerationMode.Serialization)]
public partial class FNVIDContext : JsonSerializerContext { }

public class FNVIDJsonConverter : JsonConverter<FNVID<uint>>
{
    public override FNVID<uint> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, FNVID<uint> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(value.Value));
        writer.WriteNumberValue(value.Value);
        writer.WritePropertyName(nameof(value.String));
        writer.WriteStringValue(value.String);
        writer.WriteEndObject();
    }

    public override FNVID<uint> ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] FNVID<uint> value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.Value.ToString());
    }
}
