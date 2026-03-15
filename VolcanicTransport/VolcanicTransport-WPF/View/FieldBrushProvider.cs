using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.View
{
    public static class FieldBrushProvider
    {
        private static readonly Dictionary<FieldType, Brush> _brushes = [];

        static FieldBrushProvider()
        {
            Map(FieldType.DEEP_LAVA_OCEAN,      Color.FromRgb(80, 0, 0));      // Dark Red
            Map(FieldType.LAVA_OCEAN,           Color.FromRgb(180, 20, 0));    // Bright Orange-Red
            Map(FieldType.BEACH,                Color.FromRgb(220, 150, 60));  // Scorched Sand
            Map(FieldType.LOW_LANDS,            Color.FromRgb(60, 100, 40));   // Dark Green
            Map(FieldType.LOW_MID_TRANSITION,   Color.FromRgb(90, 130, 50));   // Olive
            Map(FieldType.MID_LANDS,            Color.FromRgb(120, 160, 70));  // Light Green
            Map(FieldType.MID_HIGH_TRANSITION,  Color.FromRgb(140, 140, 120)); // Grey-Green
            Map(FieldType.HIGH_LANDS,           Color.FromRgb(100, 100, 100)); // Stone Grey
            Map(FieldType.MOUNTAINS,            Color.FromRgb(70, 70, 70));    // Dark Stone
            Map(FieldType.HIGH_MOUNTAINS,       Color.FromRgb(255, 255, 255)); // Ash/Snow Peak
        }

        private static void Map(FieldType type, Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            _brushes[type] = brush;
        }

        public static Brush GetBrush(FieldType type)
        {
            return _brushes.TryGetValue(type, out var brush) ? brush : Brushes.Magenta; // Magenta = Error
        }
    }
}
