using System.Windows.Media;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.View
{
    public static class FieldBrushProvider
    {
        private static readonly Dictionary<FieldType, Brush> _brushes = [];

        static FieldBrushProvider()
        {
            Map(FieldType.DEEP_LAVA_OCEAN, Color.FromRgb(80, 0, 0));
            Map(FieldType.DEEP_LAVA_OCEAN, Color.FromRgb(147, 0, 0));
            Map(FieldType.LAVA_OCEAN, Color.FromRgb(236, 62, 62));
            Map(FieldType.BEACH, Color.FromRgb(69, 40, 40));
            Map(FieldType.LOW_LANDS, Color.FromRgb(120, 99, 99));
            Map(FieldType.LOW_MID_TRANSITION, Color.FromRgb(120, 137, 115));
            Map(FieldType.MID_LANDS, Color.FromRgb(166, 160, 160));
            Map(FieldType.MID_HIGH_TRANSITION, Color.FromRgb(107, 97, 19));
            Map(FieldType.HIGH_LANDS, Color.FromRgb(71, 73, 14));
            Map(FieldType.MOUNTAINS, Color.FromRgb(32, 47, 40));
            Map(FieldType.HIGH_MOUNTAINS, Color.FromRgb(255, 255, 255));
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
