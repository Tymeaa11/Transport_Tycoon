using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class RoadNode(Coordinate coord, bool isStation = false) : KnowsNeighbour(coord)
    {
        public bool IsStation { get; set; } = isStation;
        public List<RoadEdge> Edges { get; set; } = [];
    }
}
