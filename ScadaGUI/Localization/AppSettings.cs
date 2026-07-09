using System;
using System.ComponentModel;

namespace ScadaGUI.Localization
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static AppSettings instance;
        public static AppSettings Instance => instance ?? (instance = new AppSettings());

        public event PropertyChangedEventHandler PropertyChanged;

        private TimeZoneInfo timeZone = TimeZoneInfo.Local;
        private string dateFormat = "dd.MM.yyyy HH:mm:ss";

        public TimeZoneInfo TimeZone
        {
            get => timeZone;
            set { timeZone = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeZone))); }
        }

        public string DateFormat
        {
            get => dateFormat;
            set { dateFormat = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateFormat))); }
        }

        // Konvertuje UTC vreme u izabranu vremensku zonu i formatira ga izabranim formatom.
        public string FormatUtc(DateTime utcTime)
        {
            var converted = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
            return converted.ToString(dateFormat);
        }
    }
}
