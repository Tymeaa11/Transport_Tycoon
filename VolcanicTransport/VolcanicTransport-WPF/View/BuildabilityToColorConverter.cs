using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.View
{
    public class BuildabilityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isBuildable = (bool)value;

            return isBuildable ? new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)) 
                : new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
            => throw new NotImplementedException();
    }
}
