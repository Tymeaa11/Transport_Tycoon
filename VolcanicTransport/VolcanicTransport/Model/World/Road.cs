using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Road(Coordinate coordinate) : KnowsNeighbour(coordinate)
    {
        #region Fields
        public bool IsReserved { get; set; } = false;
        public RoadType RoadType { get; protected set; }
        #endregion

        #region Methods

        public class FieldEventArgs(Coordinate coordinate) : EventArgs
        {
            public Coordinate Coordinate { get; } = coordinate;
        }

        public event EventHandler<FieldEventArgs>? RoadLayoutChanged;

        protected virtual void OnRoadLayoutChanged(Coordinate coord)
        {
            RoadLayoutChanged?.Invoke(this, new FieldEventArgs(coord));
        }
        public virtual void Update()
        {
            var thisField = World.Instance.GetField(Coordinate);
            if (thisField == null) return;

            var roadNorth = North?.Surface is Road;
            var roadSouth = South?.Surface is Road;
            var roadEast = East?.Surface is Road;
            var roadWest = West?.Surface is Road;

            int diffN = roadNorth ? thisField.GetHeightDifference(North) : 0;
            int diffS = roadSouth ? thisField.GetHeightDifference(South) : 0;
            int diffE = roadEast ? thisField.GetHeightDifference(East) : 0;
            int diffW = roadWest ? thisField.GetHeightDifference(West) : 0;

            if (Math.Abs(diffN) > 1 || Math.Abs(diffS) > 1 || Math.Abs(diffE) > 1 || Math.Abs(diffW) > 1)
            {
                RoadType = RoadType.INVALID;
                return;
            }

            RoadType baseType = 0;
            if (roadNorth) baseType |= RoadType.NORTH;
            if (roadSouth) baseType |= RoadType.SOUTH;
            if (roadEast) baseType |= RoadType.EAST;
            if (roadWest) baseType |= RoadType.WEST;

            var neighbourCount = CountSidesThatSatisfy((n) => n?.Surface is Road);

            int higherCount = 0;
            if (roadNorth && diffN == 1) higherCount++;
            if (roadSouth && diffS == 1) higherCount++;
            if (roadEast && diffE == 1) higherCount++;
            if (roadWest && diffW == 1) higherCount++;

            switch (neighbourCount)
            {
                case 0:
                    RoadType = RoadType.LONELY;
                    break;
                case 1:
                    RoadType = baseType;
                    if (higherCount == 1) RoadType |= RoadType.SLOPE;
                    break;

                // Straight or Curved
                case 2:
                    bool isStraight = (roadNorth && roadSouth) || (roadEast && roadWest);
                    if (isStraight)
                    {
                        RoadType = baseType | (higherCount == 1 ? RoadType.SLOPE : RoadType.STRAIGHT);
                    }
                    else
                    {
                        if (higherCount == 1)
                            RoadType = RoadType.INVALID;
                        else
                            RoadType = baseType | RoadType.CURVED;
                    }
                    break;

                // Junction
                case 3:
                case 4:
                    if (higherCount == 1)
                        RoadType = RoadType.INVALID;
                    else
                        RoadType = baseType | RoadType.JUNCTION; 
                    break;
            }

            if (neighbourCount >= 3)
            {
                OnRoadLayoutChanged(Coordinate);
            }

        }


        public bool IsStraight() => (RoadType & RoadType.STRAIGHT) != 0;
        public bool IsCurved() => (RoadType & RoadType.CURVED) != 0;
        public bool IsJunction() => (RoadType & RoadType.JUNCTION) != 0;
        public bool IsSlope() => (RoadType & RoadType.SLOPE) != 0;

        public event EventHandler? OnPlacementFailed;

        public bool TryUpdateNeighbours()
        {
            if (North?.Surface is Road northR)
            {
                northR.Update();
                if (northR.RoadType == RoadType.INVALID)
                {
                    return false;
                }
            }
            if (South?.Surface is Road southR)
            {
                southR.Update();
                if (southR.RoadType == RoadType.INVALID)
                {
                    return false;
                }
            }
            if (East?.Surface is Road eastR)
            {
                eastR.Update();
                if (eastR.RoadType == RoadType.INVALID)
                {
                    return false;
                }
            }
            if (West?.Surface is Road westR)
            {
                westR.Update();
                if (westR.RoadType == RoadType.INVALID)
                {
                    return false;
                }
            }
            return true;
        }

        public void UpdateNeighbours()
        {
            if (North?.Surface is Road northR) northR.Update();
            if (South?.Surface is Road southR) southR.Update();
            if (East?.Surface is Road eastR) eastR.Update();
            if (West?.Surface is Road westR) westR.Update();
        }



        #endregion
    }
}
