using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FirstApp.Services
{
    public class ActiveWindowService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        public static string? GetActiveWindowTitle(IntPtr hwnd)
        {
            int len = GetWindowTextLength(hwnd);
            if (len <= 0) return null;
            var sb = new StringBuilder(len + 1);
            GetWindowText(hwnd, sb, sb.Capacity);
            Debug.WriteLine("first call" + sb.ToString());
            return sb.ToString();
        }

        public static string? GetCurrentWindowTitle()
        {
            var hwnd = GetForegroundWindow();
            string? title = GetActiveWindowTitle(hwnd);
            Debug.WriteLine("second call: " + title);
            return title;
        }

        public static string? GetCurrentProcessName()
        {
            var hwnd = GetForegroundWindow();
            GetWindowThreadProcessId(hwnd, out int processId);
            var process = Process.GetProcessById(processId);
            var processName = process.ProcessName;
            Debug.WriteLine("process name: " + processName);
            return processName;
        }
    }
}
