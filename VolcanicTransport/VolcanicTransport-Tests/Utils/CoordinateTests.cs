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
            Assert.AreEqual(new Coordinate(1,2), new Coordinate(1,2));
        }


        [TestMethod]
        public void AdditionCoordCoord()
        {
            Coordinate A = new Coordinate(1, 1);
            Coordinate B = new Coordinate(1, 1);
            Coordinate result = new Coordinate(2, 2);
            Assert.AreEqual(result, A + B);
        }
    }
}
