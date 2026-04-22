using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.View
{
    public static class TextureAtlas
    {
        public class TextureAtlasNotInitialisedException : Exception { }

        #region Builsings
        private static ImageSource? cityBuildingTexture;
        public static ImageSource CityBuildingTexture
            => cityBuildingTexture ?? throw new TextureAtlasNotInitialisedException();

        public static ImageSource[,] FactoryBuildingTextures { get; private set; } = new ImageSource[2, 2];

        private static ImageSource? stationTexture;
        public static ImageSource StationTexture
            => stationTexture ?? throw new TextureAtlasNotInitialisedException();

        private static ImageSource? invalidTexture;
        public static ImageSource InvalidTexture
            => invalidTexture ?? throw new TextureAtlasNotInitialisedException();
        #endregion

        #region Road & Mushroom
        public static Dictionary<RoadType, ImageWithRotation> RoadTextures { get; private set; } = [];
        public static ImageSource[] MushroomTextures { get; private set; } = new ImageSource[5];
        #endregion

        #region Bridges
        public static Dictionary<RoadType, ImageWithRotation> BoneBridgeTextures { get; private set; } = [];

        public static Dictionary<RoadType, ImageWithRotation> StoneBridgeTextures { get; private set; } = [];

        public static Dictionary<RoadType, ImageWithRotation> SteelBridgeTextures { get; private set; } = [];
        #endregion

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

            cityBuildingTexture = GetTile(atlas, 0, 4);
            stationTexture = GetTile(atlas, 0, 6);

            BitmapImage factAtlas = new(new Uri("Assets/fact.png", UriKind.RelativeOrAbsolute));
            FactoryBuildingTextures[0, 0] = GetTile(factAtlas, 0, 0);
            FactoryBuildingTextures[1, 0] = GetTile(factAtlas, 0, 1);
            FactoryBuildingTextures[0, 1] = GetTile(factAtlas, 1, 0);
            FactoryBuildingTextures[1, 1] = GetTile(factAtlas, 1, 1);

            var straight =  GetTile(atlas, 1, 0);
            var curved =    GetTile(atlas, 1, 1);
            var xjunction = GetTile(atlas, 1, 2);
            var tjunction = GetTile(atlas, 1, 3);
            var end =       GetTile(atlas, 1, 4);
            var lonely =    GetTile(atlas, 1, 5);
            var invalid =   GetTile(atlas, 2, 6);

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

            invalidTexture = GetTile(atlas, 7, 7);

            var bone = GetTile(atlas, 2, 0);
            var stone = GetTile(atlas, 2, 1);
            var steel = GetTile(atlas, 2, 2);

            BoneBridgeTextures[RoadType.STRAIGHT_NS] = new ImageWithRotation(bone, 0);
            BoneBridgeTextures[RoadType.STRAIGHT_EW] = new ImageWithRotation(bone, 90);

            StoneBridgeTextures[RoadType.STRAIGHT_NS] = new ImageWithRotation(stone, 0);
            StoneBridgeTextures[RoadType.STRAIGHT_EW] = new ImageWithRotation(stone, 90);

            SteelBridgeTextures[RoadType.STRAIGHT_NS] = new ImageWithRotation(steel, 0);
            SteelBridgeTextures[RoadType.STRAIGHT_EW] = new ImageWithRotation(steel, 90);

            _isLoaded = true;
        }

        private const int TextureSize = 32;
        private static ImageSource GetTile(BitmapSource atlas, int row, int col)
            => new CroppedBitmap(
                atlas,
                new Int32Rect(col * TextureSize, row * TextureSize, TextureSize, TextureSize)
            );
    }
}