using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;

using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.WorldTests
{
    [TestClass]
    [DoNotParallelize]
    public class FactoryBuildingTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 0);

        private static GameWorld WInstance => GameWorld.Instance;

        private static SulfurProducer MakeFactory(string name = "SulfurFactory", int x = 3, int y = 3)
            => new(name, new Coordinate(x, y));

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
            WInstance.Factories.Add(factory);

            var building = new FactoryBuilding("RestoreFactory");
            var coord = new Coordinate(6, 6);
            building.RestoreReference(coord);

            Assert.AreEqual(factory.FinalProduct.ProductType, building.FinalProduct);

            WInstance.Factories.Remove(factory);
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class CheckIfFactoryStateIsPreservedTests
    {
        private static GameWorld WInstance => GameWorld.Instance;
        private string? _tempFile;

        [TestInitialize]
        public void Setup()
        {
            _tempFile = Path.GetTempFileName() + ".zip";
            GameModel.InitialiseNewGame(4, 8888);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_tempFile != null && File.Exists(_tempFile))
                File.Delete(_tempFile);
        }

        [TestMethod]
        public void CheckIfFactoryStateIsPeserved_ProductTypeAndName()
        {
            var world = WInstance;

            if (world.Factories.Count == 0) return;

            var original = world.Factories[0];
            string originalName = original.Name;
            ProductType originalFinalProduct = original.FinalProduct.ProductType;

            GameModel.Instance.SaveGame(_tempFile!);
            GameModel.InitialiseLoadedGame(_tempFile!);

            var restored = WInstance.Factories.FirstOrDefault(f => f.Name == originalName);
            Assert.IsNotNull(restored, "A gyár nem található betöltés után.");
            Assert.AreEqual(originalFinalProduct, restored.FinalProduct.ProductType,
                "A gyár végterméke megváltozott betöltés után.");
        }

        [TestMethod]
        public void CheckIfFactoryStateIsPeserved_FactoryCount()
        {
            int countBefore = WInstance.Factories.Count;
            GameModel.Instance.SaveGame(_tempFile!);
            GameModel.InitialiseLoadedGame(_tempFile!);
            Assert.AreEqual(countBefore, WInstance.Factories.Count,
                "Gyárak száma megváltozott betöltés után.");
        }

        [TestMethod]
        public void CheckIfFactoryStateIsPeserved_FactoryBuildingLinkRestored()
        {
            var world = WInstance;
            if (world.Factories.Count == 0) return;

            var factory = world.Factories[0];
            var coord = new Coordinate(2, 2);

            var field = world.GetField(coord);
            if (field == null || field.Surface != null) return;

            var building = new FactoryBuilding(factory);
            field.Surface = building;
            factory.AddField(field);

            GameModel.Instance.SaveGame(_tempFile!);
            GameModel.InitialiseLoadedGame(_tempFile!);

            var loadedField = WInstance.GetField(coord);
            var loadedBuilding = loadedField?.Surface as FactoryBuilding;

            Assert.IsNotNull(loadedBuilding, "A gyárépület nem töltődött be.");
            Assert.AreEqual(factory.Name, loadedBuilding.FactoryName,
                "A gyárépület helytelen gyárhoz kapcsolódik betöltés után.");

            Assert.AreEqual(factory.FinalProduct.ProductType, loadedBuilding.FinalProduct,
                "A gyárépület FinalProduct tulajdonsága nem helyesen állítódott vissza.");
        }
    }
}
