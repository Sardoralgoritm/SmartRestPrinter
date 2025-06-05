using System.IO;
using System.Management;

namespace SmartRestaurant.Desktop.Helpers.Security;

public static class LicenseService
{
    private static string AppDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SystemCache");

    private static string LicenseFilePath => Path.Combine(AppDir, "libhelper.dll");

    public static string GetCpuId()
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
    }

    public static bool IsLicenseValid()
    {
        if (!File.Exists(LicenseFilePath))
            return false;

        try
        {
            var savedCpuId = File.ReadAllText(LicenseFilePath).Trim();
            var currentCpuId = GetCpuId();

            return savedCpuId == currentCpuId;
        }
        catch
        {
            return false;
        }
    }
}

