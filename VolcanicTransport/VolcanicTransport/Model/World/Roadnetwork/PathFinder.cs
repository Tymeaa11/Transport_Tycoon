using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public static class Pathfinder
    {
        public static List<Field>? FindPath(RoadNode startNode, RoadNode targetNode)
        {
            if (startNode == targetNode)
                    return new List<Field>();

            List<RoadNode> openSet = new List<RoadNode> {startNode};
            HashSet<RoadNode> closedSet = new HashSet<RoadNode>();
            Dictionary<RoadNode, int> gScore = new Dictionary<RoadNode, int>
        {
            {startNode, 0}
        };

            Dictionary<RoadNode, int> fScore = new Dictionary<RoadNode, int>
        {
            {startNode, GetHeuristicDistance(startNode, targetNode)}
        };

            Dictionary<RoadNode, PathTrace> cameFrom = new Dictionary<RoadNode, PathTrace>();

            while (openSet.Count > 0)
            {
                RoadNode current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : int.MaxValue).First();

                if (current == targetNode)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (RoadEdge edge in current.Edges)
                {
                    RoadNode neighbor = edge.TargetNode;

                    if (closedSet.Contains(neighbor)) 
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
            return Math.Abs(a.Field.Coordinate.X - b.Field.Coordinate.X) +
                   Math.Abs(a.Field.Coordinate.Y - b.Field.Coordinate.Y);
        }
        private static List<Field> ReconstructPath(Dictionary<RoadNode, PathTrace> cameFrom, RoadNode current)
        {
            List<List<Field>> pathSegments = new List<List<Field>>();

            while (cameFrom.ContainsKey(current))
            {
                PathTrace trace = cameFrom[current];
                pathSegments.Add(trace.TakenEdge.Path);
                current = trace.ParentNode;
            }

            pathSegments.Reverse();

            List<Field> finalPath = new List<Field>();
            foreach (var segment in pathSegments)
            {
                finalPath.AddRange(segment);
            }

            return finalPath;
        }
        private class PathTrace
        {
            public RoadNode ParentNode { get; }
            public RoadEdge TakenEdge { get; }

            public PathTrace(RoadNode parent, RoadEdge edge)
            {
                ParentNode = parent;
                TakenEdge = edge;
            }
        }
    }
}
