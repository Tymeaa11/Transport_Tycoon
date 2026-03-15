namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadEdge
    {
        public RoadNode TargetNode { get; set; }
        public int Weight { get; set; }
        public List<Field> Path { get; set; }

        public RoadEdge(RoadNode target, int weight, List<Field> path)
        {
            TargetNode = target;
            Weight = weight;
            Path = path;
        }
    }
}
