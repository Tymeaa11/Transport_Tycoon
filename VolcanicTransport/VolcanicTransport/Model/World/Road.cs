using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Road(Coordinate coordinate) : KnowsNeighbour(coordinate)
    {
        #region Fields
        public bool IsReserved { get; set; } = false;
        public RoadType RoadType { get; private set; }
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
        } //TODO feliratkozni az eseményre
        public void Update()
        {
            var heightDiffNorth = 0;
            var heightDiffSouth = 0;
            var heightDiffEast = 0;
            var heightDiffWest = 0;

            var thisField = World.Instance.GetField(Coordinate);

            var roadNorth = North?.Surface is Road;
            var roadSouth = South?.Surface is Road;
            var roadEast = East?.Surface is Road;
            var roadWest = West?.Surface is Road;

            RoadType = 0;
            var neighbourCount = CountSidesThatSatisfy((n) => n?.Surface is Road);
            if (roadNorth)
            {
                RoadType |= RoadType.NORTH;
                heightDiffNorth = thisField?.GetHeightDifference(North) ?? 0;
            }
            if (roadSouth)
            {
                RoadType |= RoadType.SOUTH;
                heightDiffSouth = thisField?.GetHeightDifference(South) ?? 0;
            }
            if (roadEast)
            {
                RoadType |= RoadType.EAST;
                heightDiffEast = thisField?.GetHeightDifference(East) ?? 0;
            }
            if (roadWest)
            {
                RoadType |= RoadType.WEST;
                heightDiffWest = thisField?.GetHeightDifference(West) ?? 0;
            }

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

            if ((RoadType & RoadType.STRAIGHT) != 0)
            {
                RoadType |= RoadType.SLOPE;
                return;
            }

            // No neighbor can be higher if curved or junction
            if (heightDiffNorth < 0 || heightDiffSouth < 0 || heightDiffEast < 0 || heightDiffWest < 0)
            {
                RoadType = RoadType.INVALID;
                return;
            }

            // heights can differ only by 1 if curved or junction
            if (heightDiffNorth > 1 || heightDiffSouth > 1 || heightDiffEast > 1 || heightDiffWest > 1)
            {
                RoadType = RoadType.INVALID;
                return;
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

        //public event EventHandler? OnPlacementFailed;

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
