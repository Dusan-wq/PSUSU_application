using System;
using System.IO;

namespace DataConcentrator
{
    // Belezi svaku akciju korisnika (login, acknowledge alarma, dodavanje/update taga,
    // import/export, exception/error stanje...) u system.log fajl sa vremenskim trenutkom.
    public static class Logger
    {
        private static readonly object locker = new object();
        private static readonly string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.log");

        public static void Log(string action, string details = "")
        {
            try
            {
                lock (locker)
                {
                    string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}{(string.IsNullOrEmpty(details) ? "" : " - " + details)}";
                    File.AppendAllText(logPath, line + Environment.NewLine);
                }
            }
            catch
            {
                // logovanje ne sme da obori aplikaciju ako fajl nije dostupan
            }
        }

        public static void LogError(string context, Exception ex)
        {
            Log("ERROR", $"{context}: {ex.Message}");
        }
    }
}
