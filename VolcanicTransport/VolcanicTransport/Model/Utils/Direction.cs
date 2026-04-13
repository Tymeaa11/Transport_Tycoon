namespace VolcanicTransport.Model.Utils
{
    public static class Direction
    {
        public static readonly Coordinate North = new(0, -1);
        public static readonly Coordinate South = new(0, 1);
        public static readonly Coordinate East = new(1, 0);
        public static readonly Coordinate West = new(-1, 0);

        public static readonly Coordinate[] Directions = [North, South, East, West];
    }
}
