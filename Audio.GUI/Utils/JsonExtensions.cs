using System.Text.Json.Serialization;

namespace Audio.GUI.Extensions;
public static class JsonExtensions
{
    static JsonExtensions()
    {
        Audio.Extensions.JsonExtensions.RegisterTypeInfo(ConfigManagerContext.Default);
    }
}

[JsonSerializable(typeof(ConfigManager))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class ConfigManagerContext : JsonSerializerContext { }
