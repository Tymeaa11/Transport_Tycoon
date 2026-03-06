using Transport.Model.Utils;

namespace Transport.Model.World
{
    public class Road :  KnowsNeighbour
    {
        #region fields
        public bool IsPermanent { get; }
        public RoadType RoadType { get; private set; }
        #endregion
        
        #region constructors
        public Road(Coordinate coordinate) : base(coordinate) {}
        public Road(Coordinate coordinate, bool isPermanent) : base(coordinate)
            => IsPermanent = isPermanent;
        #endregion

        #region methods
        public void Update()
        {
            var roadNorth = North?.Surface is Road;
            var roadSouth = South?.Surface is Road;
            var roadEast = East?.Surface is Road;
            var roadWest = West?.Surface is Road;

            RoadType = 0;
            var neighbourCount = CountSidesThatSatisfy((n) => n?.Surface is Road);
            if (roadNorth) RoadType |= RoadType.NORTH;
            if (roadSouth) RoadType |= RoadType.SOUTH;
            if (roadEast) RoadType |= RoadType.EAST;
            if (roadWest) RoadType |= RoadType.WEST;

            switch (neighbourCount)
            {
                // Lonely road or end piece
                case 0:
                case 1: 
                    return; 
                
                // Straight or Curved
                case 2: 
                    if (roadNorth == roadSouth && roadNorth || roadEast == roadWest && roadEast)
                        RoadType |= RoadType.STRAIGHT;
                    else
                        RoadType |= RoadType.CURVED;
                    break;
                
                // Junction
                case 3: 
                case 4: 
                    RoadType |= RoadType.JUNCTION; 
                    break;
                
            }
            
            
            var thisField = World.Instance.GetField(Coordinate);
            
            var heightDiffNorth = thisField?.GetHeightDifference(North) ?? 0;
            var heightDiffSouth = thisField?.GetHeightDifference(South) ?? 0;
            var heightDiffEast  = thisField?.GetHeightDifference(East)  ?? 0;
            var heightDiffWest  = thisField?.GetHeightDifference(West)  ?? 0;

            if ((RoadType & RoadType.STRAIGHT) != 0)
            {
                RoadType |= RoadType.SLOPE;
                return;
            }
            
            // No neightbour can be heigher if curved or junction
            if (heightDiffNorth < 0 || heightDiffSouth < 0 || heightDiffEast < 0 || heightDiffWest < 0)
                RoadType = RoadType.INVALID;
            
            // heights can differ only by 1 if curved or junction
            if (heightDiffNorth <= 1 && heightDiffSouth <= 1 && heightDiffEast <= 1 && heightDiffWest <= 1)
                RoadType = RoadType.INVALID;
                
        }

        public bool IsStraight => (RoadType & RoadType.STRAIGHT) != 0;
        public bool IsCurved   => (RoadType & RoadType.CURVED) != 0;
        public bool IsJunction => (RoadType & RoadType.JUNCTION) != 0;
        public bool IsSlope    => (RoadType & RoadType.SLOPE) != 0;
        
        #endregion
    }
}
