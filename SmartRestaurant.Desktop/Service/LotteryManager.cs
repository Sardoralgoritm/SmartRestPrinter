using System.IO;
using System.Text.Json;
namespace SmartRestaurant.Desktop.Service;

public static class LotteryManager
{
    private static readonly string _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
    public static bool IsLotteryModeEnabled { get; set; } = false;

    static LotteryManager()
    {
        LoadSettings();
    }

    public static void LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                SaveSettings();
                return;
            }

            var json = File.ReadAllText(_settingsFilePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("LotterySettings", out var lotterySettings) &&
                lotterySettings.TryGetProperty("IsEnabled", out var isEnabled))
            {
                IsLotteryModeEnabled = isEnabled.GetBoolean();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Settings yuklashda xatolik: {ex.Message}");
            SaveSettings();
        }
    }

    public static void SaveSettings()
    {
        try
        {
            Dictionary<string, object> jsonObj;

            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                jsonObj = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            else
            {
                jsonObj = new Dictionary<string, object>();
            }

            var lotterySettings = new Dictionary<string, object>
            {
                { "IsEnabled", IsLotteryModeEnabled }
            };

            jsonObj["LotterySettings"] = lotterySettings;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = JsonSerializer.Serialize(jsonObj, options);
            File.WriteAllText(_settingsFilePath, updatedJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Settings saqlashda xatolik: {ex.Message}");
        }
    }
}
