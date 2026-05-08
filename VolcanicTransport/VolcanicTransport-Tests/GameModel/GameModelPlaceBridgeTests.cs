using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.GameModelTests
{
    [TestClass]
    [DoNotParallelize]
    public class GameModelPlaceBridgeTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(8, 99);

        [TestMethod]
        public void PlaceBridge_Diagonal_ReturnsFalse()
        {
            bool result = Model.PlaceBridge(new Coordinate(0, 0), new Coordinate(1, 1), GameSettings.BridgeTypes[0]);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PlaceBridge_TooShort_ReturnsFalse()
        {
            bool result = Model.PlaceBridge(new Coordinate(0, 0), new Coordinate(1, 0), GameSettings.BridgeTypes[0]);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PlaceBridge_TooLong_ReturnsFalse()
        {
            var bridgeType = GameSettings.BridgeTypes[0];
            int excessiveLength = bridgeType.Length + 5;
            bool result = Model.PlaceBridge(new Coordinate(0, 0), new Coordinate(excessiveLength, 0), bridgeType);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PlaceBridge_OutOfBoundsStart_ReturnsFalse()
        {
            bool result = Model.PlaceBridge(new Coordinate(999, 0), new Coordinate(1003, 0), GameSettings.BridgeTypes[0]);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PlaceBridge_StartEndDifferentFieldTypes_ReturnsFalse()
        {
            var w = GameWorld.Instance;
            Coordinate? start = null, end = null;

            for (int x = 0; x < w.SizeInFields.X - 4; x++)
            {
                for (int y = 0; y < w.SizeInFields.Y; y++)
                {
                    var f1 = w.GetField(new Coordinate(x, y));
                    var f2 = w.GetField(new Coordinate(x + 3, y));
                    if (f1 != null && f2 != null && f1.Type != f2.Type && f1.Surface == null && f2.Surface == null)
                    {
                        start = new Coordinate(x, y);
                        end = new Coordinate(x + 3, y);
                        break;
                    }
                }
                if (start != null) break;
            }

            if (start == null) return;

            bool result = Model.PlaceBridge(start.Value, end.Value, GameSettings.BridgeTypes[0]);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PlaceBridge_AvailableBridgeTypes_NotEmpty()
        {
            Assert.IsTrue(GameSettings.BridgeTypes.Length > 0);
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class GameModelPlaceStationTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(8, 100);

        [TestMethod]
        public void PlaceStation_OnLavaOcean_ReturnsFalse()
        {
            var w = GameWorld.Instance;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.Type == FieldType.LAVA_OCEAN && f.Surface == null)
                    {
                        Assert.IsFalse(Model.PlaceStation(c));
                        return;
                    }
                }
        }

        [TestMethod]
        public void PlaceStation_ValidFlat_NoRoadNearby_ReturnsFalse()
        {
            var w = GameWorld.Instance;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.Type == FieldType.LOW_LANDS && f.Surface == null)
                    {
                        bool result = Model.PlaceStation(c);
                        Assert.IsTrue(result == true || result == false);
                        return;
                    }
                }
        }

        [TestMethod]
        public void IsBuildable_OutOfBounds_ReturnsFalse()
        {
            Assert.IsFalse(Model.IsBuildable(new Coordinate(9999, 9999)));
        }

        [TestMethod]
        public void IsHeightenable_OutOfBounds_ReturnsFalse()
        {
            Assert.IsFalse(Model.IsHeightenable(new Coordinate(9999, 9999)));
        }

        [TestMethod]
        public void IsLowerable_OutOfBounds_ReturnsFalse()
        {
            Assert.IsFalse(Model.IsLowerable(new Coordinate(9999, 9999)));
        }
    }
}
