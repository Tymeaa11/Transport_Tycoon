using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_Tests.VehicleTests
{
    [TestClass]
    public class VehicleCargoTests
    {

        [TestMethod]
        public void Load_EmptyVehicle_SetsTypeAndLoad()
        {
            var bus = new Bus("B1");

            int taken = bus.Load(10, ProductType.HUMAN);

            Assert.AreEqual(10, taken);
            Assert.AreEqual(10, bus.CurrentLoad);
            Assert.AreEqual(ProductType.HUMAN, bus.CurrentType);
        }

        [TestMethod]
        public void Load_MoreThanCapacity_ClampsToCapacity()
        {
            var bus = new Bus("B2");

            int taken = bus.Load(999, ProductType.HUMAN);

            Assert.AreEqual(bus.Capacity, taken);
            Assert.AreEqual(bus.Capacity, bus.CurrentLoad);
        }

        [TestMethod]
        public void Load_VehicleAlreadyFull_ReturnsZero()
        {
            var bus = new Bus("B3");
            bus.Load(bus.Capacity, ProductType.HUMAN);

            int taken = bus.Load(5, ProductType.HUMAN);

            Assert.AreEqual(0, taken);
            Assert.AreEqual(bus.Capacity, bus.CurrentLoad);
        }

        [TestMethod]
        public void Load_ZeroAmount_DoesNotChangeState()
        {
            var bus = new Bus("B4");

            int taken = bus.Load(0, ProductType.HUMAN);

            Assert.AreEqual(0, taken);
            Assert.AreEqual(0, bus.CurrentLoad);
            Assert.AreEqual(ProductType.NONE, bus.CurrentType);
        }

        [TestMethod]
        public void Load_TwoCalls_AccumulatesLoad()
        {
            var bus = new Bus("B5");
            bus.Load(10, ProductType.HUMAN);

            bus.Load(5, ProductType.HUMAN);

            Assert.AreEqual(15, bus.CurrentLoad);
            Assert.AreEqual(ProductType.HUMAN, bus.CurrentType);
        }

        [TestMethod]
        public void Load_TypeSetOnFirstCall_NotResetOnSecond()
        {
            var bus = new Bus("B6");
            bus.Load(5, ProductType.HUMAN);

            bus.Load(3, ProductType.HUMAN);

            Assert.AreEqual(ProductType.HUMAN, bus.CurrentType);
        }

        [TestMethod]
        public void Unload_PartialAmount_DecreasesLoad()
        {
            var bus = new Bus("B7");
            bus.Load(30, ProductType.HUMAN);

            int unloaded = bus.Unload(10);

            Assert.AreEqual(10, unloaded);
            Assert.AreEqual(20, bus.CurrentLoad);
            Assert.AreEqual(ProductType.HUMAN, bus.CurrentType);
        }

        [TestMethod]
        public void Unload_AllPassengers_ResetsTypeToNone()
        {
            var bus = new Bus("B8");
            bus.Load(20, ProductType.HUMAN);

            bus.Unload(20);

            Assert.AreEqual(0, bus.CurrentLoad);
            Assert.AreEqual(ProductType.NONE, bus.CurrentType);
        }

        [TestMethod]
        public void Unload_MoreThanAvailable_ReturnsActualAmount()
        {
            var bus = new Bus("B9");
            bus.Load(10, ProductType.HUMAN);

            int unloaded = bus.Unload(999);

            Assert.AreEqual(10, unloaded);
            Assert.AreEqual(0, bus.CurrentLoad);
        }

        [TestMethod]
        public void Unload_EmptyVehicle_ReturnsZero()
        {
            var bus = new Bus("B10");

            int unloaded = bus.Unload(5);

            Assert.AreEqual(0, unloaded);
            Assert.AreEqual(ProductType.NONE, bus.CurrentType);
        }


        [TestMethod]
        public void AllType_Bus_ContainsOnlyHuman()
        {
            var bus = new Bus("B11");

            CollectionAssert.AreEquivalent(new[] { ProductType.HUMAN }, bus.AllType);
        }

        [TestMethod]
        public void AllType_TankerTruck_ContainsSteamAndWater()
        {
            var tanker = new TankerTruck("T1");

            Assert.IsTrue(tanker.AllType.Contains(ProductType.STEAM));
            Assert.IsTrue(tanker.AllType.Contains(ProductType.WATER));
        }

        [TestMethod]
        public void AllType_CargoTruck_DoesNotContainHuman()
        {
            var cargo = new CargoTruck("C1");

            CollectionAssert.DoesNotContain(cargo.AllType, ProductType.HUMAN);
        }

        [TestMethod]
        public void Capacity_TankerTruck_GreaterThanBus()
        {
            var bus = new Bus("B12");
            var tanker = new TankerTruck("T2");

            Assert.IsTrue(tanker.Capacity > bus.Capacity,
                "A tartálykocsi kapacitása nagyobb kell legyen mint a buszé.");
        }

        [TestMethod]
        public void Name_IsSetCorrectly()
        {
            var bus = new Bus("Sopron 47");

            Assert.AreEqual("Sopron 47", bus.Name);
        }

        [TestMethod]
        public void InitialState_IsWaiting()
        {
            var bus = new Bus("B13");

            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void AssignNewRoute_WhileWaiting_SetsRoute()
        {
            var bus = new Bus("B14");
            var route = new Route();

            bus.AssignNewRoute(route);

            Assert.AreEqual(route, bus.Route);
            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }

        [TestMethod]
        public void AssignNewRoute_WhileWaiting_RouteChangedEventFires()
        {
            var bus = new Bus("B15");
            bool fired = false;
            bus.RouteChanged += (_, _) => fired = true;

            bus.AssignNewRoute(new Route());

            Assert.IsTrue(fired, "RouteChanged esemény nem váltódott ki.");
        }

        [TestMethod]
        public void TryStartNextRoute_NoRoute_StaysWaiting()
        {
            var bus = new Bus("B16");

            bus.TryStartNextRoute();

            Assert.AreEqual(VehicleState.Waiting, bus.State);
        }
    }
}
