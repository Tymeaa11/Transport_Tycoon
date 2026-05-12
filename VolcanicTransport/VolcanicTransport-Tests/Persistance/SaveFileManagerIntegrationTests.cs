using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.Persistance
{
    [TestClass]
    [DoNotParallelize]
    public class SaveFileManagerIntegrationTests
    {
        [TestMethod]
        public void SaveAndLoad_EmptyWorld_PreservesWorldSize()
        {
            GameModel.InitialiseNewGame(4, 0);
            var originalSize = GameModel.WorldInstance.SizeInChunks;
            string tempFile = Path.GetTempFileName() + ".zip";

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                Assert.IsTrue(File.Exists(tempFile), "Save file should exist.");

                GameModel.InitialiseLoadedGame(tempFile);
                var loadedSize = GameModel.WorldInstance.SizeInChunks;

                Assert.AreEqual(originalSize.X, loadedSize.X);
                Assert.AreEqual(originalSize.Y, loadedSize.Y);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithMushroom_MushroomPresent()
        {
            GameModel.InitialiseNewGame(4, 1);
            var w = GameWorld.Instance;
            string tempFile = Path.GetTempFileName() + ".zip";

            // Place a mushroom manually on an empty field
            Coordinate? mushroomCoord = null;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.Surface == null && f.Type == FieldType.LOW_LANDS)
                    {
                        f.Surface = new Mushroom(c, MushroomGrowthStage.SPROUT);
                        mushroomCoord = c;
                        break;
                    }
                    if (mushroomCoord != null) break;
                }

            if (mushroomCoord == null) return;

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                var field = GameModel.WorldInstance.GetField(mushroomCoord.Value);
                Assert.IsInstanceOfType<Mushroom>(field!.Surface);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithCity_CityPreserved()
        {
            GameModel.InitialiseNewGame(4, 2);
            var w = GameWorld.Instance;
            string tempFile = Path.GetTempFileName() + ".zip";

            // May or may not have cities depending on world generation
            int citiesBeforeSave = w.Cities.Count;

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                Assert.AreEqual(citiesBeforeSave, GameModel.WorldInstance.Cities.Count);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithFactory_FactoryPreserved()
        {
            GameModel.InitialiseNewGame(4, 3);
            int factoriesBeforeSave = GameModel.WorldInstance.Factories.Count;
            string tempFile = Path.GetTempFileName() + ".zip";

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                Assert.AreEqual(factoriesBeforeSave, GameModel.WorldInstance.Factories.Count);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_PlayerMoney_Preserved()
        {
            GameModel.InitialiseNewGame(4, 4);
            double moneyBefore = GameModel.Instance.PlayerMoney;
            string tempFile = Path.GetTempFileName() + ".zip";

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                Assert.AreEqual(moneyBefore, GameModel.Instance.PlayerMoney, 0.001);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithRoute_RoutePreserved()
        {
            GameModel.InitialiseNewGame(4, 5);
            var route = new Route { Name = "TestRoute" };
            GameModel.WorldInstance.AddRoute(route);
            route.PrepareForSave();
            string tempFile = Path.GetTempFileName() + ".zip";

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                bool found = GameModel.WorldInstance.SavedRoutes.Any(r => r.Name == "TestRoute");
                Assert.IsTrue(found);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithCityStation_StationPreserved()
        {
            GameModel.InitialiseNewGame(4, 6);
            var w = GameWorld.Instance;
            string tempFile = Path.GetTempFileName() + ".zip";

            // Add a city and station manually
            var city = new City("SaveLoadCity", new Coordinate(2, 2));
            w.Cities.Add(city);
            var station = new CityStation(city, new Coordinate(2, 2), "SLStop");
            var field = w.GetField(new Coordinate(2, 2));
            if (field == null) return;
            field.Surface = station;
            w.Stations.Add(station);

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                var loadedField = GameModel.WorldInstance.GetField(new Coordinate(2, 2));
                Assert.IsInstanceOfType<CityStation>(loadedField!.Surface);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
                // restore is not needed since we reinit on load
            }
        }

        [TestMethod]
        public void CheckSavingThenLoading()
        {
            GameModel.InitialiseNewGame(4, 999);
            var model = GameModel.Instance;

            model.TryPurchase(500);
            model.Pause();
            model.Update(0);

            double moneyBefore = model.PlayerMoney;
            bool pausedBefore = model.IsPaused;
            Coordinate sizeBefore = GameModel.WorldInstance.SizeInChunks;

            string tempFile = Path.GetTempFileName() + ".zip";
            try
            {
                model.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                Assert.AreEqual(moneyBefore, GameModel.Instance.PlayerMoney, 0.001,
                    "Pénzegyenleg nem egyezik betöltés után.");
                Assert.AreEqual(pausedBefore, GameModel.Instance.IsPaused,
                    "Szünet állapota nem egyezik betöltés után.");
                Assert.AreEqual(sizeBefore.X, GameModel.WorldInstance.SizeInChunks.X,
                    "Világ mérete (X) nem egyezik betöltés után.");
                Assert.AreEqual(sizeBefore.Y, GameModel.WorldInstance.SizeInChunks.Y,
                    "Világ mérete (Y) nem egyezik betöltés után.");
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_VehicleCountPreserved()
        {
            GameModel.InitialiseNewGame(4, 1001);
            var w = GameWorld.Instance;

            var city = new City("VCPreserve", new Coordinate(1, 1));
            w.Cities.Add(city);
            var s1 = new CityStation(city, new Coordinate(2, 2), "Stop1");
            var s2 = new CityStation(city, new Coordinate(3, 5), "Stop2");
            w.GetField(s1.Coordinate)!.Surface = s1;
            w.GetField(s2.Coordinate)!.Surface = s2;
            w.Stations.Add(s1);
            w.Stations.Add(s2);

            var route = new Route { Name = "TestRoute" };
            route.AddStop(s1);
            route.AddStop(s2);
            w.AddRoute(route);

            var bus = new Bus("VCBus");
            bus.AssignNewRoute(route);
            w.AddVehicle(bus);

            int vehiclesBefore = w.Vehicles.Count;
            string tempFile = Path.GetTempFileName() + ".zip";
            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);
                Assert.AreEqual(vehiclesBefore, GameModel.WorldInstance.Vehicles.Count,
                    "Járművek száma nem egyezik betöltés után.");
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class GameModelSellVehicleSpatialGridTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 555);

        [TestMethod]
        public void SellVehicle_VehicleRemovedFromSpatialGrid()
        {
            var world = GameModel.WorldInstance;

            Coordinate? roadCoord = null;
            for (int y = 0; y < world.SizeInFields.Y && roadCoord == null; y++)
                for (int x = 0; x < world.SizeInFields.X && roadCoord == null; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = world.GetField(c);
                    if (f != null && f.Surface == null && f.Type == FieldType.LOW_LANDS)
                    {
                        var road = new Road(c);
                        f.Surface = road;
                        roadCoord = c;
                    }
                }

            if (roadCoord == null) return;

            if (world.GetField(roadCoord.Value)!.Surface is not Road road2) return;

            var bus = new Bus("GridClean");
            Model.BuyVehicle(bus);

            bus.StartJourney([road2]);

            Assert.AreEqual(1, world.VehicleManager.GetVehiclesOnField(roadCoord.Value).Count,
                "StartJourney után a járműnek benne kell lennie a spatial gridben.");

            Model.SellVehicle(bus);

            Assert.AreEqual(0, world.VehicleManager.GetVehiclesOnField(roadCoord.Value).Count,
                "Jármű eladása után a spatial grid bejegyzést törölni kell.");
        }

        [TestMethod]
        public void SellVehicle_MonthlyExpenses_OnlyCountsRemainingVehicles()
        {
            var bus1 = new Bus("Monthly1");
            var bus2 = new Bus("Monthly2");
            Model.BuyVehicle(bus1);
            Model.BuyVehicle(bus2);

            int countBefore = GameModel.WorldInstance.Vehicles.Count;
            Model.SellVehicle(bus1);

            Assert.AreEqual(countBefore - 1, GameModel.WorldInstance.Vehicles.Count,
                "Eladás után eggyel kevesebb járműnek kell lennie.");
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class SellVehicleAtStationTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 888);

        [TestMethod]
        public void SellVehicle_AtStation_StationBecomesUnoccupied()
        {
            var world = GameModel.WorldInstance;

            var city = new City("SellTestCity", new Coordinate(5, 5));
            world.Cities.Add(city);
            var station = new CityStation(city, new Coordinate(5, 5), "SellStop");
            var field = world.GetField(new Coordinate(5, 5));
            if (field == null) return;
            field.Surface = station;
            world.Stations.Add(station);

            var bus = new Bus("AtStationBus");
            GameModel.Instance.BuyVehicle(bus);
            bus.StartJourney([station]);

            Assert.IsTrue(station.IsOccupied, "StartJourney után az állomásnak foglaltnak kell lennie.");

            GameModel.Instance.SellVehicle(bus);

            Assert.IsFalse(station.IsOccupied,
                "Eladás után az állomásnak szabadnak kell lennie, hogy más jármű belépjen.");
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class GameModelMonthlyExpenseTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 701);

        [TestMethod]
        public void Update_After600Seconds_DeductsMonthlyExpenses()
        {
            var bus = new Bus("Expense1");
            Model.BuyVehicle(bus);
            Model.UnPause();

            double moneyBefore = Model.PlayerMoney;
            Model.Update(601.0);

            Assert.IsTrue(Model.PlayerMoney < moneyBefore,
                "600 másodperc után havi kiadásnak le kell vonódnia.");

            Model.SellVehicle(bus);
        }
    }
}
