using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model
{
    public class ChunkUpdatedEventArgs(Coordinate coordinate) : EventArgs
    {
        public Coordinate ChunkCoordinate { get; set; } = coordinate;
    }
}
