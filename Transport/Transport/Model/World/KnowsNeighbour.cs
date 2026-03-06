using Transport.Model.Utils;

namespace Transport.Model.World
{
    public abstract class KnowsNeighbour : ISurface
    {
        public Coordinate Coordinate { get; }
        public Field? North { get; }
        public Field? South { get; }
        public Field? East { get; }
        public Field? West { get; }
        
        protected KnowsNeighbour(Coordinate coordinate)
        {
            Coordinate = coordinate;
            
            North = World.Instance.GetField(Coordinate + Direction.North); 
            South = World.Instance.GetField(Coordinate + Direction.South); 
            East  = World.Instance.GetField(Coordinate + Direction.East); 
            West  = World.Instance.GetField(Coordinate + Direction.West); 
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
        
        
    }
}
