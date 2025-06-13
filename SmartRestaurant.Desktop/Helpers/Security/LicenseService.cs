using System.IO;
using System.Management;

namespace SmartRestaurant.Desktop.Helpers.Security;

public static class LicenseService
{
    private static string AppDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SystemCache");

    private static string LicenseFilePath => Path.Combine(AppDir, "libhelper.dll");

    public static async Task<string> GetCpuIdAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("select ProcessorId from Win32_Processor");
                foreach (var obj in searcher.Get())
                {
                    return obj["ProcessorId"]?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        });
    }

    public static async Task<bool> IsLicenseValidAsync()
    {
        if (!File.Exists(LicenseFilePath))
            return false;
        try
        {
            var savedCpuId = await File.ReadAllTextAsync(LicenseFilePath);
            var currentCpuId = await GetCpuIdAsync();
            return savedCpuId.Trim() == currentCpuId;
        }
        catch
        {
            return false;
        }
    }
}

