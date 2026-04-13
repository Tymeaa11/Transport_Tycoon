using System.Windows.Data;
using VolcanicTransport.Model;

namespace VolcanicTransport_WPF.View
{
    public class ChunkToPixelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int coord = (int)value;
            return (double)(coord * GameSettings.ChunkSize * GameSettings.FieldSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}
