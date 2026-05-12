using VolcanicTransport.Model.Persistance;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.Persistance
{
    [TestClass]
    [DoNotParallelize]
    public class PersistenceTests
    {
        private SaveFileManager _saveManager = null!;
        private string _testPath = null!;
        private const int WorldSize = 1;
        private const int Seed = 12345;

        [TestInitialize]
        public void Setup()
        {
            _saveManager = new SaveFileManager(new Zip2FileSaveFormat());
            _testPath = Path.Combine(Path.GetTempPath(), $"persistence_test_{Guid.NewGuid()}.zip");

            GameWorld.Initialise(WorldSize, Seed);
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (File.Exists(_testPath)) File.Delete(_testPath);
        }

        private GameData SaveAndLoad(GameData data)
        {
            _saveManager.SaveGame(data, _testPath);
            GameWorld.Initialise(WorldSize, Seed);
            return _saveManager.LoadGame(_testPath, (s, e) => { });
        }

        #region Basic & Binary Map Tests

        [TestMethod]
        public void TestGameStateMetadata()
        {
            var original = new GameData(GameWorld.Instance, true, 420.69, 75000.0);
            var loaded = SaveAndLoad(original);

            Assert.AreEqual(original.PlayerMoney, loaded.PlayerMoney, 0.001);
            Assert.AreEqual(original.Time, loaded.Time, 0.001);
            Assert.IsTrue(loaded.IsPaused);
        }

        [TestMethod]
        public void TestBinaryMapBitmasks()
        {
            var world = GameWorld.Instance;
            Coordinate roadCoord = new(1, 1);
            Coordinate mushCoord = new(2, 2);
            Coordinate heightCoord = new(3, 3);

            world.GetField(roadCoord)!.Surface = new Road(roadCoord) { IsReserved = true };

            world.GetField(mushCoord)!.Surface = new Mushroom(mushCoord, MushroomGrowthStage.ADULT);

            world.GetField(heightCoord)!.SetFieldTypeTo(FieldType.HIGH_MOUNTAINS);

            SaveAndLoad(new GameData(world, false, 0, 0));

            var loadedRoad = GameWorld.Instance.GetField(roadCoord)!.Surface as Road;
            var loadedMush = GameWorld.Instance.GetField(mushCoord)!.Surface as Mushroom;

            Assert.IsTrue(loadedRoad!.IsReserved, "Road reservation bit lost in binary save.");
            Assert.AreEqual(MushroomGrowthStage.ADULT, loadedMush!.GrowthStage, "Mushroom stage bits corrupted.");
            Assert.AreEqual(FieldType.HIGH_MOUNTAINS, GameWorld.Instance.GetField(heightCoord)!.Type, "Terrain height corrupted.");
        }

        #endregion

        #region Economic Entity & Reference Restoration

        [TestMethod]
        public void TestCityAndBuildingReferenceRestoration()
        {
            var world = GameWorld.Instance;
            var city = new City("Metropolis", new Coordinate(10, 10));
            world.Cities.Add(city);

            var buildingCoord = new Coordinate(11, 11);
            var building = new CityBuilding(city);
            var buildingField = world.GetField(buildingCoord);

            Assert.IsNotNull(buildingField);
            buildingField!.Surface = building;

            city.AddField(buildingField);

            SaveAndLoad(new GameData(world, false, 0, 0));

            var loadedBuilding = GameWorld.Instance.GetField(buildingCoord)!.Surface as CityBuilding;
            Assert.IsNotNull(loadedBuilding);

            Assert.IsTrue(GameWorld.Instance.Cities.Count == 1);
            Assert.IsTrue(GameWorld.Instance.Cities[0].Name == city.Name);

            Assert.AreEqual("Metropolis", loadedBuilding.CityName);
        }

        [TestMethod]
        public void TestPolymorphicFactoryRestoration()
        {
            var world = GameWorld.Instance;
            var sulfur = new SulfurProducer("Sulfur Mine", new Coordinate(5, 5));
            var ash = new AshProducer("Ash Plant", new Coordinate(15, 15));

            world.Factories.Add(sulfur);
            world.Factories.Add(ash);

            SaveAndLoad(new GameData(world, false, 0, 0));

            var loadedSulfur = GameWorld.Instance.Factories.FirstOrDefault(f => f.Name == "Sulfur Mine");
            var loadedAsh = GameWorld.Instance.Factories.FirstOrDefault(f => f.Name == "Ash Plant");

            Assert.IsInstanceOfType<SulfurProducer>(loadedSulfur, "SulfurProducer lost its specific type.");
            Assert.IsInstanceOfType<AshProducer>(loadedAsh, "AshProducer lost its specific type.");
            Assert.AreEqual(ProductType.SULFUR, loadedSulfur!.FinalProduct.ProductType);
        }

        [TestMethod]
        public void TestStationAndFactoryConnection()
        {
            var world = GameWorld.Instance;
            var factory = new ConcreteFactory("C-Fac", new Coordinate(20, 20));
            world.Factories.Add(factory);

            var stationCoord = new Coordinate(21, 21);
            var station = new FactoryStation(stationCoord, "C-Station", factory);
            world.GetField(stationCoord)!.Surface = station;
            world.Stations.Add(station);

            SaveAndLoad(new GameData(world, false, 0, 0));

            var loadedStation = GameWorld.Instance.Stations.First() as FactoryStation;
            Assert.IsNotNull(loadedStation);
            Assert.AreEqual("C-Fac", loadedStation.FactoryName);
        }

        #endregion

        #region Road Network & Routing

        [TestMethod]
        public void TestVehicleAndRouteComplexRestoration()
        {
            var world = GameWorld.Instance;

            var city = new City("Town", new Coordinate(0, 0));
            var s1 = new CityStation(city, new Coordinate(1, 1), "Stop A");
            var s2 = new CityStation(city, new Coordinate(1, 5), "Stop B");
            world.Cities.Add(city);
            world.Stations.Add(s1);
            world.Stations.Add(s2);
            world.GetField(s1.Coordinate)!.Surface = s1;
            world.GetField(s2.Coordinate)!.Surface = s2;

            var route = new Route { Name = "Express" };
            route.AddStop(s1);
            route.AddStop(s2);
            world.AddRoute(route);


            var bus = new Bus("Bus-99");
            bus.Load(5, ProductType.HUMAN);
            bus.AssignNewRoute(route);
            world.AddVehicle(bus);

            SaveAndLoad(new GameData(world, false, 0, 0));

            var loadedBus = GameWorld.Instance.Vehicles[0];
            var loadedRoute = GameWorld.Instance.SavedRoutes[0];


            Assert.AreEqual("Bus-99", loadedBus.Name);
            Assert.AreEqual(5, loadedBus.CurrentLoad);
            Assert.AreEqual(ProductType.HUMAN, loadedBus.CurrentType);
            Assert.IsNotNull(loadedBus.Route, "Vehicle's Route reference was not restored.");
            Assert.AreEqual(loadedBus.Route, loadedRoute);
            Assert.AreEqual("Express", loadedBus.Route.Name);
            Assert.AreEqual(2, loadedBus.Route.Stops.Count);

            Assert.AreSame(GameWorld.Instance.Stations[0], loadedBus.Route.Stops[0], "Route stops not pointing to restored Station instances.");
        }

        #endregion

        #region Bridge Persistence

        [TestMethod]
        public void TestBridgePersistence()
        {
            var world = GameWorld.Instance;
            Coordinate bridgeCoord = new(10, 10);

            var bridge = new SteelBridge(bridgeCoord, RoadType.STRAIGHT_NS, FieldType.HIGH_LANDS);
            world.GetField(bridgeCoord)!.Surface = bridge;

            SaveAndLoad(new GameData(world, false, 0, 0));

            var loadedBridge = GameWorld.Instance.GetField(bridgeCoord)!.Surface as SteelBridge;
            Assert.IsNotNull(loadedBridge);
            Assert.AreEqual(RoadType.STRAIGHT_NS, loadedBridge.RoadType);
            Assert.AreEqual(FieldType.HIGH_LANDS, loadedBridge.Elevation);
        }

        #endregion
    }
}