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
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.EconomyTests
{
    [TestClass]
    [DoNotParallelize]
    public class EconomyTests
    {
        public static GameWorld World => GameWorld.Instance;

        [TestInitialize]
        public void Setup()
        {
            GameWorld.Initialise(8, 123);
        }

        #region ProductBuffer Tests
        [TestMethod]
        public void Buffer_AddAmount_ShouldNotExceedCapacity()
        {
            int maxCapacity = 100;
            var buffer = new ProductBuffer(ProductType.ASH, maxCapacity, 90);
            int added = buffer.AddAmount(20);

            Assert.AreEqual(10, added, "Csak 10 egységet szabadott volna befogadnia.");
            Assert.AreEqual(100, buffer.CurrentLoad, "A raktárnak tele kellene lennie.");
        }
        [TestMethod]
        public void Buffer_DeductAmount_ShouldBeZero()
        {
            int currentLoad = 40;
            var buffer = new ProductBuffer(ProductType.ASH, 100, currentLoad);
            buffer.DeductAmount(50);

            Assert.AreEqual(0, buffer.CurrentLoad, "A raktárnak üresnek kellene lennie.");
        }
        [TestMethod]
        public void Buffer_ReciveProduct_ShouldAcceptProduct()
        {
            var buffer = new ProductBuffer(ProductType.ASH, 100, 50);
            var truck = new CargoTruck("TestTruck");
            truck.Load(30, ProductType.ASH);

            int received = buffer.ReceiveProduct(truck, 30);

            Assert.AreEqual(30, received, "A raktárnak át kellett volna vennie mind a 30 egységet.");
            Assert.AreEqual(80, buffer.CurrentLoad, "A raktár készletének 80-nak kellene lennie.");
            Assert.AreEqual(0, truck.CurrentLoad, "A teherautónak ki kellett volna ürülnie.");
        }

        [TestMethod]
        public void Buffer_ReciveProduct_ShouldReturnExcessToVehicle()
        {
            var buffer = new ProductBuffer(ProductType.SULFUR, 100, 90);
            var truck = new CargoTruck("TestTruck");
            truck.Load(50, ProductType.SULFUR);

            int received = buffer.ReceiveProduct(truck, 50);

            Assert.AreEqual(10, received, "Csak 10 egységet szabadott volna átvennie.");
            Assert.AreEqual(100, buffer.CurrentLoad, "A raktárnak meg kellene telnie.");
            Assert.AreEqual(40, truck.CurrentLoad, "A teherautónál ott kellene maradnia a maradék 40 egységnek.");
        }

        [TestMethod]
        public void Buffer_ReciveProduct_ShouldRejectEverything()
        {
            var buffer = new ProductBuffer(ProductType.ASH, 100, 0);
            var tanker = new TankerTruck("TestTanker");
            tanker.Load(50, ProductType.WATER);

            int received = buffer.ReceiveProduct(tanker, 50);

            Assert.AreEqual(0, received, "Nem szabadna átvenni eltérő típusú terméket.");
            Assert.AreEqual(50, tanker.CurrentLoad, "A jármű rakományának érintetlennek kell maradnia.");
        }

        [TestMethod]
        public void Buffer_FillVehicle_ShouldRespectVehicleCapacity()
        {
            var buffer = new ProductBuffer(ProductType.BONE, GameSettings.CargoTruckData.Capacity + 100, GameSettings.CargoTruckData.Capacity + 100);
            var truck = new CargoTruck("SmallTruck");

            int filled = buffer.FillVehicle(truck);

            Assert.AreEqual(GameSettings.CargoTruckData.Capacity, filled, "A teherautóra csak a kapacitásának megfelelő mennyiség kerülhetett.");
            Assert.AreEqual(100, buffer.CurrentLoad, "A raktárban 100 egységnek kellett maradnia.");
            Assert.AreEqual(GameSettings.CargoTruckData.Capacity, truck.CurrentLoad, "A teherautónak tele kellene lennie.");
        }

        [TestMethod]
        public void Buffer_FillVehicle_EmptyBuffer_ShouldDoNothing()
        {
            var buffer = new ProductBuffer(ProductType.STEAM, 100, 0);
            var tanker = new TankerTruck("TestTanker");

            int filled = buffer.FillVehicle(tanker);

            Assert.AreEqual(0, filled);
            Assert.AreEqual(0, tanker.CurrentLoad);
        }
        #endregion

        #region Station & Transport Tests

        [TestMethod]
        public void FactoryStation_LoadingWithCargoTruck()
        {
            var origin = new Coordinate(5, 5);
            var factory = new SulfurProducer("TestSulfur", origin);
            var station = new FactoryStation(origin+2, "FactoryStation", factory);

            SimulateProduction(factory, 60.0);
            int currentFactoryLoad = factory.FinalProductBuffer.CurrentLoad;

            Assert.IsTrue(currentFactoryLoad > 0, "A gyárnak termelnie kellett volna 60 másodperc alatt.");

            var truck = new CargoTruck("TestTruck");
            truck.Load(50, ProductType.SULFUR);

            int unloaded = station.LoadProduct(truck);

            Assert.AreEqual(currentFactoryLoad + 50, truck.CurrentLoad, "A kamion rakománya pont a gyár eddigi termelésével kellett volna növekedjen");
            Assert.AreEqual(0, factory.BaseProductBuffer.CurrentLoad, "Az árunak a gyár raktárából ki kellett volna kerülnie.");
            Assert.IsTrue(unloaded == currentFactoryLoad, "A gyárban lévő mennyiséget kellett volna a járműnek elfogadnia");

            int currentTruckLoad = truck.CurrentLoad;
            int loaded = station.UnLoadProductFromVehicle(truck);
            Assert.IsTrue(loaded == 0, "A gyárnak nincs szüksége kénre.");
            Assert.AreEqual(currentTruckLoad, truck.CurrentLoad, "A kamion rakománya változatlan kellene legyen.");
        }

        [TestMethod]
        public void CityStation_Unload_Logic_WithVehicleConstraints()
        {
            var city = new City("TestCity", new Coordinate(10, 10));
            var station = new CityStation(city, new Coordinate(12, 12), "CityStation");

            var testVehicles = new List<Vehicle>
            {
                new CargoTruck("Truck"),
                new TankerTruck("Tanker"),
            };

            foreach (var neededProduct in city.ProductTypes)
            {
                var capableVehicle = testVehicles.First(v => v.AllType.Contains(neededProduct));

                capableVehicle.Load(50, neededProduct);
                int unloaded = station.UnLoadProductFromVehicle(capableVehicle);

                Assert.AreEqual(50, unloaded, $"A városnak el kellett volna fogadnia a(z) {neededProduct} típust a(z) {capableVehicle.GetType().Name} járműtől.");
                Assert.AreEqual(0, capableVehicle.CurrentLoad, "A járműnek le kellett volna ürítenie a rakományt.");
                Assert.IsTrue(city.IsProductNeeded(neededProduct));


                var cargo = new CargoTruck("Cargo");
                var typeNotNeeded = cargo.AllType.First(v => !city.ProductTypes.Contains(v));
                cargo.Load(50, typeNotNeeded);
                int amount = station.UnLoadProductFromVehicle(cargo);

                Assert.AreEqual(0, amount, $"A városnak nem szabadna elfogadnia a(z) {typeNotNeeded} típust a(z) {cargo.GetType().Name} járműtől.");
                Assert.AreEqual(50, cargo.CurrentLoad, "A járműnek nem kellett volna ürítenie a rakományt.");
                Assert.IsFalse(city.IsProductNeeded(typeNotNeeded));
            }
        }

        [TestMethod]
        public void Passenger_Boarding_ShouldRespectCapacity()
        {
            var city = new City("TestCity", new Coordinate(0, 0));
            var station = new CityStation(city, new Coordinate(2, 2), "Station");
            int people = station.GetWaitingPassengers(1000); // 10 ember lesz

            var miniBus = new MiniBus("SmallBus");

            int boarded = station.Boarding(miniBus);

            Assert.AreEqual(people, boarded, "A minibuszra csak 12 ember férhet fel.");
            Assert.AreEqual(people, miniBus.CurrentLoad, "A buszra mindenkinek fel kellett volna férnie");
            Assert.AreEqual(ProductType.HUMAN, miniBus.CurrentType);

            people = station.GetWaitingPassengers(1000);
            boarded = station.Boarding(miniBus);
            Assert.AreEqual(miniBus.Capacity-people, boarded, "A minibuszra csak 12 ember férhet fel.");
            Assert.AreEqual(miniBus.Capacity, miniBus.CurrentLoad, "A busznak tele kellene lennie.");
        }

        [TestMethod]
        public void UnBoarding_WrongProductType_ShouldReturnZero()
        {
            var city = new City("TestCity", new Coordinate(0, 0));
            var station = new CityStation(city, new Coordinate(2, 2), "TestStation");
            var truck = new CargoTruck("Truck");
            truck.Load(50, ProductType.ASH);

            int unloaded = station.UnBoarding(truck);

            Assert.AreEqual(0, unloaded, "Csak emberek szállhatnak le az UnBoarding metódussal.");
            Assert.AreEqual(50, truck.CurrentLoad, "A rakománynak érintetlennek kell maradnia.");
        }

        #endregion

        private static void SimulateProduction(Factory factory, double seconds, float currentTime = 0)
        {
            double dt = 0.1;
            for (double t = 0; t < seconds; t += dt)
            {
                factory.Update(dt, currentTime);
            }
        }
    }
}
