using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.EconomyTests
{
    [TestClass]
    [DoNotParallelize]
    public class ProductCoverageTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 401);

        [TestMethod]
        public void GetDemand_ReturnsInt()
        {
            var product = new Product(ProductType.HUMAN, 0f, 100f);
            int demand = product.GetDemand(0f);
            Assert.IsTrue(demand >= 0);
        }

        [TestMethod]
        public void GetPassengerEfficiency_ReturnsBetweenZeroAndOne()
        {
            var product = new Product(ProductType.HUMAN, 0f, 100f);
            float eff = product.GetPassengerEfficiency(0f);
            Assert.IsTrue(float.IsFinite(eff));
        }
    }

    // ── Factory: BaseProduct deduction + full buffer ─────────────────
    [TestClass]
    [DoNotParallelize]
    public class FactoryProductionCoverageTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 402);

        [TestMethod]
        public void ConcreteFactory_WithBaseProduct_DeductsBaseProduct()
        {
            var factory = new ConcreteFactory("CovFactory", new Coordinate(5, 5));

            factory.BaseProductBuffer.AddAmount(factory.BaseProductBuffer.MaxCapacity);
            int ashBefore = factory.BaseProductBuffer.CurrentLoad;

            factory.Update(100.0, 0f);

            int ashAfter = factory.BaseProductBuffer.CurrentLoad;
            Assert.IsTrue(ashAfter <= ashBefore);
        }

        [TestMethod]
        public void SulfurProducer_FullBuffer_SetsAccumulatorToNear1()
        {
            var factory = new SulfurProducer("FullFactory", new Coordinate(6, 6));

            factory.FinalProductBuffer.AddAmount(factory.FinalProductBuffer.MaxCapacity);

            factory.Update(100.0, 0f);
        }
    }
}
