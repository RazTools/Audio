using Audio.Extensions;
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Audio;
public class ConfigManager
{
    private readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    [JsonIgnore]
    public static ConfigManager Instance { get; private set; } = new ConfigManager();
    public string? VOPath { get; set; }
    public string? EventPath { get; set; }
    public bool Convert { get; set; }
    public bool Playlist { get; set; }

    public void Load()
    {
        try
        {
            string json = File.ReadAllText(ConfigPath);
            ConfigManager? clone = JsonExtensions.Deserialize<ConfigManager?>(json);

            VOPath = clone?.VOPath ?? "";
            EventPath = clone?.EventPath ?? "";
            Convert = clone?.Convert ?? false;
            Playlist = clone?.Playlist ?? false;
        }
        catch (Exception) { }
    }
    public void Save()
    {
        try
        {
            string str = this.Serialize();
            File.WriteAllText(ConfigPath, str);
        }
        catch (Exception) { }
    }
}