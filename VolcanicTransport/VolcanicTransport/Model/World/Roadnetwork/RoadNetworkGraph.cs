using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadNetworkGraph
    {
        public Dictionary<Field, RoadNode> NodeMap { get; private set; } = [];
        public void RegisterNodeIfNeeded(Field field)
        {
            if (field.Surface == null) return;

            bool isStation = field.Surface is Station;
            bool isJunction = field.Surface is Road road && road.RoadType.HasFlag(RoadType.JUNCTION);

            if ((isStation || isJunction) && !NodeMap.ContainsKey(field))
            {
                NodeMap.Add(field, new RoadNode(field, isStation));
                System.Diagnostics.Debug.WriteLine($"Siker: Node regisztrálva a gráfba: {field.Coordinate} (Típus: {field.Surface.GetType().Name})");
            }
        }
        public void RebuildEdges()
        {

            foreach (KeyValuePair<Field, RoadNode> kvp in NodeMap)
            {
                RoadNode node = kvp.Value;
                node.Edges.Clear();

                ExploreAndConnect(node, node.North);
                ExploreAndConnect(node, node.South);
                ExploreAndConnect(node, node.East);
                ExploreAndConnect(node, node.West);
            }
        }

        private void ExploreAndConnect(RoadNode startNode, Field? currentField)
        {
            if (currentField == null || currentField.Surface is not Road)
                return;

            int totalWeight = 0;
            List<Field> pathTaken = [];
            Field previousField = startNode.Field;

            while (currentField != null && currentField.Surface is Road road)
            {
                pathTaken.Add(currentField);
                totalWeight += road.IsSlope ? 2 : 1;
                if (NodeMap.TryGetValue(currentField, out RoadNode? targetNode))
                {
                    startNode.Edges.Add(new RoadEdge(targetNode, totalWeight, pathTaken));
                    return;
                }

                Field? nextField = GetNextRoadNeighbor(currentField, previousField);
                previousField = currentField;
                currentField = nextField;
            }
        }

        private Field? GetNextRoadNeighbor(Field current, Field cameFrom)
        {
            if (current.Surface is Road currentRoad)
            {
                if (currentRoad.RoadType.HasFlag(RoadType.NORTH) && currentRoad.North != cameFrom)
                    return currentRoad.North;
                if (currentRoad.RoadType.HasFlag(RoadType.SOUTH) && currentRoad.South != cameFrom)
                    return currentRoad.South;
                if (currentRoad.RoadType.HasFlag(RoadType.EAST) && currentRoad.East != cameFrom)
                    return currentRoad.East;
                if (currentRoad.RoadType.HasFlag(RoadType.WEST) && currentRoad.West != cameFrom)
                    return currentRoad.West;
            }

            return null;
        }
    }
}
