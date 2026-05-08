using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_Tests.Roadnetwork
{

    [TestClass]
    [DoNotParallelize]
    public class RoadTypeTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            World.Initialise(4, 0);
        }


        private static VolcanicTransport.Model.World.Road PlaceRoad(Coordinate coord)
        {
            var road = new VolcanicTransport.Model.World.Road(coord);
            World.Instance.GetField(coord)!.Surface = road;
            return road;
        }

        private static void SetFieldType(Coordinate coord, FieldType type)
            => World.Instance.GetField(coord)!.SetFieldTypeTo(type);

        [TestMethod]
        public void Update_NoNeighbors_IsLonely()
        {
            var road = PlaceRoad(new Coordinate(10, 10));
            road.Update();
            Assert.AreEqual(RoadType.LONELY, road.RoadType);
        }

        [TestMethod]
        public void Update_NorthAndSouth_IsStraightNS()
        {
            var center = PlaceRoad(new Coordinate(20, 10));
            PlaceRoad(new Coordinate(20, 9));
            PlaceRoad(new Coordinate(20, 11));
            center.Update();
            Assert.AreEqual(RoadType.STRAIGHT_NS, center.RoadType);
        }

        [TestMethod]
        public void Update_EastAndWest_IsStraightEW()
        {
            var center = PlaceRoad(new Coordinate(30, 10));
            PlaceRoad(new Coordinate(31, 10));
            PlaceRoad(new Coordinate(29, 10));
            center.Update();
            Assert.AreEqual(RoadType.STRAIGHT_EW, center.RoadType);
        }

        [TestMethod]
        public void Update_NorthAndEast_IsCurvedNE()
        {
            var center = PlaceRoad(new Coordinate(40, 10));
            PlaceRoad(new Coordinate(40, 9));
            PlaceRoad(new Coordinate(41, 10));
            center.Update();
            Assert.AreEqual(RoadType.CURVED_NE, center.RoadType);
        }

        [TestMethod]
        public void Update_SouthAndWest_IsCurvedSW()
        {
            var center = PlaceRoad(new Coordinate(50, 10));
            PlaceRoad(new Coordinate(50, 11));
            PlaceRoad(new Coordinate(49, 10));
            center.Update();
            Assert.AreEqual(RoadType.CURVED_SW, center.RoadType);
        }

        [TestMethod]
        public void Update_ThreeWayNES_IsJunction()
        {
            var center = PlaceRoad(new Coordinate(60, 10));
            PlaceRoad(new Coordinate(60, 9));
            PlaceRoad(new Coordinate(61, 10));
            PlaceRoad(new Coordinate(60, 11));
            center.Update();
            Assert.AreEqual(RoadType.JUNCTION_T_NES, center.RoadType);
            Assert.IsTrue(center.IsJunction());
        }

        [TestMethod]
        public void Update_FourWay_IsJunctionNESW()
        {
            var center = PlaceRoad(new Coordinate(70, 10));
            PlaceRoad(new Coordinate(70, 9));
            PlaceRoad(new Coordinate(70, 11));
            PlaceRoad(new Coordinate(71, 10));
            PlaceRoad(new Coordinate(69, 10));
            center.Update();
            Assert.AreEqual(RoadType.JUNCTION_NESW, center.RoadType);
        }

        [TestMethod]
        public void Update_NorthNeighborHigherByOne_IsSlopeNS()
        {
            SetFieldType(new Coordinate(80, 9), FieldType.LAVA_OCEAN);
            var center = PlaceRoad(new Coordinate(80, 10));
            PlaceRoad(new Coordinate(80, 9));
            PlaceRoad(new Coordinate(80, 11));
            center.Update();
            Assert.AreEqual(RoadType.SLOPE_NS, center.RoadType);
            Assert.IsTrue(center.IsSlope());
        }

        [TestMethod]
        public void Update_NeighborHigherByTwo_IsInvalid()
        {
            SetFieldType(new Coordinate(90, 9), FieldType.BEACH);
            var center = PlaceRoad(new Coordinate(90, 10));
            PlaceRoad(new Coordinate(90, 9));
            center.Update();
            Assert.AreEqual(RoadType.INVALID, center.RoadType);
        }

        [TestMethod]
        public void Update_CurvedRoadWithHeightDifference_IsInvalid()
        {
            SetFieldType(new Coordinate(100, 9), FieldType.LAVA_OCEAN);
            var center = PlaceRoad(new Coordinate(100, 10));
            PlaceRoad(new Coordinate(100, 9));
            PlaceRoad(new Coordinate(101, 10));
            center.Update();
            Assert.AreEqual(RoadType.INVALID, center.RoadType);
        }

        [TestMethod]
        public void IsStraight_StraightEWRoad_ReturnsTrue()
        {
            var center = PlaceRoad(new Coordinate(20, 25));
            PlaceRoad(new Coordinate(21, 25));
            PlaceRoad(new Coordinate(19, 25));
            center.Update();
            Assert.IsTrue(center.IsStraight());
            Assert.IsFalse(center.IsCurved());
            Assert.IsFalse(center.IsJunction());
        }

        [TestMethod]
        public void IsCurved_CurvedNWRoad_ReturnsTrue()
        {
            var center = PlaceRoad(new Coordinate(30, 25));
            PlaceRoad(new Coordinate(30, 24));
            PlaceRoad(new Coordinate(29, 25));
            center.Update();
            Assert.IsTrue(center.IsCurved());
            Assert.IsFalse(center.IsStraight());
        }

        [TestMethod]
        public void TryUpdateNeighbours_AllValidNeighbors_ReturnsTrue()
        {
            var center = PlaceRoad(new Coordinate(40, 25));
            PlaceRoad(new Coordinate(41, 25));
            PlaceRoad(new Coordinate(39, 25));
            center.Update();

            bool result = center.TryUpdateNeighbours();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryUpdateNeighbours_InvalidNeighborAfterUpdate_ReturnsFalse()
        {
            SetFieldType(new Coordinate(50, 24), FieldType.BEACH);
            var center = PlaceRoad(new Coordinate(50, 25));
            var northRoad = PlaceRoad(new Coordinate(50, 24));
            center.Update();

            bool result = center.TryUpdateNeighbours();

            Assert.IsFalse(result);
            Assert.AreEqual(RoadType.INVALID, northRoad.RoadType);
        }


        [TestMethod]
        public void Update_EastAndSouth_IsCurvedES()
        {
            var center = PlaceRoad(new Coordinate(10, 40));
            PlaceRoad(new Coordinate(11, 40));
            PlaceRoad(new Coordinate(10, 41));
            center.Update();
            Assert.AreEqual(RoadType.CURVED_ES, center.RoadType);
            Assert.IsTrue(center.IsCurved());
        }

        [TestMethod]
        public void Update_NorthAndWest_IsCurvedWN()
        {
            var center = PlaceRoad(new Coordinate(20, 40));
            PlaceRoad(new Coordinate(20, 39));
            PlaceRoad(new Coordinate(19, 40));
            center.Update();
            Assert.AreEqual(RoadType.CURVED_WN, center.RoadType);
            Assert.IsTrue(center.IsCurved());
        }

        [TestMethod]
        public void Update_WestNorthEast_IsJunctionWNE()
        {
            var center = PlaceRoad(new Coordinate(30, 40));
            PlaceRoad(new Coordinate(29, 40));
            PlaceRoad(new Coordinate(30, 39));
            PlaceRoad(new Coordinate(31, 40));
            center.Update();
            Assert.AreEqual(RoadType.JUNCTION_T_WNE, center.RoadType);
            Assert.IsTrue(center.IsJunction());
            Assert.IsFalse(center.IsStraight());
            Assert.IsFalse(center.IsCurved());
        }

        [TestMethod]
        public void Update_EastSouthWest_IsJunctionESW()
        {
            var center = PlaceRoad(new Coordinate(40, 40));
            PlaceRoad(new Coordinate(41, 40));
            PlaceRoad(new Coordinate(40, 41));
            PlaceRoad(new Coordinate(39, 40));
            center.Update();
            Assert.AreEqual(RoadType.JUNCTION_T_ESW, center.RoadType);
            Assert.IsTrue(center.IsJunction());
        }

        [TestMethod]
        public void Update_SouthWestNorth_IsJunctionSWN()
        {
            var center = PlaceRoad(new Coordinate(50, 40));
            PlaceRoad(new Coordinate(50, 41));
            PlaceRoad(new Coordinate(49, 40));
            PlaceRoad(new Coordinate(50, 39));
            center.Update();
            Assert.AreEqual(RoadType.JUNCTION_T_SWN, center.RoadType);
            Assert.IsTrue(center.IsJunction());
        }

        [TestMethod]
        public void Update_SingleNorthNeighborFlat_HasNorthDirectionOnly()
        {
            var center = PlaceRoad(new Coordinate(60, 40));
            PlaceRoad(new Coordinate(60, 39));
            center.Update();
            Assert.AreEqual(RoadType.NORTH, center.RoadType);
            Assert.IsFalse(center.IsStraight());
            Assert.IsFalse(center.IsCurved());
            Assert.IsFalse(center.IsJunction());
            Assert.IsFalse(center.IsSlope());
        }

        [TestMethod]
        public void Update_SingleNorthNeighborHigherByOne_IsSlopeN()
        {
            SetFieldType(new Coordinate(70, 39), FieldType.LAVA_OCEAN);
            var center = PlaceRoad(new Coordinate(70, 40));
            PlaceRoad(new Coordinate(70, 39));
            center.Update();
            Assert.AreEqual(RoadType.SLOPE_N, center.RoadType);
            Assert.IsTrue(center.IsSlope());
        }

        [TestMethod]
        public void Update_StraightEWWithWestHigherByOne_IsSlopeEW()
        {
            SetFieldType(new Coordinate(79, 40), FieldType.LAVA_OCEAN);
            var center = PlaceRoad(new Coordinate(80, 40));
            PlaceRoad(new Coordinate(81, 40));
            PlaceRoad(new Coordinate(79, 40));
            center.Update();
            Assert.AreEqual(RoadType.SLOPE_EW, center.RoadType);
            Assert.IsTrue(center.IsSlope());
            Assert.IsTrue(center.IsStraight());
        }

        [TestMethod]
        public void Update_FourWayJunctionWithHeightDifference_IsInvalid()
        {
            SetFieldType(new Coordinate(89, 40), FieldType.LAVA_OCEAN);
            var center = PlaceRoad(new Coordinate(90, 40));
            PlaceRoad(new Coordinate(90, 39));
            PlaceRoad(new Coordinate(91, 40));
            PlaceRoad(new Coordinate(90, 41));
            PlaceRoad(new Coordinate(89, 40));
            center.Update();
            Assert.AreEqual(RoadType.INVALID, center.RoadType);
        }

        [TestMethod]
        public void UpdateNeighbours_AfterNewRoadPlaced_NeighborTypeUpdated()
        {
            var center = PlaceRoad(new Coordinate(10, 45));
            var eastRoad = PlaceRoad(new Coordinate(11, 45));
            center.Update();
            eastRoad.Update();
            Assert.AreEqual(RoadType.WEST, eastRoad.RoadType);

            PlaceRoad(new Coordinate(12, 45));

            center.UpdateNeighbours();

            Assert.AreEqual(RoadType.STRAIGHT_EW, eastRoad.RoadType,
                "eastRoad-nak STRAIGHT_EW-re kell frissülnie az UpdateNeighbours() hívás után.");
        }
    }
}
