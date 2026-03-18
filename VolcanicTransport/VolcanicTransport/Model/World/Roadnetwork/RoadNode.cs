namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadNode(Field field, bool isStation = false) : Road(field.Coordinate)
    {
        public bool IsStation { get; set; } = isStation;
        public List<RoadEdge> Edges { get; set; } = [];
    }
}
