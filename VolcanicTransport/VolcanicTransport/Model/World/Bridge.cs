using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Bridge : Road
    {
        public float SpeedLimit { get; }

        public FieldType Elevation { get; }

        public Bridge(Coordinate coord, RoadType fixedType, float speedLimit, FieldType elevation) : base(coord)
        {
            RoadType = fixedType;
            SpeedLimit = speedLimit;
            Elevation = elevation;
        }

        public override void Update()
        {
        }
    }
}
