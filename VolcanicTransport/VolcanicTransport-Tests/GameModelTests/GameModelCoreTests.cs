using VolcanicTransport.Model;
using VolcanicTransport.Model.Services;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_Tests.GameModelTests
{

    [TestClass]
    [DoNotParallelize]
    public class GameModelCoreTests
    {
        private static GameModel Model => GameModel.Instance;
        private static World World => World.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            GameModel.InitialiseNewGame(8, 99);
        }

        [TestMethod]
        public void StartingMoney_EqualsGameSettingsValue()
        {
            Assert.AreEqual(GameSettings.StartingMoney, Model.PlayerMoney);
        }

        [TestMethod]
        public void TryPurchase_SufficientFunds_DeductsAndReturnsTrue()
        {
            double before = Model.PlayerMoney;
            bool ok = Model.TryPurchase(100);
            Assert.IsTrue(ok);
            Assert.AreEqual(before - 100, Model.PlayerMoney, 0.001);
        }

        [TestMethod]
        public void TryPurchase_InsufficientFunds_ReturnsFalseAndDoesNotDeduct()
        {
            double before = Model.PlayerMoney;
            bool ok = Model.TryPurchase(before + 1);
            Assert.IsFalse(ok);
            Assert.AreEqual(before, Model.PlayerMoney, 0.001);
        }

        [TestMethod]
        public void MoneyChanged_FiredOnSuccessfulPurchase()
        {
            bool fired = false;
            Model.MoneyChanged += (_, _) => fired = true;
            Model.TryPurchase(1);
            Model.MoneyChanged -= (_, _) => fired = true;
            Assert.IsTrue(fired);
        }


        [TestMethod]
        public void Pause_SetsIsPausedTrue_AndFiresEvent()
        {
            bool eventFired = false;
            void handler(object? _, EventArgs __) => eventFired = true;
            Model.GamePaused += handler;
            Model.Pause();
            Model.GamePaused -= handler;

            Assert.IsTrue(Model.IsPaused);
            Assert.IsTrue(eventFired);
        }

        [TestMethod]
        public void UnPause_SetsIsPausedFalse_AndFiresEvent()
        {
            Model.Pause();
            bool eventFired = false;
            void handler(object? _, EventArgs __) => eventFired = true;
            Model.GameUnpaused += handler;
            Model.UnPause();
            Model.GameUnpaused -= handler;

            Assert.IsFalse(Model.IsPaused);
            Assert.IsTrue(eventFired);
        }

        [TestMethod]
        public void ChangeTimeSpeed1X_UnpausesAndFiresTimescaleEvent()
        {
            Model.Pause();
            bool fired = false;
            void handler(object? _, EventArgs __) => fired = true;
            Model.TimescaleChanged += handler;
            Model.ChangeTimeSpeed1X();
            Model.TimescaleChanged -= handler;

            Assert.IsFalse(Model.IsPaused);
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void ChangeTimeSpeed2X_UnpausesAndFiresTimescaleEvent()
        {
            Model.Pause();
            bool fired = false;
            void handler(object? _, EventArgs __) => fired = true;
            Model.TimescaleChanged += handler;
            Model.ChangeTimeSpeed2X();
            Model.TimescaleChanged -= handler;

            Assert.IsFalse(Model.IsPaused);
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void ChangeTimeSpeed4X_UnpausesAndFiresTimescaleEvent()
        {
            Model.Pause();
            bool fired = false;
            void handler(object? _, EventArgs __) => fired = true;
            Model.TimescaleChanged += handler;
            Model.ChangeTimeSpeed4X();
            Model.TimescaleChanged -= handler;

            Assert.IsFalse(Model.IsPaused);
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void Update_WhenNotPaused_IncrementsTime()
        {
            Model.UnPause();
            double before = Model.Time;
            Model.Update(1.0);
            Assert.IsTrue(Model.Time > before);
        }

        [TestMethod]
        public void Update_WhenPaused_DoesNotIncrementTime()
        {
            Model.Pause();
            double before = Model.Time;
            Model.Update(1.0);
            Assert.AreEqual(before, Model.Time, 0.001);
            Model.UnPause();
        }

        [TestMethod]
        public void Update_WhenNotPaused_FiresGameAdvancedEvent()
        {
            Model.UnPause();
            bool fired = false;
            void handler(object? _, EventArgs __) => fired = true;
            Model.GameAdvanced += handler;
            Model.Update(0.1);
            Model.GameAdvanced -= handler;
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void GetMushroomCosts_FieldWithNoSurface_ReturnsZero()
        {
            var field = new Field { Surface = null };
            var result = EconomyService.GetMushroomCosts(field);
            Assert.AreEqual(0.0, result, "Cost should be 0 if there is no surface.");
        }

        [TestMethod]
        public void GetMushroomCosts_FieldWithRoadSurface_ReturnsZero()
        {
            var field = new Field { Surface = new Road(new Coordinate(0, 0)) };
            var result = EconomyService.GetMushroomCosts(field);
            Assert.AreEqual(0.0, result, "Cost should be 0 if the surface is not a Mushroom.");
        }

        [TestMethod]
        public void GetMushroomCosts_SproutMushroom_ReturnsBasePrice()
        {
            var coord = new Coordinate(0, 0);
            var field = new Field
            {
                Surface = new Mushroom(coord, MushroomGrowthStage.SPROUT)
            };
            var result = EconomyService.GetMushroomCosts(field);
            // (0 + 1) * 100 = 100
            Assert.AreEqual(GameSettings.MushroomPricePerUnit, result, "Sprout stage cost calculation failed.");
        }

        [TestMethod]
        public void GetMushroomCosts_AdultMushroom_ReturnsCorrectMultiplier()
        {
            var coord = new Coordinate(0, 0);
            var field = new Field
            {
                Surface = new Mushroom(coord, MushroomGrowthStage.ADULT)
            };
            var result = EconomyService.GetMushroomCosts(field);
            // (2 + 1) * 100 = 300
            Assert.AreEqual(3 * GameSettings.MushroomPricePerUnit, result, "Adult stage cost calculation failed.");
        }


        [TestMethod]
        public void BuyVehicle_SufficientFunds_AddsToWorld_ReturnsTrue()
        {
            var v = new Bus("BuyTest");
            int before = World.Vehicles.Count;
            bool ok = Model.BuyVehicle(v);
            Assert.IsTrue(ok);
            Assert.AreEqual(before + 1, World.Vehicles.Count);
            Assert.IsTrue(World.HasVehicle(v));
        }

        [TestMethod]
        public void BuyVehicle_SufficientFunds_DeductsMoney()
        {
            double before = Model.PlayerMoney;
            var v = new MiniBus("BuyTest2");
            Model.BuyVehicle(v);
            Assert.IsTrue(Model.PlayerMoney < before);
        }

        [TestMethod]
        public void BuyVehicle_SufficientFunds_FiresVehicleBoughtEvent()
        {
            bool fired = false;
            void handler(object? _, EventArgs __) => fired = true;
            Model.VehicleBought += handler;
            Model.BuyVehicle(new Bus("BuyEv"));
            Model.VehicleBought -= handler;
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void SellVehicle_ExistingVehicle_RemovesFromWorldAndRefunds()
        {
            var v = new Bus("SellTest");
            Model.BuyVehicle(v);
            double before = Model.PlayerMoney;
            int countBefore = World.Vehicles.Count;

            Model.SellVehicle(v);

            Assert.AreEqual(countBefore - 1, World.Vehicles.Count);
            Assert.IsFalse(World.HasVehicle(v));
            Assert.IsTrue(Model.PlayerMoney > before, "Eladás után pénzt kell visszakapni.");
        }

        [TestMethod]
        public void SellVehicle_RefundsHalfPrice()
        {
            var v = new Bus("SellHalf");
            Model.BuyVehicle(v);
            double before = Model.PlayerMoney;
            Model.SellVehicle(v);
            Assert.AreEqual(before + v.Price * GameSettings.SellRefundRate, Model.PlayerMoney, 0.001);
        }

        [TestMethod]
        public void SellVehicle_VehicleNotInWorld_DoesNothing()
        {
            var v = new Bus("NotOwned");
            double before = Model.PlayerMoney;
            Model.SellVehicle(v);
            Assert.AreEqual(before, Model.PlayerMoney, 0.001);
        }

        [TestMethod]
        public void SellVehicle_FiresVehicleSoldEvent()
        {
            var v = new Bus("SellEv");
            Model.BuyVehicle(v);
            bool fired = false;
            void handler(object? _, EventArgs __) => fired = true;
            Model.VehicleSold += handler;
            Model.SellVehicle(v);
            Model.VehicleSold -= handler;
            Assert.IsTrue(fired);
        }


        [TestMethod]
        public void IsBuildable_OutOfBoundsCoordinate_ReturnsFalse()
        {
            var coord = new Coordinate(World.SizeInFields.X + 100, World.SizeInFields.Y + 100);
            Assert.IsFalse(Model.IsBuildable(coord));
        }

        [TestMethod]
        public void IsHeightenable_OutOfBoundsCoordinate_ReturnsFalse()
        {
            var coord = new Coordinate(-1, -1);
            Assert.IsFalse(Model.IsHeightenable(coord));
        }

        [TestMethod]
        public void IsLowerable_OutOfBoundsCoordinate_ReturnsFalse()
        {
            var coord = new Coordinate(-1, -1);
            Assert.IsFalse(Model.IsLowerable(coord));
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class GameModelPlacementTests
    {
        private static GameModel Model => GameModel.Instance;
        private static World World => World.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            GameModel.InitialiseNewGame(8, 42);
        }

        private static Coordinate? FindBuildableFlat()
        {
            for (int y = 0; y < World.SizeInFields.Y; y++)
                for (int x = 0; x < World.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = World.GetField(c);
                    if (f != null && f.Surface is null && f.Type == FieldType.LOW_LANDS)
                        return c;
                }
            return null;
        }

        [TestMethod]
        public void PlaceRoad_InvalidCoordinate_DoesNotThrow()
        {
            Model.PlaceRoad(new Coordinate(-1, -1));
        }

        [TestMethod]
        public void PlaceRoad_NoBudget_CannotPlace()
        {
            Model.TryPurchase(Model.PlayerMoney);
            var coord = FindBuildableFlat();
            if (coord == null) return;
            var field = World.GetField(coord.Value)!;

            Model.PlaceRoad(coord.Value);

            Assert.IsNull(field.Surface, "Pénz nélkül nem kerülhet út ide.");
        }

        [TestMethod]
        public void HeightenField_OutOfBoundsCoordinate_DoesNotThrow()
        {
            Model.HeightenField(new Coordinate(-1, -1));
        }

        [TestMethod]
        public void LowerField_OutOfBoundsCoordinate_DoesNotThrow()
        {
            Model.LowerField(new Coordinate(-1, -1));
        }

        [TestMethod]
        public void PlaceStation_InvalidCoordinate_ReturnsFalse()
        {
            bool ok = Model.PlaceStation(new Coordinate(-1, -1));
            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void PlaceBridge_StartEqualsEnd_ReturnsFalse()
        {
            var coord = new Coordinate(5, 5);
            bool ok = Model.PlaceBridge(coord, coord, GameSettings.BridgeTypes[0]);
            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void PlaceBridge_DiagonalPoints_ReturnsFalse()
        {
            bool ok = Model.PlaceBridge(
                new Coordinate(0, 0), new Coordinate(2, 2), GameSettings.BridgeTypes[0]);
            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void PlaceBridge_TooShort_ReturnsFalse()
        {
            bool ok = Model.PlaceBridge(
                new Coordinate(0, 0), new Coordinate(1, 0), GameSettings.BridgeTypes[0]);
            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void AddStopToVehicle_WithRoute_AddsStop()
        {
            var route = new Route();
            var bus = new Bus("StopTest")
            {
                Route = route
            };
            var station = new TestStation(new Coordinate(5, 5));
            World.GetField(new Coordinate(5, 5))!.Surface = station;
            World.Roadnetwork.RegisterNodeIfNeeded(new Coordinate(5, 5));

            GameModel.AddStopToVehicle(bus, station);

            Assert.AreEqual(1, route.Stops.Count);
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class GameModelVehicleArrivedTests
    {
        private static GameModel Model => GameModel.Instance;
        private static World World => World.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 7);

        [TestMethod]
        public void HandleVehicleArrived_Bus_UnboardsAndBoards()
        {
            var city = new City("ArrCity", new Coordinate(1, 1));
            var station = new CityStation(city, new Coordinate(3, 3), "ArrStation");
            station.GetWaitingPassengers(500);
            var bus = new Bus("ArrBus");
            bus.Load(10, ProductType.HUMAN);

            var args = new VehicleArrivedEventArgs(bus, station);

            Model.HandleVehicleArrived(null, args);
        }

        [TestMethod]
        public void HandleVehicleArrived_CargoTruck_UnloadsAtCityStation()
        {
            var city = new City("CargoCity", new Coordinate(1, 1));
            var station = new CityStation(city, new Coordinate(2, 3), "CargoStation");
            var truck = new CargoTruck("ArrTruck");
            var neededType = city.ProductTypes.FirstOrDefault();
            if (neededType == ProductType.NONE) return;
            truck.Load(50, neededType);

            double moneyBefore = Model.PlayerMoney;
            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(truck, station));
            Assert.IsTrue(Model.PlayerMoney >= moneyBefore, "Áruszállítás után pénzt kell kapni.");
        }

        [TestMethod]
        public void HandleVehicleArrived_FiresVehicleArrivedAtStationEvent()
        {
            var city = new City("EvCity", new Coordinate(1, 1));
            var station = new CityStation(city, new Coordinate(2, 2), "EvStation");
            var bus = new Bus("EvBus");
            bool fired = false;
            Model.VehicleArrivedAtStation += (_, _) => fired = true;

            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(bus, station));

            Model.VehicleArrivedAtStation -= (_, _) => fired = true;
            Assert.IsTrue(fired);
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class GameModelInsufficientFundsTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 13);

        [TestMethod]
        public void BuyVehicle_InsufficientFunds_ReturnsFalse()
        {
            Model.TryPurchase(Model.PlayerMoney - 1);
            bool ok = Model.BuyVehicle(new Bus("NoBudget"));
            Assert.IsFalse(ok);
        }
    }

    file class TestStation(Coordinate coord) : Station(
        coord, "TestStation",
        new ProductBuffer(ProductType.HUMAN, 50),
        new Product(ProductType.HUMAN, 0, 50))
    {
        public override int UnLoadProductFromVehicle(Vehicle vehicle) => 0;
    }
}
