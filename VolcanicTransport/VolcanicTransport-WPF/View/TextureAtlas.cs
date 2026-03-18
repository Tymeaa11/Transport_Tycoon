using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VolcanicTransport_WPF.View
{
    public static class TextureAtlas
    {
        public static ImageSource? MushroomTexture1 { get; private set; }
        public static ImageSource? MushroomTexture2 { get; private set; }
        public static ImageSource? MushroomTexture3 { get; private set; }
        public static ImageSource? MushroomTexture4 { get; private set; }

        public static ImageSource? CityBuildingTexture { get; private set; }
        public static ImageSource? FactoryBuildingTexture { get; private set; }

        public static ImageSource? RoadStraightTexture { get; private set; }
        public static ImageSource? RoadCurvedTexture { get; private set; }
        public static ImageSource? RoadJunctionTexture { get; private set; }
        public static ImageSource? RoadTJunctionTexture { get; private set; }
        public static ImageSource? RoadEndTexture { get; private set; }
        public static ImageSource? RoadInvalidTexture { get; private set; }

        private static bool _isLoaded = false;

        public static void Initialize(string atlasPath)
        {
            if (_isLoaded) return;

            BitmapImage atlas = new(new Uri(atlasPath, UriKind.RelativeOrAbsolute));

            MushroomTexture1 = GetTile(atlas, 0, 0);
            MushroomTexture2 = GetTile(atlas, 0, 1);
            MushroomTexture3 = GetTile(atlas, 0, 2);
            MushroomTexture4 = GetTile(atlas, 0, 3);

            CityBuildingTexture = GetTile(atlas, 1, 0);
            FactoryBuildingTexture = GetTile(atlas, 1, 1);

            RoadStraightTexture = GetTile(atlas, 1, 2);
            RoadCurvedTexture = GetTile(atlas, 1, 3);
            RoadJunctionTexture = GetTile(atlas, 2, 0);
            RoadTJunctionTexture = GetTile(atlas, 2, 1);
            RoadEndTexture = GetTile(atlas, 2, 2);
            RoadInvalidTexture = GetTile(atlas, 2, 3);

            _isLoaded = true;
        }

        private static ImageSource GetTile(BitmapSource atlas, int row, int col)
            => new CroppedBitmap(atlas, new Int32Rect(col * 64, row * 64, 64, 64));
    }
}
