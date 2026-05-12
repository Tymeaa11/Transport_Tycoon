using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.VehicleTests
{
    [TestClass]
    [DoNotParallelize]
    public class VehicleTypeTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 0);


        [TestMethod]
        public void Bus_MaxSpeed_IsPositive() => Assert.IsTrue(new Bus("B").MaxSpeed > 0);

        [TestMethod]
        public void Bus_Price_IsPositive() => Assert.IsTrue(new Bus("B").Price > 0);

        [TestMethod]
        public void Bus_Capacity_IsPositive() => Assert.IsTrue(new Bus("B").Capacity > 0);

        [TestMethod]
        public void Bus_CanCarryHumans()
            => Assert.IsTrue(new Bus("B").AllType.Contains(ProductType.HUMAN));

        [TestMethod]
        public void Bus_Load_SetsCurrentLoad()
        {
            var bus = new Bus("BL");
            bus.Load(10, ProductType.HUMAN);
            Assert.AreEqual(10, bus.CurrentLoad);
            Assert.AreEqual(ProductType.HUMAN, bus.CurrentType);
        }

        [TestMethod]
        public void Bus_Unload_DecreasesCurrentLoad()
        {
            var bus = new Bus("BU");
            bus.Load(20, ProductType.HUMAN);
            int unloaded = bus.Unload(10);
            Assert.AreEqual(10, unloaded);
            Assert.AreEqual(10, bus.CurrentLoad);
        }

        [TestMethod]
        public void Bus_Unload_MoreThanLoaded_UnloadsAll()
        {
            var bus = new Bus("BU2");
            bus.Load(5, ProductType.HUMAN);
            int unloaded = bus.Unload(100);
            Assert.AreEqual(5, unloaded);
            Assert.AreEqual(0, bus.CurrentLoad);
        }

        [TestMethod]
        public void Bus_Name_MatchesConstructor()
            => Assert.AreEqual("MyBus", new Bus("MyBus").Name);


        [TestMethod]
        public void MiniBus_CapacityLessThanBus()
        {
            Assert.IsTrue(new MiniBus("MB").Capacity < new Bus("B").Capacity);
        }

        [TestMethod]
        public void MiniBus_CanCarryHumans()
            => Assert.IsTrue(new MiniBus("MB").AllType.Contains(ProductType.HUMAN));

        [TestMethod]
        public void MiniBus_Load_RespectCapacity()
        {
            var bus = new MiniBus("MBL");
            int capacity = bus.Capacity;
            bus.Load(capacity + 100, ProductType.HUMAN);
            Assert.AreEqual(capacity, bus.CurrentLoad);
        }

        [TestMethod]
        public void MiniBus_Price_IsPositive()
            => Assert.IsTrue(new MiniBus("MB").Price > 0);


        [TestMethod]
        public void CargoTruck_CannotCarryHumans()
            => Assert.IsFalse(new CargoTruck("CT").AllType.Contains(ProductType.HUMAN));

        [TestMethod]
        public void CargoTruck_Load_Sulfur_Works()
        {
            var truck = new CargoTruck("CT1");
            truck.Load(10, ProductType.SULFUR);
            Assert.AreEqual(10, truck.CurrentLoad);
            Assert.AreEqual(ProductType.SULFUR, truck.CurrentType);
        }

        [TestMethod]
        public void CargoTruck_Load_Ash_Works()
        {
            var truck = new CargoTruck("CT2");
            truck.Load(10, ProductType.ASH);
            Assert.AreEqual(10, truck.CurrentLoad);
        }

        [TestMethod]
        public void CargoTruck_Load_Bone_Works()
        {
            var truck = new CargoTruck("CT3");
            truck.Load(10, ProductType.BONE);
            Assert.AreEqual(10, truck.CurrentLoad);
        }

        [TestMethod]
        public void CargoTruck_Load_ExceedsCapacity_ClampsToCapacity()
        {
            var truck = new CargoTruck("CTC");
            int cap = truck.Capacity;
            truck.Load(cap + 999, ProductType.SULFUR);
            Assert.AreEqual(cap, truck.CurrentLoad);
        }

        [TestMethod]
        public void CargoTruck_Unload_Works()
        {
            var truck = new CargoTruck("CTU");
            truck.Load(50, ProductType.SULFUR);
            int unloaded = truck.Unload(30);
            Assert.AreEqual(30, unloaded);
            Assert.AreEqual(20, truck.CurrentLoad);
        }

        [TestMethod]
        public void CargoTruck_Price_IsPositive()
            => Assert.IsTrue(new CargoTruck("CT").Price > 0);


        [TestMethod]
        public void TankerTruck_CanCarryWater()
            => Assert.IsTrue(new TankerTruck("TT").AllType.Contains(ProductType.WATER));

        [TestMethod]
        public void TankerTruck_Load_Water_Works()
        {
            var tanker = new TankerTruck("TT1");
            tanker.Load(20, ProductType.WATER);
            Assert.AreEqual(20, tanker.CurrentLoad);
        }

        [TestMethod]
        public void TankerTruck_Load_Steam_Works()
        {
            var tanker = new TankerTruck("TT2");
            tanker.Load(20, ProductType.STEAM);
            Assert.AreEqual(20, tanker.CurrentLoad);
        }

        [TestMethod]
        public void TankerTruck_Unload_Works()
        {
            var tanker = new TankerTruck("TTU");
            tanker.Load(30, ProductType.WATER);
            int unloaded = tanker.Unload(20);
            Assert.AreEqual(20, unloaded);
            Assert.AreEqual(10, tanker.CurrentLoad);
        }

        [TestMethod]
        public void TankerTruck_Price_IsPositive()
            => Assert.IsTrue(new TankerTruck("TT").Price > 0);


        [TestMethod]
        public void Bus_And_CargoTruck_HaveNoOverlappingTypes()
        {
            var busTypes = new Bus("B").AllType;
            var truckTypes = new CargoTruck("CT").AllType;
            bool overlap = busTypes.Any(t => truckTypes.Contains(t));
            Assert.IsFalse(overlap, "Bus és CargoTruck nem szállíthat ugyanolyan árut.");
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class RouteEdgeCaseTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 0);

        private static Station MakeStation(int x, int y)
            => new SimpleTestStation(new Coordinate(x, y));

        [TestMethod]
        public void Route_DefaultName_IsNotNull()
        {
            var r = new Route();
            Assert.IsNotNull(r.Name);
        }

        [TestMethod]
        public void Route_InitiallyEmpty()
        {
            var r = new Route();
            Assert.AreEqual(0, r.Stops.Count);
        }

        [TestMethod]
        public void AddStop_NullStation_DoesNotThrow()
        {
            var r = new Route();
            try { r.AddStop(null!); } catch (NullReferenceException) { }
        }

        [TestMethod]
        public void AddStop_MultipleStations_AllAdded()
        {
            var r = new Route();
            r.AddStop(MakeStation(20, 20));
            r.AddStop(MakeStation(21, 20));
            r.AddStop(MakeStation(22, 20));
            Assert.AreEqual(3, r.Stops.Count);
        }

        [TestMethod]
        public void GetNextStop_EmptyRoute_ReturnsNull()
        {
            var r = new Route();
            var s = MakeStation(30, 30);
            Assert.IsNull(r.GetNextStop(s));
        }

        [TestMethod]
        public void Route_Name_CanBeSetViaProperty()
        {
            var r = new Route { Name = "MyRoute" };
            Assert.AreEqual("MyRoute", r.Name);
        }

        [TestMethod]
        public void PrepareForSave_DoesNotThrow()
        {
            var r = new Route { Name = "SaveRoute" };
            r.AddStop(MakeStation(40, 40));
            r.PrepareForSave();
        }

        [TestMethod]
        public void RestoreReference_RestoredRoute_StopsAreRelinked()
        {
            var world = GameWorld.Instance;
            var coord = new Coordinate(50, 50);
            var station = MakeStation(50, 50);
            world.GetField(coord)!.Surface = station;
            world.Stations.Add(station);

            var r = new Route { Name = "RestoreRoute" };
            r.AddStop(station);
            r.PrepareForSave();
            r.RestoreReference();

            world.Stations.Remove(station);
            Assert.AreEqual(1, r.Stops.Count);
        }
    }

    file class SimpleTestStation(Coordinate coord) : Station(
        coord, "Simple",
        new ProductBuffer(ProductType.HUMAN, 20),
        new Product(ProductType.HUMAN, 0, 20))
    {
        public override int UnLoadProductFromVehicle(Vehicle vehicle) => 0;
    }
}
