using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadNetworkGraph
    {
        public Dictionary<KnowsNeighbour, RoadNode> NodeMap { get; private set; } = [];
        public void RegisterNodeIfNeeded(Coordinate coord)
        {
            Field? field = World.Instance.GetField(coord);
            if (field?.Surface is not KnowsNeighbour surface) return;

            bool isStation = surface is Station;
            bool isJunction = surface is Road road && road.RoadType.HasFlag(RoadType.JUNCTION);

            if ((isStation || isJunction) && !NodeMap.ContainsKey(surface))
            {
                NodeMap.Add(surface, new RoadNode(coord, isStation));
                System.Diagnostics.Debug.WriteLine($"Siker: Node regisztrálva a gráfba: {coord} (Típus: {surface.GetType().Name})");
            }
        }
        public void RebuildEdges()
        {
            System.Diagnostics.Debug.WriteLine($"--- GRÁF ÉPÍTÉS INDUL (Csomópontok száma: {NodeMap.Count}) ---");

            if (NodeMap.Count < 2)
            {
                System.Diagnostics.Debug.WriteLine("HIBA: Nincs elég csomópont a gráfban! (Regisztráltad az állomásokat/kereszteződéseket?)");
                return;
            }

            foreach (KeyValuePair<KnowsNeighbour, RoadNode> kvp in NodeMap)
            {
                RoadNode node = kvp.Value;
                node.Edges.Clear();

                System.Diagnostics.Debug.WriteLine($"Vizsgálom a csomópontot: {node.Coordinate} (Típus: {kvp.Key.GetType().Name})");

                if (node.North?.Surface is KnowsNeighbour n && IsValidPathSurface(n)) ExploreAndConnect(node, n, "Észak");
                if (node.South?.Surface is KnowsNeighbour s && IsValidPathSurface(s)) ExploreAndConnect(node, s, "Dél");
                if (node.East?.Surface is KnowsNeighbour e && IsValidPathSurface(e)) ExploreAndConnect(node, e, "Kelet");
                if (node.West?.Surface is KnowsNeighbour w && IsValidPathSurface(w)) ExploreAndConnect(node, w, "Nyugat");

                System.Diagnostics.Debug.WriteLine($"  -> Kész. Talált élek száma: {node.Edges.Count}");
            }
        }

        private void ExploreAndConnect(RoadNode startNode, KnowsNeighbour currentSurface, string dirDebug)
        {
            int totalWeight = 0;
            List<Road> pathTaken = [];

            KnowsNeighbour? previousSurface = World.Instance.GetField(startNode.Coordinate)?.Surface as KnowsNeighbour;
            if (previousSurface == null) return;

            KnowsNeighbour? iterSurface = currentSurface;

            System.Diagnostics.Debug.WriteLine($"  Elindultam {dirDebug} felé a {currentSurface.Coordinate} koordinátán...");

            while (iterSurface != null)
            {

                if (iterSurface is Road r)
                {
                    pathTaken.Add(r);
                    totalWeight += r.IsSlope ? 2 : 1;
                }
                else if (iterSurface is Station)
                {
                    totalWeight += 1;
                }

                if (NodeMap.TryGetValue(iterSurface, out RoadNode? targetNode))
                {
                    startNode.Edges.Add(new RoadEdge(targetNode, totalWeight, pathTaken));
                    System.Diagnostics.Debug.WriteLine($"    [SIKER] Megtaláltam a célcsomópontot: {targetNode.Coordinate} (Táv: {totalWeight})");
                    return;
                }

                KnowsNeighbour? nextSurface = GetNextNeighbor(iterSurface, previousSurface);

                if (nextSurface == null)
                {
                    System.Diagnostics.Debug.WriteLine($"    [ZÁKUTCA] Elakadtam a {iterSurface.Coordinate} koordinátánál. (Típus: {iterSurface.GetType().Name})");
                }

                previousSurface = iterSurface;
                iterSurface = nextSurface;
            }
        }

        private KnowsNeighbour? GetNextNeighbor(KnowsNeighbour current, KnowsNeighbour cameFrom)
        {
            if (current.North?.Surface is KnowsNeighbour n && IsValidPathSurface(n) && n.Coordinate != cameFrom.Coordinate) return n;
            if (current.South?.Surface is KnowsNeighbour s && IsValidPathSurface(s) && s.Coordinate != cameFrom.Coordinate) return s;
            if (current.East?.Surface is KnowsNeighbour e && IsValidPathSurface(e) && e.Coordinate != cameFrom.Coordinate) return e;
            if (current.West?.Surface is KnowsNeighbour w && IsValidPathSurface(w) && w.Coordinate != cameFrom.Coordinate) return w;

            return null;
        }

        private bool IsValidPathSurface(ISurface? surface)
        {
            return surface is Road || surface is Station;
        }
    }
}
