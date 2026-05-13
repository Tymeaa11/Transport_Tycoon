using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_Tests.RoadnetworkTests
{
    [TestClass]
    public class PathfinderTests
    {
        private static RoadNode CreateNode(int x, int y, bool isStation = false)
            => new(new Coordinate(x, y), isStation);

        private static Road CreateRoad(int x, int y)
            => new(new Coordinate(x, y));

        [TestMethod]
        public void FindPath_SameStartAndTarget_ReturnsEmptyList()
        {
            var node = CreateNode(0, 0);
            var result = Pathfinder.Instance.FindPath(node, node);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindPath_DisconnectedNodes_ReturnsNull()
        {
            var start = CreateNode(0, 0);
            var end = CreateNode(10, 10);

            var result = Pathfinder.Instance.FindPath(start, end);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindPath_DirectEdge_ReturnsCorrectRoads()
        {
            var start = CreateNode(0, 0);
            var end = CreateNode(0, 3);
            var r1 = CreateRoad(0, 1);
            var r2 = CreateRoad(0, 2);
            start.Edges.Add(new RoadEdge(end, 2, [r1, r2]));

            var result = Pathfinder.Instance.FindPath(start, end);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(r1, result[0]);
            Assert.AreEqual(r2, result[1]);
        }

        [TestMethod]
        public void FindPath_MultiHopPath_ReturnsAllSegmentsInOrder()
        {
            var start = CreateNode(0, 0);
            var mid = CreateNode(3, 0);
            var end = CreateNode(3, 3);
            var r1 = CreateRoad(1, 0);
            var r2 = CreateRoad(2, 0);
            var r3 = CreateRoad(3, 1);
            var r4 = CreateRoad(3, 2);
            start.Edges.Add(new RoadEdge(mid, 2, [r1, r2]));
            mid.Edges.Add(new RoadEdge(end, 2, [r3, r4]));

            var result = Pathfinder.Instance.FindPath(start, end);

            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new List<Road> { r1, r2, r3, r4 }, result);
        }

        [TestMethod]
        public void FindPath_TwoPaths_ChoosesLowerWeightPath()
        {
            var start = CreateNode(0, 0);
            var end = CreateNode(10, 0);
            var midCheap = CreateNode(5, 1);
            var midExpensive = CreateNode(5, -1);
            var cheapRoad1 = CreateRoad(2, 0);
            var cheapRoad2 = CreateRoad(8, 0);
            var expRoad1 = CreateRoad(2, -1);
            var expRoad2 = CreateRoad(8, -1);
            start.Edges.Add(new RoadEdge(midCheap, 1, [cheapRoad1]));
            start.Edges.Add(new RoadEdge(midExpensive, 5, [expRoad1]));
            midCheap.Edges.Add(new RoadEdge(end, 1, [cheapRoad2]));
            midExpensive.Edges.Add(new RoadEdge(end, 5, [expRoad2]));

            var result = Pathfinder.Instance.FindPath(start, end);

            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new List<Road> { cheapRoad1, cheapRoad2 }, result);
        }

        [TestMethod]
        public void FindPath_IntermediateStation_IsSkipped_PathNotFound()
        {
            var start = CreateNode(0, 0);
            var station = CreateNode(3, 0, isStation: true);
            var end = CreateNode(6, 0);
            start.Edges.Add(new RoadEdge(station, 2, [CreateRoad(1, 0), CreateRoad(2, 0)]));
            station.Edges.Add(new RoadEdge(end, 2, [CreateRoad(4, 0), CreateRoad(5, 0)]));

            var result = Pathfinder.Instance.FindPath(start, end);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindPath_TargetIsStation_IsReachable()
        {
            var start = CreateNode(0, 0);
            var end = CreateNode(3, 0, isStation: true);
            var r1 = CreateRoad(1, 0);
            var r2 = CreateRoad(2, 0);
            start.Edges.Add(new RoadEdge(end, 2, [r1, r2]));

            var result = Pathfinder.Instance.FindPath(start, end);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void FindPath_SlopePathVsFlatPath_ChoosesFlatPath()
        {
            var start = CreateNode(0, 0);
            var end = CreateNode(10, 0);
            var midFlat = CreateNode(5, 2);
            var midSlope = CreateNode(5, -2);
            start.Edges.Add(new RoadEdge(midFlat, 1, [CreateRoad(2, 1)]));
            start.Edges.Add(new RoadEdge(midSlope, 2, [CreateRoad(2, -1)]));
            midFlat.Edges.Add(new RoadEdge(end, 1, [CreateRoad(8, 1)]));
            midSlope.Edges.Add(new RoadEdge(end, 2, [CreateRoad(8, -1)]));

            var result = Pathfinder.Instance.FindPath(start, end);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(r => r.Coordinate.Y >= 0),
                "A kiválasztott útnak nem szabad meredek (Y<0) szakaszon átmennie.");
        }

        [TestMethod]
        public void FindPath_CyclicGraph_TerminatesAndFindsPath()
        {
            var a = CreateNode(0, 0);
            var b = CreateNode(3, 0);
            var c = CreateNode(6, 0);
            var d = CreateNode(9, 0);
            var rAb1 = CreateRoad(1, 0); var rAb2 = CreateRoad(2, 0);
            var rBc1 = CreateRoad(4, 0); var rBc2 = CreateRoad(5, 0);
            var rCa = CreateRoad(3, 1);
            var rCd1 = CreateRoad(7, 0); var rCd2 = CreateRoad(8, 0);

            a.Edges.Add(new RoadEdge(b, 2, [rAb1, rAb2]));
            b.Edges.Add(new RoadEdge(c, 2, [rBc1, rBc2]));
            c.Edges.Add(new RoadEdge(a, 1, [rCa]));
            c.Edges.Add(new RoadEdge(d, 2, [rCd1, rCd2]));

            var result = Pathfinder.Instance.FindPath(a, d);

            Assert.IsNotNull(result, "Ciklikus gráfban is megtalálható az útvonal.");
            CollectionAssert.AreEqual(
                new List<Road> { rAb1, rAb2, rBc1, rBc2, rCd1, rCd2 }, result);
        }

        [TestMethod]
        public void FindPath_FourNodeChain_ReturnsAllRoadsInOrder()
        {
            var a = CreateNode(0, 0);
            var b = CreateNode(3, 0);
            var c = CreateNode(6, 0);
            var d = CreateNode(9, 0);
            var r1 = CreateRoad(1, 0); var r2 = CreateRoad(2, 0);
            var r3 = CreateRoad(4, 0); var r4 = CreateRoad(5, 0);
            var r5 = CreateRoad(7, 0); var r6 = CreateRoad(8, 0);
            a.Edges.Add(new RoadEdge(b, 2, [r1, r2]));
            b.Edges.Add(new RoadEdge(c, 2, [r3, r4]));
            c.Edges.Add(new RoadEdge(d, 2, [r5, r6]));

            var result = Pathfinder.Instance.FindPath(a, d);

            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new List<Road> { r1, r2, r3, r4, r5, r6 }, result);
        }

        [TestMethod]
        public void FindPath_StartNodeIsStation_CanReachTarget()
        {
            var start = CreateNode(0, 0, isStation: true);
            var end = CreateNode(3, 0);
            var r1 = CreateRoad(1, 0); var r2 = CreateRoad(2, 0);
            start.Edges.Add(new RoadEdge(end, 2, [r1, r2]));

            var result = Pathfinder.Instance.FindPath(start, end);

            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new List<Road> { r1, r2 }, result);
        }
    }
}
