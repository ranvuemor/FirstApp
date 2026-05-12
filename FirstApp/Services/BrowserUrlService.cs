using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FirstApp.Services;

public static class BrowserUrlService
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    public static string? GetActiveBrowserUrl(string appName)
    {
        if (!IsSupportedBrowser(appName))
            return null;

        try
        {
            var hwnd = GetForegroundWindow();

            if (hwnd == IntPtr.Zero)
                return null;

            using var automation = new UIA3Automation();

            var window = automation.FromHandle(hwnd);

            if (window == null)
                return null;

            var edits = window.FindAllDescendants(cf =>
                cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit));

            foreach (var edit in edits)
            {
                string name = edit.Name?.ToLower() ?? "";

                if (
                    name.Contains("address") ||
                    name.Contains("search") ||
                    name.Contains("omnibox")
                )
                {
                    var textBox = edit.AsTextBox();
                    return textBox.Text;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"URL detection failed: {ex.Message}");
        }

        return null;
    }

    private static bool IsSupportedBrowser(string appName)
    {
        appName = appName.ToLower();

        return appName is "msedge" or "chrome" or "firefox";
    }
}