using System;
using System.Globalization;
using System.Windows.Data;
using DataConcentrator;
using ScadaGUI.Localization;

namespace ScadaGUI.Converters
{
    public class AlarmStateToLocalizedStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0) return string.Empty;
            if (values[0] is AlarmState state)
            {
                var key = $"AlarmState.{state}";
                return LocalizationManager.Instance[key];
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
