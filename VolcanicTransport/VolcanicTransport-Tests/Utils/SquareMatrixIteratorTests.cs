using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport_Tests.Utils
{
    [TestClass]
    public class SquareMatrixIteratorTests
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
}
