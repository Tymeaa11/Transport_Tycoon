using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;
using GameWorld = VolcanicTransport.Model.World.World;


namespace VolcanicTransport_Tests.VehicleTests
{
    [TestClass]
    [DoNotParallelize]
    public class VehicleManagerTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 0);

        public static VehicleManager VManager => GameWorld.Instance.VehicleManager;


        [TestMethod]
        public void GetVehiclesOnField_UnregisteredCoord_ReturnsEmptyList()
        {
            var result = VManager.GetVehiclesOnField(new Coordinate(1, 1));

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetVehiclesOnField_AfterRegister_ContainsVehicle()
        {
            var bus = new Bus("VM1");
            var coord = new Coordinate(100, 100);

            VManager.RegisterVehicleOnField(bus, coord);

            CollectionAssert.Contains(
                (System.Collections.ICollection)VManager.GetVehiclesOnField(coord),
                bus);

            VManager.UnregisterVehicleFromField(bus, coord);
        }

        [TestMethod]
        public void UnregisterVehicle_AfterRegister_IsNoLongerPresent()
        {
            var bus = new Bus("VM2");
            var coord = new Coordinate(101, 100);

            VManager.RegisterVehicleOnField(bus, coord);
            VManager.UnregisterVehicleFromField(bus, coord);

            Assert.AreEqual(0, VManager.GetVehiclesOnField(coord).Count);
        }

        [TestMethod]
        public void UnregisterVehicle_WhenNeverRegistered_DoesNotThrow()
        {
            var bus = new Bus("VM3");
            var coord = new Coordinate(102, 100);

            VManager.UnregisterVehicleFromField(bus, coord);

            Assert.AreEqual(0, VManager.GetVehiclesOnField(coord).Count);
        }


        [TestMethod]
        public void RegisterVehicle_TwiceAtSameCoord_NoDuplicate()
        {
            var bus = new Bus("VM4");
            var coord = new Coordinate(103, 100);

            VManager.RegisterVehicleOnField(bus, coord);
            VManager.RegisterVehicleOnField(bus, coord);

            Assert.AreEqual(1, VManager.GetVehiclesOnField(coord).Count);

            VManager.UnregisterVehicleFromField(bus, coord);
        }


        [TestMethod]
        public void RegisterTwoVehicles_SameCoord_BothPresent()
        {
            var bus1 = new Bus("VM5a");
            var bus2 = new Bus("VM5b");
            var coord = new Coordinate(104, 100);

            VManager.RegisterVehicleOnField(bus1, coord);
            VManager.RegisterVehicleOnField(bus2, coord);

            var vehicles = VManager.GetVehiclesOnField(coord);
            Assert.AreEqual(2, vehicles.Count);
            CollectionAssert.Contains((System.Collections.ICollection)vehicles, bus1);
            CollectionAssert.Contains((System.Collections.ICollection)vehicles, bus2);

            VManager.UnregisterVehicleFromField(bus1, coord);
            VManager.UnregisterVehicleFromField(bus2, coord);
        }

        [TestMethod]
        public void UnregisterOneOfTwo_OtherStillPresent()
        {
            var bus1 = new Bus("VM6a");
            var bus2 = new Bus("VM6b");
            var coord = new Coordinate(105, 100);

            VManager.RegisterVehicleOnField(bus1, coord);
            VManager.RegisterVehicleOnField(bus2, coord);

            VManager.UnregisterVehicleFromField(bus1, coord);

            var vehicles = VManager.GetVehiclesOnField(coord);
            Assert.AreEqual(1, vehicles.Count);
            CollectionAssert.Contains((System.Collections.ICollection)vehicles, bus2);

            VManager.UnregisterVehicleFromField(bus2, coord);
        }
    }
}
