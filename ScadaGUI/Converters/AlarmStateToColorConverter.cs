using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataConcentrator;

namespace ScadaGUI.Converters
{
    public class AlarmStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlarmState state)
            {
                switch (state)
                {
                    case AlarmState.Active:
                        return new SolidColorBrush(Colors.Red);
                    case AlarmState.Acknowledged:
                        return new SolidColorBrush(Colors.Gold);
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
