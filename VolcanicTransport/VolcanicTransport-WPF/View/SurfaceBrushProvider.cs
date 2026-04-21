using System.Windows.Media;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport_WPF.View
{
    public static class SurfaceBrushProvider
    {
        private static readonly Dictionary<Type, Brush> _brushes = [];

        static SurfaceBrushProvider()
        {
            Map(typeof(Mushroom), Color.FromRgb(52, 133, 157));
            Map(typeof(Road), Color.FromRgb(64, 64, 64));
            Map(typeof(FactoryStation), Color.FromRgb(247, 255, 43));
            Map(typeof(CityStation), Color.FromRgb(247, 255, 43));
            Map(typeof(FactoryBuilding), Color.FromRgb(255, 121, 43));
            Map(typeof(CityBuilding), Color.FromRgb(255, 12, 0));
        }

        private static void Map(Type surface, Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            _brushes[surface] = brush;
        }

        public static Brush GetBrush(Type surface)
        {
            return _brushes.TryGetValue(surface, out var brush) ? brush : Brushes.Magenta; // Magenta = Error
        }
    }
}
