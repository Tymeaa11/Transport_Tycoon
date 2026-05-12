using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.GameModelTests
{
    [TestClass]
    [DoNotParallelize]
    public class GameModelCreateVehicleTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 33);

        [TestMethod]
        public void CreateAndStartVehicle_NullRoute_ReturnsFalse()
        {
            Assert.IsFalse(Model.CreateAndStartVehicle("Bus", "NullRoute", null!));
        }

        [TestMethod]
        public void CreateAndStartVehicle_EmptyRoute_ReturnsFalse()
        {
            Assert.IsFalse(Model.CreateAndStartVehicle("Bus", "EmptyRoute", new Route()));
        }

        [TestMethod]
        public void CreateAndStartVehicle_SingleStopRoute_ReturnsFalse()
        {
            var route = new Route();
            route.AddStop(new AdvStation(new Coordinate(5, 5)));
            Assert.IsFalse(Model.CreateAndStartVehicle("Bus", "OneStop", route));
        }

        [TestMethod]
        public void CreateAndStartVehicle_CargoTruck_TriesCorrectType()
        {
            var route = MakeTwoStopRoute(10, 11);
            Model.CreateAndStartVehicle("CargoTruck", "CT1", route);
            var added = GameWorld.Instance.GetLatestVehicle();
            if (added != null)
            {
                Assert.IsInstanceOfType<CargoTruck>(added);
                GameWorld.Instance.RemoveVehicle(added);
            }
        }

        [TestMethod]
        public void CreateAndStartVehicle_TankerTruck_TriesCorrectType()
        {
            var route = MakeTwoStopRoute(20, 21);
            Model.CreateAndStartVehicle("TankerTruck", "TT1", route);
            var added = GameWorld.Instance.GetLatestVehicle();
            if (added != null)
            {
                Assert.IsInstanceOfType<TankerTruck>(added);
                GameWorld.Instance.RemoveVehicle(added);
            }
        }

        [TestMethod]
        public void CreateAndStartVehicle_MiniBus_TriesCorrectType()
        {
            var route = MakeTwoStopRoute(30, 31);
            Model.CreateAndStartVehicle("MiniBus", "MB1", route);
            var added = GameWorld.Instance.GetLatestVehicle();
            if (added != null)
            {
                Assert.IsInstanceOfType<MiniBus>(added);
                GameWorld.Instance.RemoveVehicle(added);
            }
        }

        [TestMethod]
        public void CreateAndStartVehicle_UnknownType_DefaultsToBus()
        {
            var route = MakeTwoStopRoute(40, 41);
            Model.CreateAndStartVehicle("Unknown", "Bus1", route);
            var added = GameWorld.Instance.GetLatestVehicle();
            if (added != null)
            {
                Assert.IsInstanceOfType<Bus>(added);
                GameWorld.Instance.RemoveVehicle(added);
            }
        }

        private static Route MakeTwoStopRoute(int x1, int x2)
        {
            var s1 = new AdvStation(new Coordinate(x1, 50));
            var s2 = new AdvStation(new Coordinate(x2, 50));
            var route = new Route();
            route.AddStop(s1);
            route.AddStop(s2);
            return route;
        }
    }


    [TestClass]
    [DoNotParallelize]
    public class GameModelTerraformTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(8, 55);

        [TestMethod]
        public void HeightenField_ValidField_IncreasesFieldType()
        {
            var coord = FindHeightenable();
            if (coord == null) return;

            FieldType before = GameWorld.Instance.GetField(coord.Value)!.Type;
            Model.HeightenField(coord.Value);
            FieldType after = GameWorld.Instance.GetField(coord.Value)!.Type;

            Assert.AreEqual((int)before + 1, (int)after);
        }

        [TestMethod]
        public void LowerField_ValidField_DecreasesFieldType()
        {
            var coord = FindLowerable();
            if (coord == null) return;

            FieldType before = GameWorld.Instance.GetField(coord.Value)!.Type;
            Model.LowerField(coord.Value);
            FieldType after = GameWorld.Instance.GetField(coord.Value)!.Type;

            Assert.AreEqual((int)before - 1, (int)after);
        }

        [TestMethod]
        public void HeightenField_InsufficientFunds_DoesNotChangeType()
        {
            var coord = FindHeightenable();
            if (coord == null) return;

            Model.TryPurchase(Model.PlayerMoney);
            FieldType before = GameWorld.Instance.GetField(coord.Value)!.Type;

            Model.HeightenField(coord.Value);

            Assert.AreEqual(before, GameWorld.Instance.GetField(coord.Value)!.Type);
        }

        private static Coordinate? FindHeightenable()
        {
            var w = GameWorld.Instance;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.IsHeightenable() && f.Surface is null)
                        return c;
                }
            return null;
        }

        private static Coordinate? FindLowerable()
        {
            var w = GameWorld.Instance;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.IsLowerable() && f.Surface is null)
                        return c;
                }
            return null;
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class GameModelRoadSuccessTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(8, 44);

        private static Coordinate? FindEmptyFlat()
        {
            var w = GameWorld.Instance;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.Surface is null && f.Type == FieldType.LOW_LANDS)
                        return c;
                }
            return null;
        }

        [TestMethod]
        public void PlaceRoad_WithBudget_DoesNotThrow()
        {
            var coord = FindEmptyFlat();
            if (coord == null) return;
            Model.PlaceRoad(coord.Value);
        }

        [TestMethod]
        public void PlaceRoad_WithBudget_FiresRoadBoughtOnSuccess()
        {
            var w = GameWorld.Instance;
            for (int y = 1; y < w.SizeInFields.Y - 1; y++)
            {
                for (int x = 1; x < w.SizeInFields.X - 1; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f == null || f.Surface != null || f.Type != FieldType.LOW_LANDS) continue;

                    var east = new Coordinate(x + 1, y);
                    var fe = w.GetField(east);
                    if (fe == null || fe.Surface != null || fe.Type != FieldType.LOW_LANDS) continue;

                    bool fired = false;
                    void H(object? _, EventArgs __) => fired = true;
                    Model.RoadBought += H;
                    Model.PlaceRoad(c);
                    Model.RoadBought -= H;

                    if (fired) return;
                }
            }
        }

        [TestMethod]
        public void IsBuildable_ValidLowLands_ReturnsTrue()
        {
            var coord = FindEmptyFlat();
            if (coord == null) return;
            Assert.IsTrue(Model.IsBuildable(coord.Value));
        }

        [TestMethod]
        public void IsHeightenable_ValidField_ReturnsTrue()
        {
            var w = GameWorld.Instance;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.IsHeightenable() && f.Surface is null)
                    {
                        Assert.IsTrue(Model.IsHeightenable(c));
                        return;
                    }
                }
        }

        [TestMethod]
        public void IsLowerable_ValidField_ReturnsTrue()
        {
            var w = GameWorld.Instance;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.IsLowerable() && f.Surface is null)
                    {
                        Assert.IsTrue(Model.IsLowerable(c));
                        return;
                    }
                }
        }

        [TestMethod]
        public void OnPlacementFailed_FiredWhenRoadCannotBePlaced()
        {
            var w = GameWorld.Instance;
            Coordinate? lava = null;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.Type == FieldType.LAVA_OCEAN && f.Surface is null)
                    { lava = c; break; }
                    if (lava != null) break;
                }

            if (lava == null) return;

            bool fired = false;
            void H(object? _, EventArgs __) => fired = true;
            Model.RoadBought += H;
            Model.PlaceRoad(lava.Value);
            Model.RoadBought -= H;
            Assert.IsFalse(fired);
        }
    }
    [TestClass]
    [DoNotParallelize]
    public class GameModelMonthlyExpensesTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 19);

        [TestMethod]
        public void MonthlyExpenses_WithVehicles_DeductsMoney()
        {
            var v = new Bus("ME1");
            GameWorld.Instance.AddVehicle(v);
            double before = Model.PlayerMoney;

            Model.UnPause();
            Model.Update(601);

            GameWorld.Instance.RemoveVehicle(v);

            Assert.IsTrue(Model.PlayerMoney < before, "Monthly vehicle cost should reduce money.");
        }

        [TestMethod]
        public void MonthlyExpenses_NoVehicles_MoneyUnchanged()
        {
            double before = Model.PlayerMoney;

            Model.UnPause();
            Model.Update(601);

            Assert.AreEqual(before, Model.PlayerMoney, 0.001);
        }

        [TestMethod]
        public void GameOver_FiredWhenCannotPayExpenses()
        {
            double drain = Model.PlayerMoney - 499;
            if (drain > 0) Model.TryPurchase(drain);

            var v = new Bus("GO1");
            GameWorld.Instance.AddVehicle(v);

            bool fired = false;
            void H(object? _, EventArgs __) => fired = true;
            Model.GameOver += H;

            Model.UnPause();
            Model.Update(601);

            Model.GameOver -= H;
            GameWorld.Instance.RemoveVehicle(v);

            Assert.IsTrue(fired, "GameOver should fire when expenses exceed funds.");
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class GameModelAddStopTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 23);

        [TestMethod]
        public void AddStopToVehicle_DuplicateStop_NotAddedAgain()
        {
            var route = new Route();
            var bus = new Bus("DS1")
            {
                Route = route
            };
            var s = new AdvStation(new Coordinate(5, 5));
            GameWorld.Instance.GetField(new Coordinate(5, 5))!.Surface = s;
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(new Coordinate(5, 5));

            GameModel.AddStopToVehicle(bus, s);
            GameModel.AddStopToVehicle(bus, s);

            Assert.AreEqual(1, route.Stops.Count);
        }

        [TestMethod]
        public void AddStopToVehicle_WaitingVehicle_FiresRouteChanged()
        {
            var route = new Route();
            var bus = new Bus("RC1")
            {
                Route = route
            };
            var s = new AdvStation(new Coordinate(6, 6));
            GameWorld.Instance.GetField(new Coordinate(6, 6))!.Surface = s;

            bool fired = false;
            bus.RouteChanged += (_, _) => fired = true;

            GameModel.AddStopToVehicle(bus, s);

            Assert.IsTrue(fired);
        }
    }

    file class AdvStation(Coordinate coord) : Station(
        coord, "AdvStation",
        new ProductBuffer(ProductType.HUMAN, 20),
        new Product(ProductType.HUMAN, 0, 20))
    {
        public override int UnLoadProductFromVehicle(Vehicle vehicle) => 0;
    }
}
