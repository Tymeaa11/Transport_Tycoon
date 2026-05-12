namespace VolcanicTransport.Model.Utils
{
    public class FieldsEventArgs(List<Coordinate> coordinates) : EventArgs
    {
        public List<Coordinate> ChangedCoordinates { get; } = coordinates;
    }
}
