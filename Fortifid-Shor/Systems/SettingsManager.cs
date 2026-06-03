using System;
using System.IO;
using System.Text.Json;

namespace Fortifid.Systems;

/// <summary>
/// Зберігає ігрові налаштування у файл settings.json поруч з .exe
/// </summary>
public class SettingsManager
{
    private static readonly string SettingsPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    public int    ResolutionWidth  { get; set; } = 1280;
    public int    ResolutionHeight { get; set; } = 720;
    public bool   IsFullscreen     { get; set; } = false;
    public float  MasterVolume     { get; set; } = 1.0f;
    public float  MusicVolume      { get; set; } = 0.6f;
    public float  SfxVolume        { get; set; } = 0.8f;

    public static readonly (int W, int H)[] Resolutions =
    {
        (1280, 720),
        (1600, 900),
        (1920, 1080),
        (2560, 1440),
    };

    public static SettingsManager Load()
    {
        if (File.Exists(SettingsPath))
        {
            try
            {
                string json = File.ReadAllText(SettingsPath);
                var s = JsonSerializer.Deserialize<SettingsManager>(json);
                if (s != null) return s;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Settings] Помилка читання: {ex.Message}");
            }
        }
        return new SettingsManager();   // defaults
    }

    public void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Settings] Помилка збереження: {ex.Message}");
        }
    }
}
