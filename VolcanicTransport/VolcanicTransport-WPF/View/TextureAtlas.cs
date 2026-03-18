using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.View
{
    public static class TextureAtlas
    {
        public static ImageSource[] MushroomTextures { get; private set; } = new ImageSource[5];

        public static ImageSource? CityBuildingTexture { get; private set; }
        public static ImageSource? FactoryBuildingTexture { get; private set; }

     
        public readonly struct ImageAndRotation
        {
            public ImageSource ImageSource { get; }
            public int AngleDegrees { get; }

            public ImageAndRotation(ImageSource src, int deg) : this()
            {
                ImageSource = src;
                AngleDegrees = deg;
            }
        }

        public static Dictionary<RoadType, ImageAndRotation> RoadTextures { get; private set; } = [];


        //public static Dictionary<RoadType, ImageSource> RoadTextures { get; private set; } = [];

        private static bool _isLoaded = false;

        public static void Initialize(string atlasPath)
        {
            if (_isLoaded) return;

            BitmapImage atlas = new(new Uri(atlasPath, UriKind.RelativeOrAbsolute));

            MushroomTextures = [
                GetTile(atlas, 0, 0),
                GetTile(atlas, 0, 1),
                GetTile(atlas, 0, 2),
                GetTile(atlas, 0, 3)
                ];

            CityBuildingTexture = GetTile(atlas, 1, 0);
            FactoryBuildingTexture = GetTile(atlas, 1, 1);

            var straight = GetTile(atlas, 1, 2);
            var curved = GetTile(atlas, 1, 3);
            var xjunction = GetTile(atlas, 2, 0);
            var tjunction = GetTile(atlas, 2, 1);
            var end = GetTile(atlas, 2, 2);
            var lonely = GetTile(atlas, 3, 0);
            var invalid = GetTile(atlas, 2, 3);

            RoadTextures[RoadType.STRAIGHT_NS] = new ImageAndRotation(straight, 0);
            RoadTextures[RoadType.STRAIGHT_EW] = new ImageAndRotation(straight, 90);

            RoadTextures[RoadType.SLOPE_NS] = new ImageAndRotation(straight, 0);
            RoadTextures[RoadType.SLOPE_EW] = new ImageAndRotation(straight, 90);

            RoadTextures[RoadType.CURVED_ES] = new ImageAndRotation(curved, 0);
            RoadTextures[RoadType.CURVED_SW] = new ImageAndRotation(curved, 90);
            RoadTextures[RoadType.CURVED_WN] = new ImageAndRotation(curved, 180);
            RoadTextures[RoadType.CURVED_NE] = new ImageAndRotation(curved, 270);

            RoadTextures[RoadType.JUNCTION_T_ESW] = new ImageAndRotation(tjunction, 0);
            RoadTextures[RoadType.JUNCTION_T_SWN] = new ImageAndRotation(tjunction, 90);
            RoadTextures[RoadType.JUNCTION_T_WNE] = new ImageAndRotation(tjunction, 180);
            RoadTextures[RoadType.JUNCTION_T_NES] = new ImageAndRotation(tjunction, 270);

            RoadTextures[RoadType.JUNCTION_NESW] = new ImageAndRotation(xjunction, 0);

            RoadTextures[RoadType.SOUTH] = new ImageAndRotation(end, 0);
            RoadTextures[RoadType.WEST] = new ImageAndRotation(end, 90);
            RoadTextures[RoadType.NORTH] = new ImageAndRotation(end, 180);
            RoadTextures[RoadType.EAST] = new ImageAndRotation(end, 270);

            RoadTextures[RoadType.LONELY] = new ImageAndRotation(lonely, 0);

            RoadTextures[RoadType.INVALID] = new ImageAndRotation(invalid, 0);

            _isLoaded = true;
        }

        private static ImageSource GetTile(BitmapSource atlas, int row, int col)
            => new CroppedBitmap(atlas, new Int32Rect(col * 64, row * 64, 64, 64));
    }
}
