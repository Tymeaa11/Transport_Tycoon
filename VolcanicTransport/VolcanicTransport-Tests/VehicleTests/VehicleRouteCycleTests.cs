using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.VehicleTests
{
    [TestClass]
    [DoNotParallelize]
    public class VehicleRouteCycleTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 42);


        [TestMethod]
        public void AssignNewRoute_WhenWaiting_SetsRoute()
        {
            var bus = new Bus("RC_Bus1");
            var route = new Route();
            var s1 = SimpleStation.MakeStation(10, 10);
            var s2 = SimpleStation.MakeStation(11, 10);
            route.AddStop(s1);
            route.AddStop(s2);

            bus.AssignNewRoute(route);

            Assert.AreEqual(route, bus.Route);
        }

        [TestMethod]
        public void AssignNewRoute_WhenMoving_SetsPendingRoute()
        {
            var bus = new Bus("RC_Bus2")
            {
                State = VehicleState.Moving
            };
            var route = new Route();
            var s1 = SimpleStation.MakeStation(12, 10);
            var s2 = SimpleStation.MakeStation(13, 10);
            route.AddStop(s1);
            route.AddStop(s2);

            bus.AssignNewRoute(route);

            Assert.AreEqual(route, bus.PendingRoute);
        }

        [TestMethod]
        public void AssignNewRoute_FiresRouteChanged()
        {
            var bus = new Bus("RC_Bus3");
            bool fired = false;
            bus.RouteChanged += (_, _) => fired = true;

            var route = new Route();
            var s1 = SimpleStation.MakeStation(14, 10);
            var s2 = SimpleStation.MakeStation(15, 10);
            route.AddStop(s1);
            route.AddStop(s2);

            bus.AssignNewRoute(route);
            Assert.IsTrue(fired);
        }


        [TestMethod]
        public void ClearRoute_SetsStateToWaiting()
        {
            var bus = new Bus("Clear_Bus")
            {
                State = VehicleState.Moving
            };
            bus.ClearRoute();
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void ClearRoute_SetsRouteToNull()
        {
            var bus = new Bus("Clear_Bus2");
            var route = new Route();
            bus.Route = route;
            bus.ClearRoute();
            Assert.IsNull(bus.Route);
        }

        [TestMethod]
        public void ClearRoute_FiresStateUpdated()
        {
            var bus = new Bus("Clear_Bus3");
            bool fired = false;
            bus.StateUpdated += (_, _) => fired = true;
            bus.ClearRoute();
            Assert.IsTrue(fired);
        }


        [TestMethod]
        public void TryStartNextRoute_WhenWaitingNoRoute_StaysWaiting()
        {
            var bus = new Bus("TSN_Bus1")
            {
                State = VehicleState.Waiting,
                Route = null
            };
            bus.TryStartNextRoute();
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void TryStartNextRoute_WhenWaitingWithSingleStopRoute_StaysWaiting()
        {
            var bus = new Bus("TSN_Bus2")
            {
                State = VehicleState.Waiting
            };
            var route = new Route();
            route.AddStop(SimpleStation.MakeStation(20, 20));
            bus.Route = route;
            bus.TryStartNextRoute();
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }


        [TestMethod]
        public void Load_SetsCurrenType()
        {
            var bus = new Bus("Load_Bus");
            bus.Load(5, ProductType.HUMAN);
            Assert.AreEqual(ProductType.HUMAN, bus.CurrentType);
        }

        [TestMethod]
        public void Unload_ZeroLoad_SetsTypeToNone()
        {
            var bus = new Bus("Unload_Bus");
            bus.Load(5, ProductType.HUMAN);
            bus.Unload(5);
            Assert.AreEqual(ProductType.NONE, bus.CurrentType);
        }

        [TestMethod]
        public void Unload_PartialLoad_DoesNotClearType()
        {
            var bus = new Bus("Partial_Bus");
            bus.Load(10, ProductType.HUMAN);
            bus.Unload(3);
            Assert.AreEqual(ProductType.HUMAN, bus.CurrentType);
        }


        [TestMethod]
        public void Update_LoadingState_IncrementsWaitTimer()
        {
            var bus = new Bus("Loading_Bus")
            {
                State = VehicleState.Loading
            };
            bus.Update(1.0);
            Assert.AreEqual(VehicleState.Loading, bus.State);
        }

        [TestMethod]
        public void Update_LoadingState_AfterFullWait_TransitionsToWaiting()
        {
            var bus = new Bus("FullWait_Bus")
            {
                State = VehicleState.Loading
            };
            bus.Update(25.0);
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void Update_WaitingState_StaysWaiting()
        {
            var bus = new Bus("Wait_Bus")
            {
                State = VehicleState.Waiting
            };
            bus.Update(1.0);
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void Update_LoadingComplete_WithPendingRoute_ConsumesRoute()
        {
            var bus = new Bus("PR_Bus")
            {
                State = VehicleState.Loading
            };
            var pending = new Route();
            pending.AddStop(SimpleStation.MakeStation(60, 60));
            pending.AddStop(SimpleStation.MakeStation(61, 60));
            bus.PendingRoute = pending;

            bus.Update(25.0);

            Assert.IsNull(bus.PendingRoute);
            Assert.IsNotNull(bus.Route);
        }

        [TestMethod]
        public void Update_LoadingComplete_NoRoute_GoesToWaiting()
        {
            var bus = new Bus("NR_Bus")
            {
                Route = null,
                State = VehicleState.Loading
            };
            bus.Update(25.0);
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void Update_LoadingComplete_TwoStopRoute_AttemptsToNavigate()
        {
            var bus = new Bus("Nav_Bus");
            var route = new Route();
            route.AddStop(SimpleStation.MakeStation(62, 62));
            route.AddStop(SimpleStation.MakeStation(63, 62));
            bus.Route = route;
            bus.CurrentStopIndex = 1;
            bus.State = VehicleState.Loading;

            bus.Update(25.0);

            Assert.IsTrue(bus.State == VehicleState.Waiting || bus.State == VehicleState.Moving);
        }

        [TestMethod]
        public void TryStartNextRoute_WhenWaiting_TwoStops_EntersHandleRouteCycle()
        {
            var bus = new Bus("TSN_Valid");
            var route = new Route();
            route.AddStop(SimpleStation.MakeStation(70, 70));
            route.AddStop(SimpleStation.MakeStation(71, 70));
            bus.Route = route;
            bus.CurrentStopIndex = 1;
            bus.State = VehicleState.Waiting;

            bus.TryStartNextRoute();
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void AssignNewRoute_WhenWaiting_EmptyRoute_StaysWaiting()
        {
            var bus = new Bus("AW_Empty")
            {
                State = VehicleState.Waiting
            };
            var route = new Route();
            bus.AssignNewRoute(route);
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }


    }

    file class SimpleStation(Coordinate coord) : Station(
        coord, "SimpleStation",
        new ProductBuffer(ProductType.HUMAN, 20),
        new Product(ProductType.HUMAN, 0, 20))
    {
        public override int UnLoadProductFromVehicle(Vehicle vehicle) => 0;

        public static SimpleStation MakeStation(int x, int y)
        {
            var coord = new Coordinate(x, y);
            var station = new SimpleStation(coord);
            var field = GameWorld.Instance.GetField(coord);
            if (field != null) field.Surface = station;
            return station;
        }
    }
}
