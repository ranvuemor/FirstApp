using System.Runtime.InteropServices;

namespace FirstApp.Services
{
    public static class IdleDetectionService
    {
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static TimeSpan GetIdleTime()
        {
            LASTINPUTINFO info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);

            GetLastInputInfo(ref info);

            uint idleTicks = ((uint)Environment.TickCount - info.dwTime);

            return TimeSpan.FromMilliseconds(idleTicks);
        }

        public static bool IsIdle(int seconds)
        {
            return GetIdleTime().TotalSeconds >= seconds;
        }
    }
}