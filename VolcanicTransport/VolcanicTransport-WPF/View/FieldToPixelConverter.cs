using System.Windows.Data;
using VolcanicTransport.Model;

namespace VolcanicTransport_WPF.View
{
    public class FieldToPixelConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int coord)
                return (double)(coord * GameSettings.FieldSize);
            return 0.0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}
