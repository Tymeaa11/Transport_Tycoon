using System.Text.Json.Serialization;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public abstract class KnowsNeighbour : ISurface
    {
        public Coordinate Coordinate { get; }

        [JsonIgnore] public Field? North { get; private set; }
        [JsonIgnore] public Field? South { get; private set; }
        [JsonIgnore] public Field? East { get; private set; }
        [JsonIgnore] public Field? West { get; private set; }

        protected KnowsNeighbour(Coordinate coordinate)
        {
            Coordinate = coordinate;
            North = South = East = West = null;
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
            if (!World.IsInitialised()) 
                return;

            North = World.Instance.GetField(Coordinate + Coordinate.North);
            South = World.Instance.GetField(Coordinate + Coordinate.South);
            East = World.Instance.GetField(Coordinate + Coordinate.East);
            West = World.Instance.GetField(Coordinate + Coordinate.West);
        }


    }
}
