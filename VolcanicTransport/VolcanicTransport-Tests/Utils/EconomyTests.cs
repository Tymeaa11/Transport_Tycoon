using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_Tests.Utils
{
    [TestClass]
    public class EconomyTests
    {
        private World _world;

        [TestInitialize]
        public void Setup()
        {
            World.Initialise(8, 123);
            _world = World.Instance;
        }

        #region ProductBuffer Tests
        [TestMethod]
        public void Buffer_AddAmount_ShouldNotExceedCapacity()
        {
            var buffer = new ProductBuffer(ProductType.ASH, 100, 90);
            int added = buffer.AddAmount(20);

            Assert.AreEqual(10, added, "Csak 10 egységet szabadott volna befogadnia.");
            Assert.AreEqual(100, buffer.CurrentLoad, "A raktárnak tele kellene lennie.");
        }
        #endregion

        #region Station & Transport Tests

        [TestMethod]
        public void FactoryStation_Unload_ShouldFillFactoryBaseBuffer()
        {
            // 1. Setup: Létrehozunk egy gyárat, ami ként (SULFUR) kér
            var origin = new Coordinate(5, 5);
            var factory = new SulfurProducer("TestSulfur", origin); // Feltételezzük, hogy létezik
            var station = new FactoryStation(origin, "FactoryStation", factory);

            // 2. Setup: Jármű kénnel tele (Kapacitás: 50, Rakomány: 50)
            var truck = new CargoTruck("TestTruck"); // Kamion, ami szállíthat ként
            truck.Load(50, ProductType.SULFUR);

            // 3. Action: Megérkezik a gyárhoz és lepakol
            int unloaded = station.UnLoadProductFromVehicle(truck);

            // 4. Assert
            Assert.IsTrue(unloaded > 0, "A gyárnak át kellett volna vennie a ként.");
            Assert.AreEqual(0, truck.CurrentLoad, "A kamionnak ki kellett volna ürülnie.");
            Assert.AreEqual(unloaded, factory.BaseProductBuffer.CurrentLoad, "Az árunak a gyár raktárába kellett kerülnie.");
        }

        [TestMethod]
        public void CityStation_Unload_OnlyAcceptedProducts()
        {
            // 1. Setup: Város, ami csak HAMUT (ASH) kér
            var city = new City("TestCity", new Coordinate(10, 10));
            city.ProductTypes.Add(ProductType.ASH);
            var station = new CityStation(city, new Coordinate(10, 10), "CityStation");

            // 2. Setup: Jármű vízzel (WATER) tele
            var tanker = new TankerTruck("TestTanker");
            tanker.Load(50, ProductType.WATER);

            // 3. Action: Megpróbál lepakolni a városban
            int unloaded = station.UnLoadProductFromVehicle(tanker);

            // 4. Assert
            Assert.AreEqual(0, unloaded, "A város nem fogadhatna el vizet, ha hamut kér.");
            Assert.AreEqual(50, tanker.CurrentLoad, "A járműben benne kellett volna maradnia az árunak.");
        }

        [TestMethod]
        public void Passenger_Boarding_ShouldRespectCapacity()
        {
            // 1. Setup: Állomás 50 várakozó utassal
            var city = new City("TestCity", new Coordinate(0, 0));
            var station = new CityStation(city, new Coordinate(0, 0), "Station");
            station.GetWaitingPassengers(1000); // Feltöltjük az akkumulátort, hogy legyenek utasok

            // 2. Setup: Kis busz (Kapacitás: 10)
            var miniBus = new MiniBus("SmallBus");

            // 3. Action: Felszállás
            int boarded = station.Boarding(miniBus);

            // 4. Assert
            Assert.AreEqual(10, boarded, "A minibuszra csak 10 ember férhet fel.");
            Assert.AreEqual(10, miniBus.CurrentLoad, "A busznak tele kellene lennie.");
            Assert.AreEqual(ProductType.HUMAN, miniBus.CurrentType);
        }

        #endregion
    }
}
