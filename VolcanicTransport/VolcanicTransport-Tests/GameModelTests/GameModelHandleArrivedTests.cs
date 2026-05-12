using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using static VolcanicTransport.Model.World.Roadnetwork.Vehicle;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.GameModelTests
{
    [TestClass]
    [DoNotParallelize]
    public class GameModelHandleArrivedTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 66);

        private static City MakeCity(string name, int x, int y)
            => new(name, new Coordinate(x, y), [new Product(ProductType.HUMAN, 0, 100, 5)]);

        private static ConcreteFactory MakeConcrete(string name, int x, int y)
            => new(name, new Coordinate(x, y));

        [TestMethod]
        public void HandleVehicleArrived_Bus_WithPassengers_AddsOrKeepsMoney()
        {
            var city = MakeCity("City", 1, 1);
            var station = new CityStation(city, new Coordinate(1, 1), "CityStop");
            var bus = new Bus("ArrBus");
            bus.Load(10, ProductType.HUMAN);

            double before = Model.PlayerMoney;
            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(bus, station));

            Assert.IsTrue(Model.PlayerMoney >= before);
        }

        [TestMethod]
        public void HandleVehicleArrived_Bus_EmptyVehicle_DoesNotThrow()
        {
            var city = MakeCity("Herculaneum", 2, 2);
            var station = new CityStation(city, new Coordinate(2, 2), "HStop");
            station.GetWaitingPassengers(50);

            var bus = new Bus("EmptyBus");
            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(bus, station));
        }

        [TestMethod]
        public void HandleVehicleArrived_Bus_FiresVehicleArrivedAtStation()
        {
            var city = MakeCity("Stabiae", 3, 3);
            var station = new CityStation(city, new Coordinate(3, 3), "SStop");
            var bus = new Bus("EventBus");

            bool fired = false;
            void h(object? _, VehicleArrivedEventArgs __) => fired = true;
            Model.VehicleArrivedAtStation += h;
            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(bus, station));
            Model.VehicleArrivedAtStation -= h;

            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void HandleVehicleArrived_MiniBus_WorksSameAsBus()
        {
            var city = MakeCity("Oplontis", 4, 4);
            var station = new CityStation(city, new Coordinate(4, 4), "OStop");
            var minibus = new MiniBus("ArrMiniBus");
            minibus.Load(3, ProductType.HUMAN);

            double before = Model.PlayerMoney;
            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(minibus, station));
            Assert.IsTrue(Model.PlayerMoney >= before);
        }


        [TestMethod]
        public void HandleVehicleArrived_CargoTruck_WithASH_DoesNotThrow()
        {
            var factory = MakeConcrete("ConcreteF1", 5, 5);
            var station = new FactoryStation(new Coordinate(5, 5), "CStop", factory);
            var truck = new CargoTruck("AshTruck");
            truck.Load(5, ProductType.ASH);

            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(truck, station));
        }

        [TestMethod]
        public void HandleVehicleArrived_CargoTruck_NoLoad_MoneyUnchanged()
        {
            var factory = MakeConcrete("ConcreteF2", 6, 6);
            var station = new FactoryStation(new Coordinate(6, 6), "CStop2", factory);
            var truck = new CargoTruck("EmptyTruck");

            double before = Model.PlayerMoney;
            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(truck, station));
            Assert.AreEqual(before, Model.PlayerMoney, 0.001);
        }

        [TestMethod]
        public void HandleVehicleArrived_TankerTruck_WrongProduct_MoneyUnchanged()
        {
            var factory = MakeConcrete("ConcreteF3", 7, 7);
            var station = new FactoryStation(new Coordinate(7, 7), "CStop3", factory);
            var tanker = new TankerTruck("SteamTanker");
            tanker.Load(5, ProductType.STEAM);

            double before = Model.PlayerMoney;
            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(tanker, station));
            Assert.AreEqual(before, Model.PlayerMoney, 0.001);
        }

        [TestMethod]
        public void HandleVehicleArrived_CargoTruck_CityStation_UnloadsWhenNeeded()
        {
            var city = new City("AshCity", new Coordinate(8, 8), [new Product(ProductType.ASH, 0, 100, 5)]);
            var station = new CityStation(city, new Coordinate(8, 8), "AshStop");
            var truck = new CargoTruck("AshDelivery");
            truck.Load(5, ProductType.ASH);

            Model.HandleVehicleArrived(null, new VehicleArrivedEventArgs(truck, station));
        }
    }
}
