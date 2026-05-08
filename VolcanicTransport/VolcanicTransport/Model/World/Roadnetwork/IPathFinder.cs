namespace VolcanicTransport.Model.World.Roadnetwork
{
    public interface IPathFinder
    {
        List<Road>? FindPath(RoadNode startNode, RoadNode targetNode);
    }
}
