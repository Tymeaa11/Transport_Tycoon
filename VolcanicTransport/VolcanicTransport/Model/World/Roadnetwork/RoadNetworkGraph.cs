using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadNetworkGraph : IRoadNetworkGraph
    {
        public Dictionary<KnowsNeighbour, RoadNode> NodeMap { get; } = [];
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
                RebuildEdges();
            }
        }
        public void RebuildEdges()
        {
            System.Diagnostics.Debug.WriteLine($"--- GRÁF ÉPÍTÉS INDUL (Csomópontok száma: {NodeMap.Count}) ---");

            if (NodeMap.Count < 2) return;

            foreach (KeyValuePair<KnowsNeighbour, RoadNode> kvp in NodeMap)
            {
                RoadNode node = kvp.Value;
                node.Edges.Clear();

                System.Diagnostics.Debug.WriteLine($"Vizsgálom a csomópontot: {node.Coordinate} (Típus: {kvp.Key.GetType().Name})");

                if (node.North?.Surface is KnowsNeighbour n && IsValidPathSurface(n)) StartTrace(node, n, kvp.Key);
                if (node.South?.Surface is KnowsNeighbour s && IsValidPathSurface(s)) StartTrace(node, s, kvp.Key);
                if (node.East?.Surface is KnowsNeighbour e && IsValidPathSurface(e)) StartTrace(node, e, kvp.Key);
                if (node.West?.Surface is KnowsNeighbour w && IsValidPathSurface(w)) StartTrace(node, w, kvp.Key);
            }
        }

        private void StartTrace(RoadNode startNode, KnowsNeighbour firstStep, KnowsNeighbour startSurface)
        {
            Trace(startNode, firstStep, startSurface, [], 0, []);
        }

        private void Trace(RoadNode startNode, KnowsNeighbour current, KnowsNeighbour cameFrom, List<Road> currentPath, int currentWeight, HashSet<Coordinate> visited)
        {
            if (!visited.Add(current.Coordinate)) return;

            if (NodeMap.TryGetValue(current, out RoadNode? targetNode))
            {
                if (current is Road r && current is not Station)
                {
                    currentPath.Add(r);
                    currentWeight += r.IsSlope() ? 2 : 1;
                }
                else if (current is Station) currentWeight += 1;

                startNode.Edges.Add(new RoadEdge(targetNode, currentWeight, [.. currentPath]));
                return;
            }

            if (current is Road currentRoad)
            {
                currentPath.Add(currentRoad);
                currentWeight += currentRoad.IsSlope() ? 2 : 1;

                CheckAndAddAdjacentStation(startNode, currentRoad.North?.Surface, currentPath, currentWeight);
                CheckAndAddAdjacentStation(startNode, currentRoad.South?.Surface, currentPath, currentWeight);
                CheckAndAddAdjacentStation(startNode, currentRoad.East?.Surface, currentPath, currentWeight);
                CheckAndAddAdjacentStation(startNode, currentRoad.West?.Surface, currentPath, currentWeight);

                if (currentRoad.North?.Surface is Road rN && rN != cameFrom) Trace(startNode, rN, current, [.. currentPath], currentWeight, [.. visited]);
                if (currentRoad.South?.Surface is Road rS && rS != cameFrom) Trace(startNode, rS, current, [.. currentPath], currentWeight, [.. visited]);
                if (currentRoad.East?.Surface is Road rE && rE != cameFrom) Trace(startNode, rE, current, [.. currentPath], currentWeight, [.. visited]);
                if (currentRoad.West?.Surface is Road rW && rW != cameFrom) Trace(startNode, rW, current, [.. currentPath], currentWeight, [.. visited]);
            }
        }

        private void CheckAndAddAdjacentStation(RoadNode startNode, ISurface? adjacentSurface, List<Road> currentPath, int weight)
        {
            if (adjacentSurface is Station st && st.Coordinate != startNode.Coordinate)
            {
                if (NodeMap.TryGetValue(st, out RoadNode? stationNode))
                {
                    startNode.Edges.Add(new RoadEdge(stationNode, weight + 1, [.. currentPath]));
                    System.Diagnostics.Debug.WriteLine($"    [REJTETT ÁLLOMÁS MEGTALÁLVA] Út bejegyezve: {startNode.Coordinate} -> {stationNode.Coordinate}");
                }
            }
        }

        private bool IsValidPathSurface(ISurface? surface)
        {
            return surface is Road or Station;
        }
    }
}
