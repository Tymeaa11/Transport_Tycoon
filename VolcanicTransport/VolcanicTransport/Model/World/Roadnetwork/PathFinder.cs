namespace VolcanicTransport.Model.World.Roadnetwork
{
    public static class Pathfinder
    {
        public static List<Road>? FindPath(RoadNode startNode, RoadNode targetNode)
        {
            if (startNode == targetNode)
                return [];

            List<RoadNode> openSet = [startNode];
            HashSet<RoadNode> closedSet = [];
            Dictionary<RoadNode, int> gScore = new()
            {
                {startNode, 0}
            };

            Dictionary<RoadNode, int> fScore = new()
            {
                {startNode, GetHeuristicDistance(startNode, targetNode)}
            };

            Dictionary<RoadNode, PathTrace> cameFrom = [];

            while (openSet.Count > 0)
            {
                RoadNode current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : int.MaxValue).First();

                if (current == targetNode)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (RoadEdge edge in current.Edges)
                {
                    RoadNode neighbor = edge.TargetNode;

                    if (closedSet.Contains(neighbor))
                        continue;

                    if (neighbor.IsStation && neighbor != targetNode)
                        continue;

                    int tentativeGScore = gScore[current] + edge.Weight;

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = new PathTrace(current, edge);

                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + GetHeuristicDistance(neighbor, targetNode);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
            return null;
        }

        private static int GetHeuristicDistance(RoadNode a, RoadNode b)
        {
            return Math.Abs(a.Coordinate.X - b.Coordinate.X) +
                   Math.Abs(a.Coordinate.Y - b.Coordinate.Y);
        }

        private static List<Road> ReconstructPath(Dictionary<RoadNode, PathTrace> cameFrom, RoadNode current)
        {
            List<List<Road>> pathSegments = [];

            while (cameFrom.ContainsKey(current))
            {
                PathTrace trace = cameFrom[current];
                pathSegments.Add(trace.TakenEdge.Path);
                current = trace.ParentNode;
            }

            pathSegments.Reverse();

            List<Road> finalPath = [];
            foreach (var segment in pathSegments)
            {
                finalPath.AddRange(segment);
            }

            return finalPath;
        }

        private class PathTrace(RoadNode parent, RoadEdge edge)
        {
            public RoadNode ParentNode { get; } = parent;
            public RoadEdge TakenEdge { get; } = edge;
        }
    }
}