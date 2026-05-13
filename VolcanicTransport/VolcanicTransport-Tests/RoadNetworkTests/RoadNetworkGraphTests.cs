using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.RoadnetworkTests
{
    [TestClass]
    [DoNotParallelize]
    public class RoadNetworkGraphTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            GameWorld.Initialise(4, 0);
        }

        [TestInitialize]
        public void ResetGraph()
        {
            GameWorld.Instance.Roadnetwork.NodeMap.Clear();
        }


        private static Road PlaceRoad(Coordinate coord)
        {
            var road = new Road(coord);
            GameWorld.Instance.GetField(coord)!.Surface = road;
            return road;
        }

        private static void SetFieldType(Coordinate coord, FieldType type)
            => GameWorld.Instance.GetField(coord)!.SetFieldTypeTo(type);

        private static (Road jA, Road conn, Road jB)
            BuildTwoJunctions(Coordinate jACoord, Coordinate jBCoord)
        {
            var connCoord = new Coordinate(jACoord.X + 1, jACoord.Y);

            PlaceRoad(new Coordinate(jACoord.X, jACoord.Y - 1));
            PlaceRoad(new Coordinate(jACoord.X, jACoord.Y + 1));
            PlaceRoad(new Coordinate(jBCoord.X, jBCoord.Y - 1));
            PlaceRoad(new Coordinate(jBCoord.X, jBCoord.Y + 1));

            var conn = PlaceRoad(connCoord);
            var jA = PlaceRoad(jACoord);
            var jB = PlaceRoad(jBCoord);

            jA.Update();
            jB.Update();
            conn.Update();

            return (jA, conn, jB);
        }


        [TestMethod]
        public void RegisterNodeIfNeeded_JunctionRoad_IsAddedToNodeMap()
        {
            var jACoord = new Coordinate(10, 50);
            var jBCoord = new Coordinate(12, 50);
            var (jA, _, _) = BuildTwoJunctions(jACoord, jBCoord);

            Assert.IsTrue(jA.IsJunction(), "A road típusát Update() után JUNCTION-nek kell lennie.");

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jACoord);

            Assert.IsTrue(GameWorld.Instance.Roadnetwork.NodeMap.ContainsKey(jA),
                "A junction csomópontként kell megjelenjen a NodeMap-ben.");
        }

        [TestMethod]
        public void RegisterNodeIfNeeded_StraightRoad_NotAddedToNodeMap()
        {
            var coord = new Coordinate(20, 50);
            var straight = PlaceRoad(coord);
            PlaceRoad(new Coordinate(21, 50));
            PlaceRoad(new Coordinate(19, 50));
            straight.Update();

            Assert.IsTrue(straight.IsStraight());

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(coord);

            Assert.IsFalse(GameWorld.Instance.Roadnetwork.NodeMap.ContainsKey(straight),
                "Egyenes útszakasz nem kerülhet be a gráfba csomópontként.");
        }

        [TestMethod]
        public void RegisterNodeIfNeeded_SameJunctionTwice_OnlyOneNodeAdded()
        {
            var jACoord = new Coordinate(30, 50);
            var jBCoord = new Coordinate(32, 50);
            var (jA, _, _) = BuildTwoJunctions(jACoord, jBCoord);

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jACoord);
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jACoord);

            int count = GameWorld.Instance.Roadnetwork.NodeMap.Count(kvp => kvp.Key == jA);
            Assert.AreEqual(1, count, "Ugyanaz a csomópont nem adható hozzá kétszer.");
        }

        [TestMethod]
        public void RebuildEdges_TwoConnectedJunctions_EdgesExistBothDirections()
        {
            var jACoord = new Coordinate(40, 50);
            var jBCoord = new Coordinate(42, 50);
            var (jA, _, jB) = BuildTwoJunctions(jACoord, jBCoord);

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jACoord);
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jBCoord);

            var nodeA = GameWorld.Instance.Roadnetwork.NodeMap[jA];
            var nodeB = GameWorld.Instance.Roadnetwork.NodeMap[jB];

            Assert.IsTrue(nodeA.Edges.Any(e => e.TargetNode == nodeB),
                "A jA csomópontból kell él vezessen jB felé.");
            Assert.IsTrue(nodeB.Edges.Any(e => e.TargetNode == nodeA),
                "A jB csomópontból kell él vezessen jA felé.");
        }

        [TestMethod]
        public void RebuildEdges_TwoConnectedJunctions_EdgePathContainsConnectorRoad()
        {
            var jACoord = new Coordinate(50, 50);
            var jBCoord = new Coordinate(52, 50);
            var (jA, conn, _) = BuildTwoJunctions(jACoord, jBCoord);

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jACoord);
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jBCoord);

            var nodeA = GameWorld.Instance.Roadnetwork.NodeMap[jA];
            var edgeToB = nodeA.Edges.First(e => e.TargetNode.Coordinate == jBCoord);

            CollectionAssert.Contains(edgeToB.Path, conn,
                "Az él útjának tartalmaznia kell az összekötő útszakaszt.");
        }

        [TestMethod]
        public void RebuildEdges_SlopeConnectorRoad_EdgeWeightIsHigherThanFlat()
        {
            var flatJaCoord = new Coordinate(60, 50);
            var flatJbCoord = new Coordinate(62, 50);
            var (flatJa, _, _) = BuildTwoJunctions(flatJaCoord, flatJbCoord);
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(flatJaCoord);
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(flatJbCoord);
            var flatNodeA = GameWorld.Instance.Roadnetwork.NodeMap[flatJa];
            var flatEdge = flatNodeA.Edges.First(e => e.TargetNode.Coordinate == flatJbCoord);
            int flatWeight = flatEdge.Weight;

            ResetGraph();
            SetFieldType(new Coordinate(70, 50), FieldType.LAVA_OCEAN);
            var slopeJaCoord = new Coordinate(70, 50);
            var slopeJbCoord = new Coordinate(72, 50);
            var (slopeJa, slopeConn, _) = BuildTwoJunctions(slopeJaCoord, slopeJbCoord);

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(slopeJaCoord);
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(slopeJbCoord);

            Assert.IsTrue(slopeConn.IsSlope(),
                "Az összekötő útszakasznak meredeknek (SLOPE) kell lennie.");

            var slopeNodeA = GameWorld.Instance.Roadnetwork.NodeMap[slopeJa];
            var slopeEdge = slopeNodeA.Edges.First(e => e.TargetNode.Coordinate == slopeJbCoord);

            Assert.IsTrue(slopeEdge.Weight > flatWeight,
                $"A meredek él súlya ({slopeEdge.Weight}) nagyobb kell legyen a sík él súlyánál ({flatWeight}).");
        }

        [TestMethod]
        public void RebuildEdges_SingleRegisteredNode_NodeHasNoEdges()
        {
            PlaceRoad(new Coordinate(10, 69));
            PlaceRoad(new Coordinate(10, 71));
            PlaceRoad(new Coordinate(11, 70));
            var junction = PlaceRoad(new Coordinate(10, 70));
            junction.Update();
            Assert.IsTrue(junction.IsJunction());

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(new Coordinate(10, 70));

            Assert.AreEqual(1, GameWorld.Instance.Roadnetwork.NodeMap.Count);
            var node = GameWorld.Instance.Roadnetwork.NodeMap[junction];
            Assert.AreEqual(0, node.Edges.Count,
                "1 csomópont esetén nem épülnek élek (RebuildEdges korai kilépés).");
        }


        [TestMethod]
        public void RebuildEdges_ThreeConnectedJunctions_AllEdgesExist()
        {
            var jACoord = new Coordinate(10, 60);
            var jBCoord = new Coordinate(12, 60);
            var jCCoord = new Coordinate(14, 60);

            PlaceRoad(new Coordinate(10, 59)); PlaceRoad(new Coordinate(10, 61));
            PlaceRoad(new Coordinate(12, 59)); PlaceRoad(new Coordinate(12, 61));
            PlaceRoad(new Coordinate(14, 59)); PlaceRoad(new Coordinate(14, 61));

            var connAb = PlaceRoad(new Coordinate(11, 60));
            var connBc = PlaceRoad(new Coordinate(13, 60));
            var jA = PlaceRoad(jACoord);
            var jB = PlaceRoad(jBCoord);
            var jC = PlaceRoad(jCCoord);

            jA.Update(); jB.Update(); jC.Update();
            connAb.Update(); connBc.Update();

            Assert.IsTrue(jA.IsJunction()); Assert.IsTrue(jB.IsJunction()); Assert.IsTrue(jC.IsJunction());

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jACoord);
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jBCoord);
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jCCoord);

            var nodeA = GameWorld.Instance.Roadnetwork.NodeMap[jA];
            var nodeB = GameWorld.Instance.Roadnetwork.NodeMap[jB];
            var nodeC = GameWorld.Instance.Roadnetwork.NodeMap[jC];

            Assert.IsTrue(nodeA.Edges.Any(e => e.TargetNode.Coordinate == jBCoord), "jA → jB él hiányzik.");
            Assert.IsTrue(nodeB.Edges.Any(e => e.TargetNode.Coordinate == jACoord), "jB → jA él hiányzik.");
            Assert.IsTrue(nodeB.Edges.Any(e => e.TargetNode.Coordinate == jCCoord), "jB → jC él hiányzik.");
            Assert.IsTrue(nodeC.Edges.Any(e => e.TargetNode.Coordinate == jBCoord), "jC → jB él hiányzik.");
            Assert.AreEqual(1, nodeA.Edges.Count, "jA-nak pontosan 1 éle kell legyen (csak jB felé).");
            Assert.AreEqual(2, nodeB.Edges.Count, "jB-nek pontosan 2 éle kell legyen (jA és jC felé).");
        }


        [TestMethod]
        public void RebuildEdges_ThreeIntermediateRoads_EdgePathContainsAllRoads()
        {
            var jACoord = new Coordinate(10, 65);
            var jBCoord = new Coordinate(14, 65);

            PlaceRoad(new Coordinate(10, 64)); PlaceRoad(new Coordinate(10, 66));
            PlaceRoad(new Coordinate(14, 64)); PlaceRoad(new Coordinate(14, 66));

            var r1 = PlaceRoad(new Coordinate(11, 65));
            var r2 = PlaceRoad(new Coordinate(12, 65));
            var r3 = PlaceRoad(new Coordinate(13, 65));
            var jA = PlaceRoad(jACoord);
            var jB = PlaceRoad(jBCoord);

            jA.Update(); jB.Update(); r1.Update(); r2.Update(); r3.Update();

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jACoord);
            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(jBCoord);

            var nodeA = GameWorld.Instance.Roadnetwork.NodeMap[jA];
            var edgeToJb = nodeA.Edges.First(e => e.TargetNode.Coordinate == jBCoord);

            CollectionAssert.IsSubsetOf(new List<Road> { r1, r2, r3 }, edgeToJb.Path,
                "Az él útjának tartalmaznia kell az összes közbülső útszakaszt.");
            Assert.AreEqual(4, edgeToJb.Weight);
        }


        [TestMethod]
        public void RegisterNodeIfNeeded_CurvedRoad_NotAddedToNodeMap()
        {
            var coord = new Coordinate(20, 70);
            var curved = PlaceRoad(coord);
            PlaceRoad(new Coordinate(20, 69));
            PlaceRoad(new Coordinate(21, 70));
            curved.Update();
            Assert.IsTrue(curved.IsCurved());

            GameWorld.Instance.Roadnetwork.RegisterNodeIfNeeded(coord);

            Assert.IsFalse(GameWorld.Instance.Roadnetwork.NodeMap.ContainsKey(curved),
                "Kanyar típusú út nem kerülhet be a gráfba csomópontként.");
        }
    }
}
