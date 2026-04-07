using System.Text.Json.Serialization;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Road), typeDiscriminator: "road")]
    [JsonDerivedType(typeof(Mushroom), typeDiscriminator: "mushroom")]
    [JsonDerivedType(typeof(BridgePart), typeDiscriminator: "bridge")]
    [JsonDerivedType(typeof(CityBuilding), typeDiscriminator: "city_building")]
    [JsonDerivedType(typeof(CityStation), typeDiscriminator: "city_station")]
    [JsonDerivedType(typeof(FactoryBuilding), typeDiscriminator: "factory_building")]
    [JsonDerivedType(typeof(FactoryStation), typeDiscriminator: "factory_station")]
    public interface ISurface { }
}
