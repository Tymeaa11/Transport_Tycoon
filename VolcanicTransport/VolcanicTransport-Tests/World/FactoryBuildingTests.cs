using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport_Tests.WorldTests
{
    [TestClass]
    [DoNotParallelize]
    public class FactoryBuildingTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => World.Initialise(4, 0);

        private static SulfurProducer MakeFactory(string name = "SulfurFactory", int x = 3, int y = 3)
            => new SulfurProducer(name, new Coordinate(x, y));

        [TestMethod]
        public void Constructor_WithFactory_SetsFactoryName()
        {
            var factory = MakeFactory("MyFactory");
            var building = new FactoryBuilding(factory);
            Assert.AreEqual("MyFactory", building.FactoryName);
        }

        [TestMethod]
        public void JsonConstructor_SetsFactoryNameOnly()
        {
            var building = new FactoryBuilding("JsonFactory");
            Assert.AreEqual("JsonFactory", building.FactoryName);
        }

        [TestMethod]
        public void BaseProduct_ReturnsFactoryBaseProduct()
        {
            var factory = MakeFactory();
            var building = new FactoryBuilding(factory);
            Assert.AreEqual(factory.BaseProduct, building.BaseProduct);
        }

        [TestMethod]
        public void FinalProduct_ReturnsFactoryFinalProductType()
        {
            var factory = MakeFactory();
            var building = new FactoryBuilding(factory);
            Assert.AreEqual(factory.FinalProduct.ProductType, building.FinalProduct);
        }

        [TestMethod]
        public void BaseProductNeed_ReturnsBufferMaxCapacity()
        {
            var factory = MakeFactory();
            var building = new FactoryBuilding(factory);
            Assert.AreEqual(factory.BaseProductBuffer.MaxCapacity, building.BaseProductNeed);
        }

        [TestMethod]
        public void BaseProductAmount_ReturnsCurrentLoad()
        {
            var factory = MakeFactory();
            var building = new FactoryBuilding(factory);
            Assert.AreEqual(factory.BaseProductBuffer.CurrentLoad, building.BaseProductAmount);
        }

        [TestMethod]
        public void FinalProductAmount_ReturnsCurrentFinalLoad()
        {
            var factory = MakeFactory();
            var building = new FactoryBuilding(factory);
            Assert.AreEqual(factory.FinalProductBuffer.CurrentLoad, building.FinalProductAmount);
        }

        [TestMethod]
        public void Inspect_ReturnsNonEmptyString()
        {
            var factory = MakeFactory();
            var building = new FactoryBuilding(factory);
            Assert.IsFalse(string.IsNullOrWhiteSpace(building.Inspect()));
        }

        [TestMethod]
        public void Inspect_ContainsFactoryName()
        {
            var factory = MakeFactory("InspFactory");
            var building = new FactoryBuilding(factory);
            Assert.IsTrue(building.Inspect().Contains("InspFactory"));
        }

        [TestMethod]
        public void Inspect_ContainsFinalProductType()
        {
            var factory = MakeFactory();
            var building = new FactoryBuilding(factory);
            Assert.IsTrue(building.Inspect().Contains(factory.FinalProduct.ProductType.ToString()));
        }

        [TestMethod]
        public void RestoreReference_FindsCorrectFactory()
        {
            var factory = MakeFactory("RestoreFactory", 6, 6);
            World.Instance.Factories.Add(factory);

            var building = new FactoryBuilding("RestoreFactory");
            var coord = new Coordinate(6, 6);
            building.RestoreReference(coord);

            Assert.AreEqual(factory.FinalProduct.ProductType, building.FinalProduct);

            World.Instance.Factories.Remove(factory);
        }
    }
}
