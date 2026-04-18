using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Bridge : Road
    {
        public float SpeedLimit { get; }

        public Bridge(Coordinate coord, RoadType fixedType, float speedLimit) : base(coord)
        {
            RoadType = fixedType;
            SpeedLimit = speedLimit;
        }

        public override void Update()
        {
        }
    }
}
