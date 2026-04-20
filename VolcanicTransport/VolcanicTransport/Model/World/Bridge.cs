using System.Text.Json.Serialization;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World
{

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(AshProducer), "ash")]
    [JsonDerivedType(typeof(BoneProducer), "bone")]
    [JsonDerivedType(typeof(MushroomProducer), "mushroom")]
    public abstract class Bridge(Coordinate coordinate, RoadType fixedType, FieldType elevation, GameSettings.BridgeData bridgeData) : Road(coordinate)
    {
        public override RoadType RoadType => fixedType;
        public float SpeedLimit => bridgeData.MaxSpeed;
        public FieldType Elevation { get; } = elevation;

        public override void Update() {}
    }

    public class BoneBridge(
        Coordinate coordinate,
        RoadType roadType, 
        FieldType elevation) 
    : Bridge(coordinate, roadType, elevation, GameSettings.BoneBridge) 
    {

    }

    public class StoneBridge(
        Coordinate coordinate,
        RoadType roadType,
        FieldType elevation)
    : Bridge(coordinate, roadType, elevation, GameSettings.StoneBridge)
    {

    }

    public class SteelBridge(
        Coordinate coordinate,
        RoadType roadType,
        FieldType elevation)
    : Bridge(coordinate, roadType, elevation, GameSettings.SteelBridge)
    {

    }
}
