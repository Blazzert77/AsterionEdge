using Microsoft.Win32;
namespace Asterion.Companion;

public static class StartupManager
{
    const string KeyPath=@"Software\Microsoft\Windows\CurrentVersion\Run";
    const string ValueName="AsterionEdge";
    public static bool IsEnabled()
    {
        using var key=Registry.CurrentUser.OpenSubKey(KeyPath,false);
        return !string.IsNullOrWhiteSpace(key?.GetValue(ValueName)?.ToString());
    }
    public static void Set(bool enabled)
    {
        using var key=Registry.CurrentUser.CreateSubKey(KeyPath);
        if(enabled)
        {
            string exe=Environment.ProcessPath ?? throw new InvalidOperationException("Executable path unavailable");
            key.SetValue(ValueName,$"\"{exe}\" --background",RegistryValueKind.String);
        }
        else key.DeleteValue(ValueName,false);
    }
}
