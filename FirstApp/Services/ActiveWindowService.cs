using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FirstApp.Services
{
    public static class ActiveWindowService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        /// <summary>
        /// Returns both process name and window title in one call
        /// </summary>
        public static (string AppName, string Title) GetActiveWindowInfo()
        {
            var hwnd = GetForegroundWindow();

            if (hwnd == IntPtr.Zero)
                return ("Unknown", "Unknown");

            string title = GetWindowTitle(hwnd);
            string appName = GetProcessName(hwnd);

            return (appName, title);
        }

        private static string GetWindowTitle(IntPtr hwnd)
        {
            int length = GetWindowTextLength(hwnd);
            if (length == 0) return "Unknown";

            var builder = new StringBuilder(length + 1);
            GetWindowText(hwnd, builder, builder.Capacity);

            return builder.ToString();
        }

        private static string GetProcessName(IntPtr hwnd)
        {
            try
            {
                GetWindowThreadProcessId(hwnd, out int processId);

                if (processId == 0)
                    return "Unknown";

                var process = Process.GetProcessById(processId);
                return process.ProcessName;
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}