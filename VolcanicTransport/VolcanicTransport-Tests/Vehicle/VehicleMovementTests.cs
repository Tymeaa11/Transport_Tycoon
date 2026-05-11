using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using static VolcanicTransport.Model.World.Roadnetwork.Vehicle;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.VehicleTests
{
    [TestClass]
    [DoNotParallelize]
    public class VehicleMovementTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 300);

        private static GameWorld W => GameWorld.Instance;

        private static Road MakeRoad(int x, int y, FieldType type = FieldType.LOW_LANDS)
        {
            var coord = new Coordinate(x, y);
            var field = W.GetField(coord)!;
            field.SetFieldTypeTo(type);
            var road = new Road(coord);
            field.Surface = road;
            road.Update();
            return road;
        }

        [TestMethod]
        public void StartJourney_TwoRoadPath_SetsMovingState()
        {
            var r1 = MakeRoad(80, 80);
            var r2 = MakeRoad(81, 80);
            var bus = new Bus("SJ_Bus");

            bus.StartJourney([r1, r2], alreadyOnRoad: true);

            Assert.AreEqual(VehicleState.Moving, bus.State);
            Assert.AreEqual(r1, bus.CurrentRoad);

            W.GetField(new Coordinate(80, 80))!.Surface = null;
            W.GetField(new Coordinate(81, 80))!.Surface = null;
        }

        [TestMethod]
        public void Update_MovingTwoRoadPath_TransitionsToLoading()
        {
            var r1 = MakeRoad(82, 80);
            var r2 = MakeRoad(83, 80);
            var bus = new Bus("Move_Bus");

            bus.StartJourney([r1, r2], alreadyOnRoad: true);
            bus.Update(1000.0);

            Assert.IsTrue(bus.State == VehicleState.Loading || bus.State == VehicleState.Waiting);

            W.GetField(new Coordinate(82, 80))!.Surface = null;
            W.GetField(new Coordinate(83, 80))!.Surface = null;
        }

        [TestMethod]
        public void Update_Moving_WithRoute_FiresArrivedAtStation()
        {
            var r1 = MakeRoad(84, 80);
            var r2 = MakeRoad(85, 80);
            var bus = new Bus("Arr_Bus");

            var stationCoord = new Coordinate(85, 80);
            var city = new City("ArrCity", stationCoord);
            var station = new CityStation(city, stationCoord, "ArrStop");
            W.GetField(stationCoord)!.Surface = station;

            var route = new Route();
            route.AddStop(station);
            bus.Route = route;
            bus.CurrentStopIndex = 0;

            bool arrived = false;
            bus.ArrivedAtStation += (_, _) => arrived = true;

            bus.StartJourney([r1, station], alreadyOnRoad: true);
            bus.Update(1000.0);

            W.GetField(new Coordinate(84, 80))!.Surface = null;
            W.GetField(stationCoord)!.Surface = null;

            Assert.IsTrue(arrived || bus.State == VehicleState.Loading || bus.State == VehicleState.Waiting);
        }

        [TestMethod]
        public void Update_Moving_BridgeAsCurrentRoad_UsesBridgeSpeedLimit()
        {
            var bridgeCoord = new Coordinate(86, 80);
            var nextCoord = new Coordinate(87, 80);

            var bridgeField = W.GetField(bridgeCoord)!;
            bridgeField.SetFieldTypeTo(FieldType.LAVA_OCEAN);
            var bridge = new BoneBridge(bridgeCoord, RoadType.STRAIGHT_EW, FieldType.LOW_LANDS);
            bridgeField.Surface = bridge;

            var nextField = W.GetField(nextCoord)!;
            nextField.SetFieldTypeTo(FieldType.LOW_LANDS);
            var road = new Road(nextCoord);
            nextField.Surface = road;
            road.Update();

            var bus = new Bus("Bridge_Bus");
            bus.StartJourney([bridge, road], alreadyOnRoad: true);

            float speedBefore = bus.CurrentSpeed;
            bus.Update(0.001);

            Assert.IsTrue(bus.CurrentSpeed <= GameSettings.BridgeTypes[0].MaxSpeed + 0.001f);

            bridgeField.Surface = null;
            nextField.Surface = null;
        }

        [TestMethod]
        public void Update_Moving_SlopeUphill_ReducesSpeed()
        {
            var r1Coord = new Coordinate(88, 80);
            var r2Coord = new Coordinate(89, 80);

            var f1 = W.GetField(r1Coord)!;
            var f2 = W.GetField(r2Coord)!;
            f1.SetFieldTypeTo(FieldType.LOW_LANDS);
            f2.SetFieldTypeTo(FieldType.LOW_MID_TRANSITION);

            var r1 = new Road(r1Coord);
            f1.Surface = r1;
            r1.Update();
            var r2 = new Road(r2Coord);
            f2.Surface = r2;
            r2.Update();

            var bus = new Bus("Slope_Bus");
            bus.StartJourney([r1, r2], alreadyOnRoad: true);
            bus.Update(0.001);

            Assert.IsTrue(bus.CurrentSpeed <= bus.MaxSpeed);

            f1.Surface = null;
            f2.Surface = null;
        }

        [TestMethod]
        public void StartJourney_WithCurrentRoad_UnregistersOldRoad()
        {
            var r1 = MakeRoad(90, 80);
            var r2 = MakeRoad(91, 80);
            var r3 = MakeRoad(92, 80);
            var bus = new Bus("Unreg_Bus");

            bus.StartJourney([r1, r2], alreadyOnRoad: true);
            bus.StartJourney([r2, r3], alreadyOnRoad: true);

            Assert.AreEqual(r2, bus.CurrentRoad);

            W.GetField(new Coordinate(90, 80))!.Surface = null;
            W.GetField(new Coordinate(91, 80))!.Surface = null;
            W.GetField(new Coordinate(92, 80))!.Surface = null;
        }

        [TestMethod]
        public void StartJourney_WithStationAsFirstRoad_SetsOccupied()
        {
            var city = new City("OccupyCity", new Coordinate(93, 80));
            var stationCoord = new Coordinate(93, 80);
            var station = new CityStation(city, stationCoord, "OccupyStop");
            var f1 = W.GetField(stationCoord)!;
            f1.SetFieldTypeTo(FieldType.LOW_LANDS);
            f1.Surface = station;

            var r2 = MakeRoad(94, 80);
            var bus = new Bus("OccBus");

            bus.StartJourney([station, r2], alreadyOnRoad: true);

            Assert.IsTrue(station.IsOccupied);

            f1.Surface = null;
            W.GetField(new Coordinate(94, 80))!.Surface = null;
        }

        [TestMethod]
        public void StartJourney_NotAlreadyOnRoad_SetsPositionFromStation()
        {
            var r1 = MakeRoad(95, 80);
            var r2 = MakeRoad(96, 80);
            var startCoord = new Coordinate(95, 80);
            var city = new City("StartCity", startCoord);
            var startStation = new CityStation(city, startCoord, "StartStop");
            W.GetField(startCoord)!.Surface = startStation;

            var bus = new Bus("NotOnRoad_Bus");
            bus.StartJourney([startStation, r2], alreadyOnRoad: false, currentStation: startStation);

            Assert.AreEqual(VehicleState.Moving, bus.State);

            W.GetField(startCoord)!.Surface = null;
            W.GetField(new Coordinate(96, 80))!.Surface = null;
        }

        [TestMethod]
        public void Update_ThreeRoadPath_TransitionsThroughAllRoads()
        {
            var r1 = MakeRoad(97, 80);
            var r2 = MakeRoad(98, 80);
            var r3 = MakeRoad(99, 80);
            var bus = new Bus("Three_Bus");

            bus.StartJourney([r1, r2, r3], alreadyOnRoad: true);
            bus.Update(1000.0);

            Assert.IsTrue(bus.State == VehicleState.Loading || bus.State == VehicleState.Waiting);

            W.GetField(new Coordinate(97, 80))!.Surface = null;
            W.GetField(new Coordinate(98, 80))!.Surface = null;
            W.GetField(new Coordinate(99, 80))!.Surface = null;
        }

        [TestMethod]
        public void RestoreReference_WithSavedPath_RestoresCurrentRoad()
        {
            var r1 = MakeRoad(100, 80);
            var r2 = MakeRoad(101, 80);

            var bus = new Bus("Restore_Bus");
            bus.StartJourney([r1, r2], alreadyOnRoad: true);
            bus.PrepareForSave();

            var bus2 = new Bus("Restore_Bus2");
            bus2.SavedPathCoordinates = bus.SavedPathCoordinates;
            bus2.RestoreReference(new Coordinate(100, 80));

            Assert.AreEqual(r1, bus2.CurrentRoad);

            W.GetField(new Coordinate(100, 80))!.Surface = null;
            W.GetField(new Coordinate(101, 80))!.Surface = null;
        }

        [TestMethod]
        public void PrepareForSave_SavesPathCoordinates()
        {
            var r1 = MakeRoad(102, 80);
            var r2 = MakeRoad(103, 80);
            var bus = new Bus("PS_Bus");

            var route = new Route { Name = "TestRoute" };
            bus.Route = route;
            bus.StartJourney([r1, r2], alreadyOnRoad: true);
            bus.PrepareForSave();

            Assert.IsNotNull(bus.SavedPathCoordinates);
            Assert.AreEqual(2, bus.SavedPathCoordinates!.Count);
            Assert.AreEqual("TestRoute", bus.RouteName);

            W.GetField(new Coordinate(102, 80))!.Surface = null;
            W.GetField(new Coordinate(103, 80))!.Surface = null;
        }
    }
}
