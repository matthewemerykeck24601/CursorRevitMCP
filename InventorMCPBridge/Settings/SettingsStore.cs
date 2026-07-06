using System.Text.Json;
using System.IO;

namespace InventorMCPBridge.Settings;

public sealed class SettingsStore
{
    private readonly string _settingsFilePath;

    public SettingsStore(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
    }

    public BridgeSettings Current { get; private set; } = new();

    public void Load()
    {
        if (!File.Exists(_settingsFilePath))
        {
            Save(Current);
            return;
        }

        try
        {
            var raw = File.ReadAllText(_settingsFilePath);
            var parsed = JsonSerializer.Deserialize<BridgeSettings>(raw);
            Current = parsed ?? new BridgeSettings();
        }
        catch
        {
            Current = new BridgeSettings();
        }
    }

    public void Save(BridgeSettings settings)
    {
        var folder = Path.GetDirectoryName(_settingsFilePath);
        if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var json = JsonSerializer.Serialize(
            settings,
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsFilePath, json);
        Current = settings;
    }
}
