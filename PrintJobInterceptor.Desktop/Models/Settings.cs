using System.IO;
using System.Text.Json;
using System.Windows;

namespace PrintJobInterceptor.Desktop.Models;

public class Settings
{
    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PrintJobInterceptor",
        "settings.json");

    public static JsonSerializerOptions DefaultOptions = new() 
    { 
        WriteIndented = true, 
        Converters = { new ThemeModeJsonConverter() }
    };
    public int PrintJobRelationTime { get; set; } = 10;

    public ThemeMode Theme { get; set; } = ThemeMode.System;
    
    public Settings()
    {
    }

    public void Save()
    {
        string? directory = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(this, DefaultOptions);

        try
        {
            File.WriteAllText(FilePath, json);
            ServiceLogger.LogInfo("Settings saved ");
        }
        catch (Exception e)
        {
            ServiceLogger.LogError(e, "Failed to save settings");
        }
    }
}