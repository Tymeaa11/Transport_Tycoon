using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.View
{
    public static class TextureAtlas
    {
        public class TextureAtlasNotInitialisedException : Exception { }

        public static ImageSource[] MushroomTextures { get; private set; } = new ImageSource[5];

        private static ImageSource? cityBuildingTexture;
        public static ImageSource CityBuildingTexture
        {
            get => cityBuildingTexture ?? throw new TextureAtlasNotInitialisedException();
        }

        private static ImageSource? factoryBuildingTexture;
        public static ImageSource FactoryBuildingTexture
        {
            get => factoryBuildingTexture ?? throw new TextureAtlasNotInitialisedException();
        }

        private static ImageSource? stationTexture;
        public static ImageSource StationTexture
        {
            get => stationTexture ?? throw new TextureAtlasNotInitialisedException();
        }

        private static ImageSource? invalidTexture;
        public static ImageSource InvalidTexture
        {
            get => invalidTexture ?? throw new TextureAtlasNotInitialisedException();
        }

        public static Dictionary<RoadType, ImageWithRotation> RoadTextures { get; private set; } = [];

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

            cityBuildingTexture = GetTile(atlas, 1, 0);
            factoryBuildingTexture = GetTile(atlas, 1, 1);

            var straight = GetTile(atlas, 1, 2);
            var curved = GetTile(atlas, 1, 3);
            var xjunction = GetTile(atlas, 2, 0);
            var tjunction = GetTile(atlas, 2, 1);
            var end = GetTile(atlas, 2, 2);
            var lonely = GetTile(atlas, 3, 0);
            var invalid = GetTile(atlas, 2, 3);

            RoadTextures[RoadType.STRAIGHT_NS] = new ImageWithRotation(straight, 0);
            RoadTextures[RoadType.STRAIGHT_EW] = new ImageWithRotation(straight, 90);

            RoadTextures[RoadType.SLOPE_N] = new ImageWithRotation(end, 180);
            RoadTextures[RoadType.SLOPE_W] = new ImageWithRotation(end, 90);
            RoadTextures[RoadType.SLOPE_S] = new ImageWithRotation(end, 0);
            RoadTextures[RoadType.SLOPE_E] = new ImageWithRotation(end, 270);


            RoadTextures[RoadType.SLOPE_NS] = new ImageWithRotation(straight, 0);
            RoadTextures[RoadType.SLOPE_EW] = new ImageWithRotation(straight, 90);

            RoadTextures[RoadType.CURVED_ES] = new ImageWithRotation(curved, 0);
            RoadTextures[RoadType.CURVED_SW] = new ImageWithRotation(curved, 90);
            RoadTextures[RoadType.CURVED_WN] = new ImageWithRotation(curved, 180);
            RoadTextures[RoadType.CURVED_NE] = new ImageWithRotation(curved, 270);

            RoadTextures[RoadType.JUNCTION_T_ESW] = new ImageWithRotation(tjunction, 0);
            RoadTextures[RoadType.JUNCTION_T_SWN] = new ImageWithRotation(tjunction, 90);
            RoadTextures[RoadType.JUNCTION_T_WNE] = new ImageWithRotation(tjunction, 180);
            RoadTextures[RoadType.JUNCTION_T_NES] = new ImageWithRotation(tjunction, 270);

            RoadTextures[RoadType.JUNCTION_NESW] = new ImageWithRotation(xjunction, 0);

            RoadTextures[RoadType.SOUTH] = new ImageWithRotation(end, 0);
            RoadTextures[RoadType.WEST] = new ImageWithRotation(end, 90);
            RoadTextures[RoadType.NORTH] = new ImageWithRotation(end, 180);
            RoadTextures[RoadType.EAST] = new ImageWithRotation(end, 270);

            RoadTextures[RoadType.LONELY] = new ImageWithRotation(lonely, 0);

            RoadTextures[RoadType.INVALID] = new ImageWithRotation(invalid, 0);

            stationTexture = GetTile(atlas, 3, 1);
            invalidTexture = GetTile(atlas, 3, 3);

            _isLoaded = true;
        }

        private static ImageSource GetTile(BitmapSource atlas, int row, int col)
            => new CroppedBitmap(atlas, new Int32Rect(col * 64, row * 64, 64, 64));
    }
}
