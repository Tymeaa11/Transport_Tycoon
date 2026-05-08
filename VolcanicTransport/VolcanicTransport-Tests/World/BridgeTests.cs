using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_Tests.WorldTests
{
    [TestClass]
    [DoNotParallelize]
    public class BridgeTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => World.Initialise(4, 0);

        [TestMethod]
        public void BoneBridge_RoadType_MatchesConstructorArgument()
        {
            var b = new BoneBridge(new Coordinate(1, 1), RoadType.STRAIGHT_NS, FieldType.LOW_LANDS);
            Assert.AreEqual(RoadType.STRAIGHT_NS, b.RoadType);
        }

        [TestMethod]
        public void BoneBridge_Elevation_MatchesConstructorArgument()
        {
            var b = new BoneBridge(new Coordinate(1, 1), RoadType.STRAIGHT_NS, FieldType.LOW_LANDS);
            Assert.AreEqual(FieldType.LOW_LANDS, b.Elevation);
        }

        [TestMethod]
        public void BoneBridge_SpeedLimit_IsPositive()
        {
            var b = new BoneBridge(new Coordinate(1, 1), RoadType.STRAIGHT_EW, FieldType.LOW_LANDS);
            Assert.IsTrue(b.SpeedLimit > 0);
        }

        [TestMethod]
        public void StoneBridge_RoadType_MatchesConstructorArgument()
        {
            var b = new StoneBridge(new Coordinate(2, 2), RoadType.STRAIGHT_EW, FieldType.HIGH_LANDS);
            Assert.AreEqual(RoadType.STRAIGHT_EW, b.RoadType);
        }

        [TestMethod]
        public void StoneBridge_Elevation_MatchesConstructorArgument()
        {
            var b = new StoneBridge(new Coordinate(2, 2), RoadType.STRAIGHT_EW, FieldType.HIGH_LANDS);
            Assert.AreEqual(FieldType.HIGH_LANDS, b.Elevation);
        }

        [TestMethod]
        public void SteelBridge_RoadType_MatchesConstructorArgument()
        {
            var b = new SteelBridge(new Coordinate(3, 3), RoadType.STRAIGHT_NS, FieldType.HIGH_LANDS);
            Assert.AreEqual(RoadType.STRAIGHT_NS, b.RoadType);
        }

        [TestMethod]
        public void SteelBridge_Elevation_MatchesConstructorArgument()
        {
            var b = new SteelBridge(new Coordinate(3, 3), RoadType.STRAIGHT_NS, FieldType.HIGH_LANDS);
            Assert.AreEqual(FieldType.HIGH_LANDS, b.Elevation);
        }

        [TestMethod]
        public void BoneBridge_Update_DoesNotThrow()
        {
            var b = new BoneBridge(new Coordinate(1, 1), RoadType.STRAIGHT_NS, FieldType.LOW_LANDS);
            b.Update();
        }

        [TestMethod]
        public void StoneBridge_Update_DoesNotThrow()
        {
            var b = new StoneBridge(new Coordinate(1, 1), RoadType.STRAIGHT_NS, FieldType.LOW_LANDS);
            b.Update();
        }

        [TestMethod]
        public void SteelBridge_Update_DoesNotThrow()
        {
            var b = new SteelBridge(new Coordinate(1, 1), RoadType.STRAIGHT_NS, FieldType.LOW_LANDS);
            b.Update();
        }

        [TestMethod]
        public void BoneBridge_SpeedLimit_BelowOrEqualStoneBridge()
        {
            var bone = new BoneBridge(new Coordinate(0, 0), RoadType.STRAIGHT_NS, FieldType.LOW_LANDS);
            var stone = new StoneBridge(new Coordinate(0, 0), RoadType.STRAIGHT_NS, FieldType.LOW_LANDS);
            Assert.IsTrue(bone.SpeedLimit <= stone.SpeedLimit);
        }
    }
}
