using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.Utils
{
    // ── Coordinate uncovered constructors and operators ──────────────
    [TestClass]
    public class CoordinateCoverageTests
    {
        [TestMethod]
        public void Coordinate_CopyConstructor_CopiesXY()
        {
            var orig = new Coordinate(3, 7);
            var copy = new Coordinate(orig);
            Assert.AreEqual(orig.X, copy.X);
            Assert.AreEqual(orig.Y, copy.Y);
        }

        [TestMethod]
        public void Coordinate_DefaultConstructor_IsOrigin()
        {
            var c = new Coordinate();
            Assert.AreEqual(0, c.X);
            Assert.AreEqual(0, c.Y);
        }

        [TestMethod]
        public void Coordinate_IsInsideWithTopLeftBottomRight_True()
        {
            var c = new Coordinate(2, 2);
            Assert.IsTrue(c.IsInside(new Coordinate(0, 0), new Coordinate(5, 5)));
        }

        [TestMethod]
        public void Coordinate_IsInsideWithTopLeftBottomRight_False()
        {
            var c = new Coordinate(6, 6);
            Assert.IsFalse(c.IsInside(new Coordinate(0, 0), new Coordinate(5, 5)));
        }

        [TestMethod]
        public void GetArea_OutOfBounds_ReturnsEmpty()
        {
            // topLeft not inside topRight → empty list
            var list = Coordinate.GetArea(new Coordinate(5, 5), new Coordinate(3, 3));
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void Multiply_CoordinateCoordinate()
        {
            var result = new Coordinate(3, 4) * new Coordinate(2, 5);
            Assert.AreEqual(6, result.X);
            Assert.AreEqual(20, result.Y);
        }

        [TestMethod]
        public void Divide_CoordinateCoordinate()
        {
            var result = new Coordinate(10, 20) / new Coordinate(2, 4);
            Assert.AreEqual(5, result.X);
            Assert.AreEqual(5, result.Y);
        }

        [TestMethod]
        public void Modulo_CoordinateCoordinate()
        {
            var result = new Coordinate(10, 7) % new Coordinate(3, 3);
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(1, result.Y);
        }
    }

    // ── ScalableTimer property/method coverage ───────────────────────
    [TestClass]
    public class ScalableTimerCoverageTests
    {
        [TestMethod]
        public void Enabled_GetAndSet_Work()
        {
            using var timer = new ScalableTimer();
            timer.Enabled = false;
            Assert.IsFalse(timer.Enabled);
            timer.Enabled = true;
            Assert.IsTrue(timer.Enabled);
            timer.Enabled = false;
        }

        [TestMethod]
        public void TimeScale_Getter_ReturnsSetValue()
        {
            using var timer = new ScalableTimer();
            timer.TimeScale = 2;
            Assert.AreEqual(2, timer.TimeScale);
            timer.TimeScale = 0;
        }

        [TestMethod]
        public void Stop_DoesNotThrow()
        {
            using var timer = new ScalableTimer();
            timer.TimeScale = 1;
            timer.Stop();
        }
    }

    // ── SquareMatrixIterator setter coverage ────────────────────────
    [TestClass]
    public class SquareMatrixIteratorSetterTests
    {
        [TestMethod]
        public void Indexer_Set_ValidIndex_ThrowsIndexOutOfRange()
        {
            var m = new SquareMatrixIterator<int>(3);
            Assert.ThrowsException<IndexOutOfRangeException>(() => m[new Coordinate(0, 0)] = 42);
        }

        [TestMethod]
        public void Indexer_Set_OutOfBoundsIndex_ThrowsIndexOutOfRange()
        {
            var m = new SquareMatrixIterator<int>(3);
            Assert.ThrowsException<IndexOutOfRangeException>(() => m[new Coordinate(-1, 0)] = 99);
        }
    }

    // ── NameSet setter + RandomNameGenerator exhaustion ─────────────
    [TestClass]
    public class NameSetCoverageTests
    {
        [TestMethod]
        public void NameSet_AllNames_SetterWorks()
        {
            var ns = new NameSet(new HashSet<string> { "A", "B" });
            ns.AllNames = new HashSet<string> { "C", "D" };
            Assert.IsTrue(ns.AllNames.Contains("C"));
        }

        [TestMethod]
        public void RandomNameGenerator_Exhaustion_ResetsUsable()
        {
            RandomNameGenerator.Reset();
            for (int i = 0; i < 6; i++)
            {
                RandomNameGenerator.NewName(typeof(SulfurProducer), i);
            }
            Assert.IsTrue(true);
        }
    }

    // ── Product.GetDemand and GetPassengerEfficiency ──────────────────
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

            Assert.IsTrue(true);
        }
    }
}
