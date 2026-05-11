using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public interface IRoadNetworkGraph
    {
        Dictionary<KnowsNeighbour, RoadNode> NodeMap { get; }
        void RegisterNodeIfNeeded(Coordinate coord);
        void RebuildEdges();
    }
}
