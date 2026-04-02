using System.Collections.Immutable;
using VolcanicTransport.Model.World;

namespace VolcanicTransport;

public static class GameSettings
{
    public const string GameName = "Volcanic Transport";
    public const string GameVersion = "0.1.0";

    public const int FieldSize = 64;
    public const int ChunkSize = 32;

    private static readonly Dictionary<FieldType, float>  FieldTypeThickness = new()
    {
        [FieldType.DEEP_LAVA_OCEAN] = 20, // -inf to 20
        [FieldType.LAVA_OCEAN] = 60, // 20 to 20 + 60
        [FieldType.BEACH] = 20, // 20 + 60 to 20 + 60 + 20
        [FieldType.LOW_LANDS] = 90,
        [FieldType.LOW_MID_TRANSITION] = 10,
        [FieldType.MID_LANDS] = 90,
        [FieldType.MID_HIGH_TRANSITION] = 10,
        [FieldType.HIGH_LANDS] = 100,
        [FieldType.MOUNTAINS] = 120,
        [FieldType.HIGH_MOUNTAINS] = 99999999, // from x to ~+inf
    };

    public static readonly ImmutableArray<float> MaxFieldTypeHeights;

    
    
    static GameSettings()
    {
        var tempArray = new float[FieldTypeThickness.Count];

        var height = FieldTypeThickness[FieldType.DEEP_LAVA_OCEAN];
        
        for (var i = 1; i < FieldTypeThickness.Count; i++)
        {
            tempArray[i - 1] = height;
            height += FieldTypeThickness.Values.ElementAt(i);
        }
        tempArray[FieldTypeThickness.Count - 1] = height;
        
        MaxFieldTypeHeights = [..tempArray];
    }
}