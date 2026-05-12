namespace VolcanicTransport.Model.Utils
{
    public class ChunkUpdatedEventArgs(Coordinate coordinate) : EventArgs
    {
        public Coordinate ChunkCoordinate { get; set; } = coordinate;
    }
}
