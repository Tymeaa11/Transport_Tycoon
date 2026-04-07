using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public abstract class KnowsNeighbour : ISurface
    {
        public Coordinate Coordinate { get; }

        public Field? North { get; private set; }
        public Field? South { get; private set; }
        public Field? East { get; private set; }
        public Field? West { get; private set; }

        protected KnowsNeighbour(Coordinate coordinate)
        {
            Coordinate = coordinate;
            North = South =  East = West = null;
            UpdateNeighbourReferences();
        }

        protected int CountSidesThatSatisfy(Predicate<Field?> predicate)
        {
            var count = 0;
            count += predicate(North) ? 1 : 0;
            count += predicate(South) ? 1 : 0;
            count += predicate(East) ? 1 : 0;
            count += predicate(West) ? 1 : 0;
            return count;
        }

        public void UpdateNeighbourReferences()
        {
            North = World.Instance.GetField(Coordinate + Direction.North);
            South = World.Instance.GetField(Coordinate + Direction.South);
            East = World.Instance.GetField(Coordinate + Direction.East);
            West = World.Instance.GetField(Coordinate + Direction.West);
        }


    }
}
