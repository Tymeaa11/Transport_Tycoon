using VolcanicTransport.Model.Utils;

namespace VolcanicTransport_Tests.Utils
{
    [TestClass]
    public sealed class CoordinateTests
    {
        [TestMethod]
        public void MetaTest()
        {
            // if this doesn't pass something is really wrong
        }


        [TestMethod]
        public void EqualsTest()
        {
            Assert.AreEqual(new Coordinate(1, 2), new Coordinate(1, 2));
        }


        [TestMethod]
        public void AdditionCoordCoord()
        {
            Coordinate a = new(1, 1);
            Coordinate b = new(1, 1);
            Coordinate result = new(2, 2);
            Assert.AreEqual(result, a + b);
        }

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
}
