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
            var json = File.ReadAllText(ConfigPath);
            var clone = JsonSerializer.Deserialize(json, ConfigManagerContext.Default.ConfigManager);

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
            var str = JsonSerializer.Serialize(this, ConfigManagerContext.Default.ConfigManager);
            File.WriteAllText(ConfigPath, str);
        }
        catch (Exception) { }
    }
}