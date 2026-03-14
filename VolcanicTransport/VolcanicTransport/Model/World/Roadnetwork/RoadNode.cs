namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadNode : Road
    {
        public Field Field { get; set; }
        public bool IsStation { get; set; }
        public List<RoadEdge> Edges { get; set; } = new List<RoadEdge>();

        public RoadNode(Field field, bool isStation = false) : base(field.Coordinate) 
        {
            Field = field;
            IsStation = isStation;
        }
    }
}
