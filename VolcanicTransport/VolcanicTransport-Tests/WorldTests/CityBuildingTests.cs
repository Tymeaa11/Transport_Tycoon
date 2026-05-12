using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;

using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.WorldTests
{
    [TestClass]
    [DoNotParallelize]
    public class CityBuildingTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 0);

        private static City MakeCity(string name = "Szolnok", int x = 5, int y = 5)
            => new(name, new Coordinate(x, y));

        [TestMethod]
        public void Constructor_WithCity_SetsCityName()
        {
            var city = MakeCity("Cegléd");
            var building = new CityBuilding(city);
            Assert.AreEqual("Cegléd", building.CityName);
        }

        [TestMethod]
        public void JsonConstructor_SetsCityNameOnly()
        {
            var building = new CityBuilding("JsonCity");
            Assert.AreEqual("JsonCity", building.CityName);
        }

        [TestMethod]
        public void ProductTypes_ReturnsNonEmptyList()
        {
            var city = MakeCity();
            var building = new CityBuilding(city);
            Assert.IsTrue(building.ProductTypes.Count > 0);
        }

        [TestMethod]
        public void ProductTypes_MatchesCityProductTypes()
        {
            var city = MakeCity();
            var building = new CityBuilding(city);
            CollectionAssert.AreEquivalent(city.ProductTypes, building.ProductTypes);
        }

        [TestMethod]
        public void Inspect_ReturnsNonEmptyString()
        {
            var city = MakeCity("InspCity");
            var building = new CityBuilding(city);
            string text = building.Inspect();
            Assert.IsFalse(string.IsNullOrWhiteSpace(text));
        }

        [TestMethod]
        public void Inspect_ContainsCityName()
        {
            var city = MakeCity("NameCity");
            var building = new CityBuilding(city);
            Assert.IsTrue(building.Inspect().Contains("NameCity"));
        }

        [TestMethod]
        public void Inspect_ContainsAllProductTypes()
        {
            var city = MakeCity();
            var building = new CityBuilding(city);
            string text = building.Inspect();
            foreach (var pt in city.ProductTypes)
                Assert.IsTrue(text.Contains(pt.ToString()), $"'{pt}' missing from Inspect().");
        }

        [TestMethod]
        public void RestoreReference_FindsCorrectCity()
        {
            var city = MakeCity("RestoreCity", 10, 10);
            GameWorld.Instance.Cities.Add(city);

            var building = new CityBuilding("RestoreCity");
            var coord = new Coordinate(10, 10);
            building.RestoreReference(coord);

            Assert.AreEqual(city.ProductTypes.Count, building.ProductTypes.Count);

            GameWorld.Instance.Cities.Remove(city);
        }
    }
}
