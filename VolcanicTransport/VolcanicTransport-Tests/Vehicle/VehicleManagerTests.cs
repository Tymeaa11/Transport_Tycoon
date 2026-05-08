using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_Tests.Roadnetwork
{
    [TestClass]
    [DoNotParallelize]
    public class VehicleManagerTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => World.Initialise(4, 0);


        [TestMethod]
        public void GetVehiclesOnField_UnregisteredCoord_ReturnsEmptyList()
        {
            var result = World.Instance.VehicleManager.GetVehiclesOnField(new Coordinate(1, 1));

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetVehiclesOnField_AfterRegister_ContainsVehicle()
        {
            var bus = new Bus("VM1");
            var coord = new Coordinate(100, 100);

            World.Instance.VehicleManager.RegisterVehicleOnField(bus, coord);

            CollectionAssert.Contains(
                (System.Collections.ICollection)World.Instance.VehicleManager.GetVehiclesOnField(coord),
                bus);

            World.Instance.VehicleManager.UnregisterVehicleFromField(bus, coord);
        }

        [TestMethod]
        public void UnregisterVehicle_AfterRegister_IsNoLongerPresent()
        {
            var bus = new Bus("VM2");
            var coord = new Coordinate(101, 100);

            World.Instance.VehicleManager.RegisterVehicleOnField(bus, coord);
            World.Instance.VehicleManager.UnregisterVehicleFromField(bus, coord);

            Assert.AreEqual(0, World.Instance.VehicleManager.GetVehiclesOnField(coord).Count);
        }

        [TestMethod]
        public void UnregisterVehicle_WhenNeverRegistered_DoesNotThrow()
        {
            var bus = new Bus("VM3");
            var coord = new Coordinate(102, 100);

            World.Instance.VehicleManager.UnregisterVehicleFromField(bus, coord);

            Assert.AreEqual(0, World.Instance.VehicleManager.GetVehiclesOnField(coord).Count);
        }


        [TestMethod]
        public void RegisterVehicle_TwiceAtSameCoord_NoDuplicate()
        {
            var bus = new Bus("VM4");
            var coord = new Coordinate(103, 100);

            World.Instance.VehicleManager.RegisterVehicleOnField(bus, coord);
            World.Instance.VehicleManager.RegisterVehicleOnField(bus, coord);

            Assert.AreEqual(1, World.Instance.VehicleManager.GetVehiclesOnField(coord).Count);

            World.Instance.VehicleManager.UnregisterVehicleFromField(bus, coord);
        }


        [TestMethod]
        public void RegisterTwoVehicles_SameCoord_BothPresent()
        {
            var bus1 = new Bus("VM5a");
            var bus2 = new Bus("VM5b");
            var coord = new Coordinate(104, 100);

            World.Instance.VehicleManager.RegisterVehicleOnField(bus1, coord);
            World.Instance.VehicleManager.RegisterVehicleOnField(bus2, coord);

            var vehicles = World.Instance.VehicleManager.GetVehiclesOnField(coord);
            Assert.AreEqual(2, vehicles.Count);
            CollectionAssert.Contains((System.Collections.ICollection)vehicles, bus1);
            CollectionAssert.Contains((System.Collections.ICollection)vehicles, bus2);

            World.Instance.VehicleManager.UnregisterVehicleFromField(bus1, coord);
            World.Instance.VehicleManager.UnregisterVehicleFromField(bus2, coord);
        }

        [TestMethod]
        public void UnregisterOneOfTwo_OtherStillPresent()
        {
            var bus1 = new Bus("VM6a");
            var bus2 = new Bus("VM6b");
            var coord = new Coordinate(105, 100);

            World.Instance.VehicleManager.RegisterVehicleOnField(bus1, coord);
            World.Instance.VehicleManager.RegisterVehicleOnField(bus2, coord);

            World.Instance.VehicleManager.UnregisterVehicleFromField(bus1, coord);

            var vehicles = World.Instance.VehicleManager.GetVehiclesOnField(coord);
            Assert.AreEqual(1, vehicles.Count);
            CollectionAssert.Contains((System.Collections.ICollection)vehicles, bus2);

            World.Instance.VehicleManager.UnregisterVehicleFromField(bus2, coord);
        }
    }
}
