namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadEdge(RoadNode target, int weight, List<Road> path)
    {
        public RoadNode TargetNode { get; set; } = target;
        public int Weight { get; set; } = weight;
        public List<Road> Path { get; set; } = path;
    }
}
