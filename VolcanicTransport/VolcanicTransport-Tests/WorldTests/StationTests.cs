using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;

using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.WorldTests
{
    [TestClass]
    [DoNotParallelize]
    public class CityStationTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(8, 55);

        private static City MakeCity(string name = "TestCity", int x = 10, int y = 10)
            => new(name, new Coordinate(x, y));

        private static CityStation MakeStation(City city, int x = 5, int y = 5)
            => new(city, new Coordinate(x, y), city.Name + " Stop");

        [TestMethod]
        public void CityStation_Constructor_SetsCorrectCityName()
        {
            var city = MakeCity("Kecskemét");
            var station = MakeStation(city, 3, 3);
            Assert.AreEqual("Kecskemét", station.CityName);
        }

        [TestMethod]
        public void CityStation_Constructor_StationNameSet()
        {
            var city = MakeCity("Szolnok");
            var station = MakeStation(city, 4, 4);
            Assert.IsFalse(string.IsNullOrEmpty(station.StationName));
        }

        [TestMethod]
        public void UnLoadProductFromVehicle_NullVehicle_ReturnsZero()
        {
            var city = MakeCity("NullCity", 1, 1);
            var station = MakeStation(city, 2, 2);
            int result = station.UnLoadProductFromVehicle(null!);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Inspect_ReturnsNonEmptyString()
        {
            var city = MakeCity("InspCity", 3, 3);
            var station = MakeStation(city, 3, 4);
            string text = station.Inspect();
            Assert.IsFalse(string.IsNullOrWhiteSpace(text));
        }

        [TestMethod]
        public void Inspect_ContainsStationName()
        {
            var city = MakeCity("NameCity", 2, 2);
            var station = MakeStation(city, 2, 3);
            string text = station.Inspect();
            Assert.IsTrue(text.Contains(station.StationName));
        }

        [TestMethod]
        public void Inspect_ContainsCityProductNeeds()
        {
            var city = MakeCity("NeedCity", 1, 1);
            var station = MakeStation(city, 1, 2);
            string text = station.Inspect();
            foreach (var pt in city.ProductTypes)
                Assert.IsTrue(text.Contains(pt.ToString()), $"'{pt}' hiányzik az Inspect() szövegéből.");
        }

        [TestMethod]
        public void GetCityProductNeeds_HasThreeProducts()
        {
            var city = MakeCity("ThreeCity", 9, 9);
            var station = MakeStation(city, 9, 10);
            Assert.AreEqual(3, station.GetCityProductNeeds.Count);
        }

        [TestMethod]
        public void JsonConstructor_SetsCityName()
        {
            var buf = new ProductBuffer(ProductType.HUMAN, 50);
            var dem = new Product(ProductType.HUMAN, 0, 50, 5);
            var station = new CityStation("JsonCityName", new Coordinate(20, 20), "JsonStop", buf, dem, 0.0);
            Assert.AreEqual("JsonCityName", station.CityName);
        }

        [TestMethod]
        public void RestoreReference_FindsCity()
        {
            var city = new City("RestoreCity", new Coordinate(21, 21));
            GameWorld.Instance.Cities.Add(city);

            var buf = new ProductBuffer(ProductType.HUMAN, 50);
            var dem = new Product(ProductType.HUMAN, 0, 50, 5);
            var station = new CityStation("RestoreCity", new Coordinate(21, 21), "RS", buf, dem, 0.0);
            station.RestoreReference(new Coordinate(21, 21));

            Assert.AreEqual(city.ProductTypes.Count, station.GetCityProductNeeds.Count);
            GameWorld.Instance.Cities.Remove(city);
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class FactoryStationTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(8, 66);

        private static SulfurProducer MakeFactory()
            => new("TestFactory", new Coordinate(3, 3));

        [TestMethod]
        public void FactoryStation_Constructor_SetsFactoryName()
        {
            var f = MakeFactory();
            var station = new FactoryStation(new Coordinate(5, 5), "TestStop", f);
            Assert.AreEqual(f.Name, station.FactoryName);
        }

        [TestMethod]
        public void LoadProduct_NullVehicle_ReturnsZero()
        {
            var factory = MakeFactory();
            var station = new FactoryStation(new Coordinate(5, 5), "NullStop", factory);
            int result = station.LoadProduct(null!);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void UnLoadProductFromVehicle_NullVehicle_ReturnsZero()
        {
            var factory = MakeFactory();
            var station = new FactoryStation(new Coordinate(5, 5), "NullUnld", factory);
            int result = station.UnLoadProductFromVehicle(null!);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Inspect_ReturnsNonEmptyString()
        {
            var factory = MakeFactory();
            var station = new FactoryStation(new Coordinate(5, 5), "InspStop", factory);
            string text = station.Inspect();
            Assert.IsFalse(string.IsNullOrWhiteSpace(text));
        }

        [TestMethod]
        public void Inspect_ContainsStationName()
        {
            var factory = MakeFactory();
            var station = new FactoryStation(new Coordinate(5, 5), "InspStop2", factory);
            string text = station.Inspect();
            Assert.IsTrue(text.Contains("InspStop2"));
        }

        [TestMethod]
        public void GetFactoryEfficiency_ReturnsValueBetweenZeroAndOne()
        {
            var factory = MakeFactory();
            var station = new FactoryStation(new Coordinate(5, 5), "EffStop", factory);
            float eff = station.GetFactoryEfficiency(0f);
            Assert.IsTrue(eff >= 0f && eff <= 1f);
        }

        [TestMethod]
        public void FactoryStation_Properties_DoNotThrow()
        {
            var factory = MakeFactory();
            var station = new FactoryStation(new Coordinate(5, 5), "PropStop", factory);
            _ = station.GetFactoryNeeds;
            _ = station.GetFactoryFinishedProduct;
            _ = station.GetFactoryFinishedProductAmount;
            _ = station.GetFactoryBaseProductAmount;
            _ = station.GetFactoryNeedsAmount;
            _ = station.PricePerBaseProduct;
        }

        [TestMethod]
        public void JsonConstructor_SetsFactoryName()
        {
            var buf = new ProductBuffer(ProductType.HUMAN, 50);
            var dem = new Product(ProductType.HUMAN, 0, 50, 5);
            var station = new FactoryStation("JsonFactoryXYZ", new Coordinate(30, 30), "JsonFStop", buf, dem, 0.0);
            Assert.AreEqual("JsonFactoryXYZ", station.FactoryName);
        }

        [TestMethod]
        public void RestoreReference_FindsFactory()
        {
            var factory = new SulfurProducer("RestoreFactoryXYZ", new Coordinate(31, 31));
            GameWorld.Instance.Factories.Add(factory);

            var buf = new ProductBuffer(ProductType.HUMAN, 50);
            var dem = new Product(ProductType.HUMAN, 0, 50, 5);
            var station = new FactoryStation("RestoreFactoryXYZ", new Coordinate(31, 31), "RFS", buf, dem, 0.0);
            station.RestoreReference(new Coordinate(31, 31));

            Assert.AreEqual(ProductType.SULFUR, station.GetFactoryFinishedProduct);
            GameWorld.Instance.Factories.Remove(factory);
        }
    }
}
