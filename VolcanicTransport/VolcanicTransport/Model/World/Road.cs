using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Road(Coordinate coordinate) : KnowsNeighbour(coordinate)
    {
        #region fields
        public bool IsPermanent { get; }
        public RoadType RoadType { get; private set; }

        #endregion
        #region constructors
        //public Road(Coordinate coordinate, bool isPermanent) : base(coordinate)
        //    => IsPermanent = isPermanent;
        // Nemtudom kelleni fog-e ez : VR
        #endregion

        #region methods

        public class FieldEventArgs(Field field) : EventArgs
        {
            public Field Field { get; } = field;
        }

        public event EventHandler<FieldEventArgs>? RoadLayoutChanged;

        protected virtual void OnRoadLayoutChanged(Field field)
        {
            RoadLayoutChanged?.Invoke(this, new FieldEventArgs(field));
        } //TODO feliratkozni az eseményre
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
            var heightDiffEast = thisField?.GetHeightDifference(East) ?? 0;
            var heightDiffWest = thisField?.GetHeightDifference(West) ?? 0;

            if ((RoadType & RoadType.STRAIGHT) != 0)
            {
                RoadType |= RoadType.SLOPE;
                return;
            }

            // No neightbour can be heigher if curved or junction
            if (heightDiffNorth < 0 || heightDiffSouth < 0 || heightDiffEast < 0 || heightDiffWest < 0)
            {
                RoadType = RoadType.INVALID;
                return;
            }

            // heights can differ only by 1 if curved or junction
            if (heightDiffNorth <= 1 && heightDiffSouth <= 1 && heightDiffEast <= 1 && heightDiffWest <= 1)
            {
                RoadType = RoadType.INVALID;
                return;
            }

            if (RoadType.JUNCTION == RoadType)
            {
                OnRoadLayoutChanged(Field);
            }

        }


        public bool IsStraight => (RoadType & RoadType.STRAIGHT) != 0;
        public bool IsCurved => (RoadType & RoadType.CURVED) != 0;
        public bool IsJunction => (RoadType & RoadType.JUNCTION) != 0;
        public bool IsSlope => (RoadType & RoadType.SLOPE) != 0;

        public event EventHandler? onPlacementFailed;
        private Road? CanPlaceRoadHere(Field field)
        {
            if (field == null)
                return null;

            if (!field.IsBuildable())
                return null;

            //creating a temporal to see if a road can be place here
            Road tempRoad = new(field.Coordinate);
            field.Surface = tempRoad;
            tempRoad.Update();
            field.Surface = null;

            if (RoadType == RoadType.INVALID)
            {
                onPlacementFailed?.Invoke(this, EventArgs.Empty);
                return null;
            }


            return tempRoad;
        }

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
            if (North?.Surface is Road southR)
            {
                southR.Update();
                if (southR.RoadType == RoadType.INVALID)
                {
                    return false;
                }
            }
            if (North?.Surface is Road eastR)
            {
                eastR.Update();
                if (eastR.RoadType == RoadType.INVALID)
                {
                    return false;
                }
            }
            if (North?.Surface is Road westR)
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

        public void PlaceRoad(Field field)
        {
            Road? road = CanPlaceRoadHere(field);
            if (road == null) return;
            field.Surface = road;
            if (!road.TryUpdateNeighbours())
            {
                field.Surface = null;
                road.UpdateNeighbours();
                onPlacementFailed?.Invoke(this, EventArgs.Empty);
            }
        }


        #endregion
    }
}
