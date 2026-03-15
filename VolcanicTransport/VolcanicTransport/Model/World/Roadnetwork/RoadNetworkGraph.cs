using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadNetworkGraph
    {
        public Dictionary<Field, RoadNode> NodeMap { get; private set; } = new Dictionary<Field, RoadNode>();
        public void RegisterNodeIfNeeded(Field field)
        {
            if (field.Surface is Road road)
            {
                bool isJunction = road.RoadType.HasFlag(RoadType.JUNCTION);
                bool isStation = field.HasStation;
                if ((isJunction || isStation) && !NodeMap.ContainsKey(field))
                {
                    NodeMap.Add(field, new RoadNode(field, isStation));
                }
            }
        }
        public void RebuildEdges()
        {

            foreach (KeyValuePair<Field,RoadNode> kvp in NodeMap)
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
            if (currentField == null || !(currentField.Surface is Road)) 
                return;

            int totalWeight = 0;
            List<Field> pathTaken = new List<Field>();
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
