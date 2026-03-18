namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadEdge(RoadNode target, int weight, List<Field> path)
    {
        public RoadNode TargetNode { get; set; } = target;
        public int Weight { get; set; } = weight;
        public List<Field> Path { get; set; } = path;
    }
}
