using Audio.Models.Entries;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Audio.Models.Utils;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ConfigManager))]
public partial class ConfigManagerContext : JsonSerializerContext { }

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(IEnumerable<Entry>))]
public partial class EntryContext : JsonSerializerContext { }