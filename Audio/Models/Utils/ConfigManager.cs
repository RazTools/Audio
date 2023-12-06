using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Audio.Models.Utils;
public class ConfigManager
{
    private readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    [JsonIgnore]
    public static ConfigManager Instance { get; private set; } = new ConfigManager();
    public string VOPath { get; set; }
    public string EventPath { get; set; }
    public string WWiserPath { get; set; }
    public string VGMStreamPath { get; set; }
    public void Load()
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            var json = File.ReadAllText(ConfigPath);
            var clone = JsonSerializer.Deserialize<ConfigManager>(json, options);

            VOPath = clone.VOPath;
            EventPath = clone.EventPath;
            WWiserPath = clone.WWiserPath;
            VGMStreamPath = clone.VGMStreamPath;
        }
        catch (Exception) { }
    }
    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            var str = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigPath, str);
        }
        catch (Exception) { }
    }
}
