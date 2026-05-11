using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_Tests.Utils
{
    [TestClass]
    [DoNotParallelize]
    public class PersistanceTests
    {
        private SaveFileManager? _saveManager;
        private string _testPath = "test_persistence.zip";

        [TestInitialize]
        public void Setup()
        {
            _saveManager = new SaveFileManager();

            _testPath = Path.Combine(Path.GetTempPath(), $"test_save_{Guid.NewGuid()}.zip");
            World.Initialise(4, 42);
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (File.Exists(_testPath)) File.Delete(_testPath);
        }

        // TODO Rectactor this
        public void CheckSavingThenLoading()
        {
            var world = World.Instance;
            var money = 123456.0;
            var gameTime = 500.5;

            Coordinate roadCoord = new(5, 5);
            world.GetField(roadCoord)!.Surface = new Road(roadCoord);

            Coordinate mushroomCoord = new(6, 6);
            world.GetField(mushroomCoord)!.Surface = new Mushroom(mushroomCoord, MushroomGrowthStage.ADULT);

            var testRoute = new Route { Name = "Express 1" };
            world.AddRoute(testRoute);

            var tanker = new TankerTruck("BigRed")
            {
                PosX = 10.5f,
                PosY = 12.2f,
                Angle = 0.5f,
                State = VehicleState.Moving
            };
            tanker.AssignNewRoute(testRoute);
            world.AddVehicle(tanker);

            var saveData = new ISaveFileManager.GameData(world, true, gameTime, money);
            _saveManager!.SaveGame(saveData, _testPath);

            World.Initialise(4, 42);
            Assert.AreEqual(0, World.Instance.Vehicles.Count, "World reset failed.");

            var loadedData = _saveManager.LoadGame(_testPath);

            Assert.AreEqual(money, loadedData.PlayerMoney, "Money mismatch after load.");
            Assert.AreEqual(gameTime, loadedData.Time, "Time mismatch after load.");
            Assert.IsTrue(loadedData.IsPaused, "Paused state mismatch.");

            // Binary
            var roadField = World.Instance.GetField(roadCoord);
            Assert.IsInstanceOfType<Road>(roadField!.Surface, "Road was not restored.");

            var mushField = World.Instance.GetField(mushroomCoord);
            Assert.IsInstanceOfType<Mushroom>(mushField!.Surface, "Mushroom was not restored.");
            Assert.AreEqual(MushroomGrowthStage.ADULT, ((Mushroom)mushField.Surface).GrowthStage);

            // JSON
            Assert.AreEqual(1, World.Instance.Vehicles.Count, "Vehicle was not restored.");
            var loadedVehicle = World.Instance.Vehicles[0];

            Assert.AreEqual("BigRed", loadedVehicle.Name);
            Assert.AreEqual(10.5f, loadedVehicle.PosX, 0.01f, "Position X mismatch.");

            
            Assert.IsNotNull(loadedVehicle.Route, "Vehicle route reference was not re-linked.");
            Assert.AreEqual("Express 1", loadedVehicle.Route.Name);
        }


        [TestMethod]
        public void CheckIfFactoryStateIsPeserved()
        {
            var world = World.Instance;
            var factory = new ConcreteFactory("TestFactory", new Coordinate(10, 10));
            typeof(Factory).GetField("productionAccumulator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(factory, 0.75);

            world.Factories.Add(factory);

            _saveManager!.SaveGame(new ISaveFileManager.GameData(world, false, 0, 0), _testPath);
            World.Initialise(4, 42);
            _saveManager.LoadGame(_testPath);

            var loadedFactory = World.Instance.Factories[0];
            var accValue = typeof(Factory).GetField("productionAccumulator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(loadedFactory);

            Assert.AreEqual(0.75, (double)accValue!, 0.001, "Production accumulator reset to zero.");
        }
    }
}
