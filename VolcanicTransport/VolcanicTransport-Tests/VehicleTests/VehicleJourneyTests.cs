using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.VehicleTests
{
    file class TestStation(Coordinate coord) : Station(
        coord, "TestStation",
        new ProductBuffer(ProductType.HUMAN, 50),
        new Product(ProductType.HUMAN, 0, 50))
    {
        public override int UnLoadProductFromVehicle(Vehicle vehicle) => 0;

        public static TestStation MakeStation(int x, int y)
            => new(new Coordinate(x, y));

        public static TestStation PlaceStation(int x, int y)
        {
            var coord = new Coordinate(x, y);
            var station = new TestStation(coord);
            GameWorld.Instance.GetField(coord)!.Surface = station;
            return station;
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class RouteTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 0);



        [TestMethod]
        public void AddStop_NewStation_IsAdded()
        {
            var route = new Route();
            var s = TestStation.MakeStation(1, 1);

            route.AddStop(s);

            Assert.AreEqual(1, route.Stops.Count);
        }

        [TestMethod]
        public void AddStop_SameStationTwice_CountStaysOne()
        {
            var route = new Route();
            var s = TestStation.MakeStation(1, 2);

            route.AddStop(s);
            route.AddStop(s);

            Assert.AreEqual(1, route.Stops.Count);
        }

        [TestMethod]
        public void GetNextStop_SingleStop_ReturnsNull()
        {
            var route = new Route();
            var s = TestStation.MakeStation(1, 3);
            route.AddStop(s);

            Assert.IsNull(route.GetNextStop(s));
        }

        [TestMethod]
        public void GetNextStop_TwoStops_WrapsAround()
        {
            var route = new Route();
            var s1 = TestStation.MakeStation(1, 4);
            var s2 = TestStation.MakeStation(1, 5);
            route.AddStop(s1);
            route.AddStop(s2);

            Assert.AreEqual(s2, route.GetNextStop(s1));
            Assert.AreEqual(s1, route.GetNextStop(s2));
        }

        [TestMethod]
        public void GetNextStop_UnknownStation_ReturnsFirst()
        {
            var route = new Route();
            var s1 = TestStation.MakeStation(1, 6);
            var s2 = TestStation.MakeStation(1, 7);
            var sOther = TestStation.MakeStation(1, 8);
            route.AddStop(s1);
            route.AddStop(s2);

            Assert.AreEqual(s1, route.GetNextStop(sOther));
        }

        [TestMethod]
        public void GetNextStop_ThreeStops_CyclesCorrectly()
        {
            var route = new Route();
            var s1 = TestStation.MakeStation(1, 9);
            var s2 = TestStation.MakeStation(1, 10);
            var s3 = TestStation.MakeStation(1, 11);
            route.AddStop(s1);
            route.AddStop(s2);
            route.AddStop(s3);

            Assert.AreEqual(s2, route.GetNextStop(s1));
            Assert.AreEqual(s3, route.GetNextStop(s2));
            Assert.AreEqual(s1, route.GetNextStop(s3));
        }
    }


    [TestClass]
    [DoNotParallelize]
    public class VehicleJourneyTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 0);

        [TestInitialize]
        public void ResetGraph() => GameWorld.Instance.Roadnetwork.NodeMap.Clear();

        private static Road PlaceRoad(int x, int y)
        {
            var coord = new Coordinate(x, y);
            var road = new Road(coord);
            GameWorld.Instance.GetField(coord)!.Surface = road;
            return road;
        }



        private static IReadOnlyList<Vehicle> VehiclesAt(int x, int y)
            => GameWorld.Instance.VehicleManager.GetVehiclesOnField(new Coordinate(x, y));

        [TestMethod]
        public void StartJourney_EmptyPath_DoesNotSetCurrentRoad()
        {
            var bus = new Bus("SJ0");

            bus.StartJourney([]);

            Assert.IsNull(bus.CurrentRoad);
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void StartJourney_ValidPath_SetsCurrentRoadToFirst()
        {
            var r1 = PlaceRoad(10, 10);
            var r2 = PlaceRoad(11, 10);
            var bus = new Bus("SJ1");

            bus.StartJourney([r1, r2]);

            Assert.AreEqual(r1, bus.CurrentRoad);
        }

        [TestMethod]
        public void StartJourney_ValidPath_SetsStateToMoving()
        {
            var r1 = PlaceRoad(12, 10);
            var r2 = PlaceRoad(13, 10);
            var bus = new Bus("SJ2");

            bus.StartJourney([r1, r2]);

            Assert.AreEqual(VehicleState.Moving, bus.State);
        }

        [TestMethod]
        public void StartJourney_ValidPath_RegistersVehicleOnFirstRoad()
        {
            var r1 = PlaceRoad(14, 10);
            var r2 = PlaceRoad(15, 10);
            var bus = new Bus("SJ3");

            bus.StartJourney([r1, r2]);

            CollectionAssert.Contains(
                (System.Collections.ICollection)VehiclesAt(14, 10), bus);
        }

        [TestMethod]
        public void StartJourney_StationAsFirstRoad_MarksStationOccupied()
        {
            var st = TestStation.PlaceStation(16, 10);
            var r2  = PlaceRoad(17, 10);
            var bus = new Bus("SJ4");

            bus.StartJourney([st, r2]);

            Assert.IsTrue(st.IsOccupied);
        }

        [TestMethod]
        public void StartJourney_CalledTwice_UnregistersFromPreviousRoad()
        {
            var r1 = PlaceRoad(18, 10);
            var r2 = PlaceRoad(19, 10);
            var r3 = PlaceRoad(18, 11);
            var bus = new Bus("SJ5");

            bus.StartJourney([r1, r2]);
            bus.StartJourney([r3, r2]);

            Assert.AreEqual(0, VehiclesAt(18, 10).Count,
                "A jármű ne maradjon regisztrálva az előző koordinátán.");
        }

        [TestMethod]
        public void ClearRoute_SetsStateToWaiting()
        {
            var r1 = PlaceRoad(20, 10);
            var r2 = PlaceRoad(21, 10);
            var bus = new Bus("CR1");
            bus.StartJourney([r1, r2]);
            bus.Route = new Route();

            bus.ClearRoute();

            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void ClearRoute_NullsRouteReference()
        {
            var r1 = PlaceRoad(22, 10);
            var r2 = PlaceRoad(23, 10);
            var bus = new Bus("CR2");
            bus.StartJourney([r1, r2]);
            bus.Route = new Route();

            bus.ClearRoute();

            Assert.IsNull(bus.Route);
        }

        [TestMethod]
        public void ClearRoute_UnregistersFromVehicleManager()
        {
            var r1 = PlaceRoad(24, 10);
            var r2 = PlaceRoad(25, 10);
            var bus = new Bus("CR3");
            bus.StartJourney([r1, r2]);

            bus.ClearRoute();

            Assert.AreEqual(0, VehiclesAt(24, 10).Count);
        }

        [TestMethod]
        public void AssignNewRoute_WhileMoving_StoresAsPendingRoute()
        {
            var r1 = PlaceRoad(30, 10);
            var r2 = PlaceRoad(31, 10);
            var bus = new Bus("AR1");
            bus.StartJourney([r1, r2]);
            var newRoute = new Route();

            bus.AssignNewRoute(newRoute);

            Assert.AreEqual(newRoute, bus.PendingRoute);
            Assert.IsNull(bus.Route,
                "Mozgó jármű esetén az útvonal nem váltódhat azonnal.");
        }

        [TestMethod]
        public void AssignNewRoute_WhileMoving_RouteChangedEventFires()
        {
            var r1 = PlaceRoad(32, 10);
            var r2 = PlaceRoad(33, 10);
            var bus = new Bus("AR2");
            bus.StartJourney([r1, r2]);
            bool fired = false;
            bus.RouteChanged += (_, _) => fired = true;

            bus.AssignNewRoute(new Route());

            Assert.IsTrue(fired, "RouteChanged esemény nem váltódott ki mozgó jármű esetén.");
        }

        [TestMethod]
        public void AssignNewRoute_WhileWaiting_SetsCurrentRoute()
        {
            var bus = new Bus("AR3");
            var route = new Route();

            bus.AssignNewRoute(route);

            Assert.AreEqual(route, bus.Route,
                "Várakozó jármű esetén az útvonal azonnal beállítódik.");
        }

        [TestMethod]
        public void TryStartNextRoute_WaitingWithSingleStop_StaysWaiting()
        {
            var bus = new Bus("TS1");
            var route = new Route();
            route.AddStop(TestStation.PlaceStation(40, 10));
            bus.Route = route;

            bus.TryStartNextRoute();

            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void TryStartNextRoute_WaitingWithNoRoute_StaysWaiting()
        {
            var bus = new Bus("TS2");

            bus.TryStartNextRoute();

            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void TryStartNextRoute_MovingState_DoesNothing()
        {
            var r1 = PlaceRoad(41, 10);
            var r2 = PlaceRoad(42, 10);
            var bus = new Bus("TS3");
            bus.StartJourney([r1, r2]);
            var route = new Route();
            route.AddStop(TestStation.PlaceStation(43, 10));
            route.AddStop(TestStation.PlaceStation(44, 10));
            bus.Route = route;

            bus.TryStartNextRoute();

            Assert.AreEqual(VehicleState.Moving, bus.State);
        }

        [TestMethod]
        public void Update_LoadingState_IncrementsWaitTimer()
        {
            var r1 = PlaceRoad(50, 10);
            var bus = new Bus("UP1");
            bus.StartJourney([r1]);
            bus.State = VehicleState.Loading;

            bus.Update(5.0);

            Assert.IsTrue(bus.WaitTimer > 0, "Loading állapotban a waitTimer nem nőtt.");
        }

        [TestMethod]
        public void Update_LoadingState_SetsSpeedToZero()
        {
            var r1 = PlaceRoad(51, 10);
            var bus = new Bus("UP2");
            bus.StartJourney([r1]);
            bus.State = VehicleState.Loading;

            bus.Update(1.0);

            Assert.AreEqual(0f, bus.CurrentSpeed);
        }

        [TestMethod]
        public void Update_MovingWithNoPath_SetsWaiting()
        {
            var bus = new Bus("UP3")
            {
                State = VehicleState.Moving
            };

            bus.Update(1.0);

            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void Update_LoadingTimerExceeds_MorePathAvailable_TransitionsToMoving()
        {
            var r1 = PlaceRoad(52, 10);
            var r2 = PlaceRoad(53, 10);
            var bus = new Bus("UP4");
            bus.StartJourney([r1, r2]);
            bus.State = VehicleState.Loading;

            bus.Update(21.0);

            Assert.AreEqual(VehicleState.Moving, bus.State);
        }

        [TestMethod]
        public void Update_LoadingTimerExceeds_AtPathEnd_NoRoute_SetsWaiting()
        {
            var r1 = PlaceRoad(54, 10);
            var bus = new Bus("UP5");
            bus.StartJourney([r1]);
            bus.State = VehicleState.Loading;

            bus.Update(21.0);

            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void Boarding_EmptyStation_ReturnsZero()
        {
            var station = TestStation.PlaceStation(60, 10);
            var bus = new Bus("BD1");

            int boarded = station.Boarding(bus);

            Assert.AreEqual(0, boarded);
        }

        [TestMethod]
        public void Boarding_StationWithPassengers_LoadsVehicle()
        {
            var station = TestStation.PlaceStation(61, 10);
            station.GetWaitingPassengers(200.0);
            var bus = new Bus("BD2");

            int boarded = station.Boarding(bus);

            Assert.IsTrue(boarded > 0, "Legalább 1 utasnak fel kell szállnia.");
            Assert.AreEqual(bus.CurrentLoad, boarded);
            Assert.AreEqual(ProductType.HUMAN, bus.CurrentType);
        }

        [TestMethod]
        public void Boarding_VehicleCarryingOtherProduct_ReturnsZero()
        {
            var station = TestStation.PlaceStation(62, 10);
            station.GetWaitingPassengers(200.0);
            var truck = new TankerTruck("BD3");
            truck.Load(100, ProductType.WATER);

            int boarded = station.Boarding(truck);

            Assert.AreEqual(0, boarded,
                "Nem HUMAN típusú rakományú jármű nem vehet fel utasokat.");
        }

        [TestMethod]
        public void Boarding_StationPassengersDecreaseAfterBoarding()
        {
            var station = TestStation.PlaceStation(63, 10);
            station.GetWaitingPassengers(200.0);
            var bus = new Bus("BD4");

            station.Boarding(bus);

            Assert.AreEqual(0, station.WaitingPassengers,
                "Felszállás után az állomáson nem maradhatnak utasok (busz kapacitása > várakozók).");
        }

        [TestMethod]
        public void UnBoarding_NonHumanVehicle_ReturnsZero()
        {
            var station = TestStation.PlaceStation(64, 10);
            var truck = new TankerTruck("BD5");
            truck.Load(200, ProductType.WATER);

            int unboarded = station.UnBoarding(truck);

            Assert.AreEqual(0, unboarded);
        }

        [TestMethod]
        public void UnBoarding_HumanVehicle_EventuallyReducesLoad()
        {
            var station = TestStation.PlaceStation(65, 10);
            var bus = new Bus("BD6");
            bus.Load(50, ProductType.HUMAN);

            int totalLeft = 0;
            for (int i = 0; i < 10; i++)
            {
                bus.Load(50 - bus.CurrentLoad, ProductType.HUMAN);
                totalLeft += station.UnBoarding(bus);
            }

            Assert.IsTrue(totalLeft > 0,
                $"10 próba alatt (ChanceToUnboard={GameSettings.ChanceToUnboard}) legalább 1 utasnak le kell szállnia.");
        }

        [TestMethod]
        public void UnBoarding_EmptyVehicle_ReturnsZero()
        {
            var station = TestStation.PlaceStation(66, 10);
            var bus = new Bus("BD7");
            bus.Load(10, ProductType.HUMAN);
            bus.Unload(10);

            int unboarded = station.UnBoarding(bus);

            Assert.AreEqual(0, unboarded);
        }


        [TestMethod]
        public void Update_LoadingTimerExceeds_SingleStopRoute_SetsWaiting()
        {
            var st = TestStation.PlaceStation(67, 10);
            var bus = new Bus("EV1");
            var route = new Route();
            route.AddStop(st);
            bus.Route = route;
            bus.StartJourney([st]);
            bus.State = VehicleState.Loading;

            bus.Update(21.0);

            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void Update_MovingVehicle_TransitionsToNextRoad()
        {
            var r1 = PlaceRoad(80, 10);
            var r2 = PlaceRoad(81, 10);
            var bus = new Bus("MV1");
            bus.StartJourney([r1, r2]);

            Assert.AreEqual(VehicleState.Moving, bus.State);

            for (int i = 0; i < 5; i++) bus.Update(1.0);

            Assert.AreEqual(r2, bus.CurrentRoad);
        }

        [TestMethod]
        public void Update_MovingVehicle_ReachesLoadingStateAtPathEnd()
        {
            var r1 = PlaceRoad(82, 10);
            var r2 = PlaceRoad(83, 10);
            var bus = new Bus("MV2");
            bus.StartJourney([r1, r2]);

            for (int i = 0; i < 15; i++) bus.Update(1.0);

            Assert.AreEqual(VehicleState.Loading, bus.State);
        }

        [TestMethod]
        public void Update_MovingVehicle_FiresStateUpdatedEvent()
        {
            var r1 = PlaceRoad(84, 10);
            var r2 = PlaceRoad(85, 10);
            var bus = new Bus("MV3");
            bus.StartJourney([r1, r2]);

            bool fired = false;
            bus.StateUpdated += (_, _) => fired = true;

            bus.Update(0.5);

            Assert.IsTrue(fired, "StateUpdated should fire during movement.");
        }

        [TestMethod]
        public void Update_MovingVehicle_PositionChanges()
        {
            var r1 = PlaceRoad(86, 10);
            var r2 = PlaceRoad(87, 10);
            var bus = new Bus("MV4");
            bus.StartJourney([r1, r2]);

            var startPos = bus.Position;

            bus.Update(2.0);

            Assert.AreNotEqual(startPos, bus.Position, "Position should change during movement.");
        }

        [TestMethod]
        public void Update_TwoVehiclesSamePath_SecondSlowsDown()
        {
            var r1 = PlaceRoad(88, 10);
            var r2 = PlaceRoad(89, 10);
            var r3 = PlaceRoad(90, 10);

            var bus1 = new Bus("TwoA");
            bus1.StartJourney([r1, r2, r3]);

            var bus2 = new Bus("TwoB");
            bus2.StartJourney([r1, r2, r3]);

            for (int i = 0; i < 4; i++) bus1.Update(1.0);

            bus2.Update(1.0);

            Assert.IsTrue(bus2.CurrentSpeed >= 0);
        }


        [TestMethod]
        public void PrepareForSave_EmptyPath_SavesEmptyList()
        {
            var bus = new Bus("PS0");

            bus.PrepareForSave();

            Assert.IsNotNull(bus.SavedPathCoordinates);
            Assert.AreEqual(0, bus.SavedPathCoordinates!.Count);
        }

        [TestMethod]
        public void PrepareForSave_WithPath_SavesAllCoordinates()
        {
            var r1 = PlaceRoad(92, 10);
            var r2 = PlaceRoad(93, 10);
            var bus = new Bus("PS1");
            bus.StartJourney([r1, r2]);

            bus.PrepareForSave();

            Assert.AreEqual(2, bus.SavedPathCoordinates!.Count);
            CollectionAssert.Contains(bus.SavedPathCoordinates, r1.Coordinate);
            CollectionAssert.Contains(bus.SavedPathCoordinates, r2.Coordinate);
        }

        [TestMethod]
        public void PrepareForSave_WithRoute_SavesRouteName()
        {
            var bus = new Bus("PS2")
            {
                Route = new Route { Name = "RouteX" }
            };

            bus.PrepareForSave();

            Assert.AreEqual("RouteX", bus.RouteName);
        }

        [TestMethod]
        public void PrepareForSave_NoRoute_NullRouteName()
        {
            var bus = new Bus("PS3");

            bus.PrepareForSave();

            Assert.IsNull(bus.RouteName);
        }

        [TestMethod]
        public void RestoreReference_WithSavedPath_RebuildsCurrent()
        {
            var r1 = PlaceRoad(94, 10);
            var r2 = PlaceRoad(95, 10);
            var bus = new Bus("RR1");
            bus.StartJourney([r1, r2]);
            bus.PrepareForSave();

            var bus2 = new Bus("RR1b")
            {
                SavedPathCoordinates = bus.SavedPathCoordinates
            };

            bus2.RestoreReference(r1.Coordinate);

            Assert.AreEqual(r1, bus2.CurrentRoad);
        }

        [TestMethod]
        public void RestoreReference_NoSavedPath_DoesNotThrow()
        {
            var bus = new Bus("RR2");
            bus.StartJourney([PlaceRoad(96, 10)]);

            bus.RestoreReference(new Coordinate(96, 10));
        }

        [TestMethod]
        public void TriggerRouteChanged_FiresEvent()
        {
            var bus = new Bus("TRC1");
            bool fired = false;
            bus.RouteChanged += (_, _) => fired = true;

            bus.TriggerRouteChanged();

            Assert.IsTrue(fired);
        }
    }
}
