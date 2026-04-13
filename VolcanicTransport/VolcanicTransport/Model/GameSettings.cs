using System.Collections.Immutable;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model;

public static class GameSettings
{
    public const string GameName = "Volcanic Transport";
    public const string GameVersion = "0.1.0";

    #region World & WorldGeneration
    public const int FieldSize = 32; // should be divisible by 8
    public const int ChunkSize = 32; // should be even

    public const int FieldSizeP2 = FieldSize / 2;
    public const int FieldSizeP4 = FieldSize / 4;
    public const int FieldSizeP8 = FieldSize / 8;

    public const int ChunkSizeInPixels = ChunkSize * FieldSize;

    private static readonly Dictionary<FieldType, float> FieldTypeThickness = new()
    {
        [FieldType.DEEP_LAVA_OCEAN] = 20, // -inf to 20
        [FieldType.LAVA_OCEAN] = 60,
        [FieldType.BEACH] = 20,
        [FieldType.LOW_LANDS] = 90,
        [FieldType.LOW_MID_TRANSITION] = 10,
        [FieldType.MID_LANDS] = 90,
        [FieldType.MID_HIGH_TRANSITION] = 10,
        [FieldType.HIGH_LANDS] = 100,
        [FieldType.MOUNTAINS] = 120,
        [FieldType.HIGH_MOUNTAINS] = 99999999, // from the last to ~+inf
    };

    public static readonly ImmutableArray<float> MaxFieldTypeHeights;

    #region FactoryAndCityGeneration
    // Városok és gyárak közötti minimális távolság mezőkben
    public const double MinimumDistance = 15.0;

    // Keresési próbálkozások száma
    public const int MaxAttempts = 100;

    // Minimum távolság a világ szélétől
    public const int WorldEdgeBufferZone = 2;
    #endregion

    #region Mushrooms

    public const int SpreadChance = 50; // 0-100 %

    #region MushroomGeneration
    public const float Stage0MinHeight = 0.5f;
    public const float Stage1MinHeight = 0.55f;
    public const float Stage2MinHeight = 0.6f;
    public const float Stage3MinHeight = 0.65f;
    #endregion

    #endregion
    #endregion

    #region Economy
    #region FactoryData
    public readonly record struct FactoryData(
        ProductType BaseProduct,
        Product FinalProduct,
        int BaseProductBufferCapacity,
        int FinalProductBufferCapacity
    )
    { }

    public static readonly FactoryData AshProducerFactoryData = new(
        BaseProduct: ProductType.NONE,
        FinalProduct: new Product(ProductType.ASH, 0, 100, 5),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
        );

    public static readonly FactoryData BoneProducerFactoryData = new(
        BaseProduct: ProductType.NONE,
        FinalProduct: new Product(ProductType.BONE, 0, 100, 5),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData MushroomProducerFactoryData = new(
        BaseProduct: ProductType.NONE,
        FinalProduct: new Product(ProductType.MUSHROOM, 0, 100, 5),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData SteamProducerFactoryData = new(
        BaseProduct: ProductType.NONE,
        FinalProduct: new Product(ProductType.STEAM, 0, 100, 5),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData SulfurProducerFactoryData = new(
        BaseProduct: ProductType.NONE,
        FinalProduct: new Product(ProductType.SULFUR, 0, 100, 5),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData ConcreteFactoryData = new(
        BaseProduct: ProductType.ASH,
        FinalProduct: new Product(ProductType.CONCRETE, 0, 100, 5),
        BaseProductBufferCapacity: 5000,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData CondensatorFactoryData = new(
        BaseProduct: ProductType.STEAM,
        FinalProduct: new Product(ProductType.WATER, 0, 100, 5),
        BaseProductBufferCapacity: 5000,
        FinalProductBufferCapacity: 5000
    );
    #endregion

    #region Vehicles
    public readonly record struct VehicleData(
        float MaxSpeed,
        int Capacity,
        int Price)
    { }

    public static readonly VehicleData BusData = new(
        MaxSpeed: 2.0f,
        Capacity: 50,
         Price: 4000
    );

    public static readonly VehicleData MiniBusData = new(
        MaxSpeed: 100.0f,
        Capacity: 15,
        Price: 6000
    );

    public static readonly VehicleData TankerTruckData = new(
        MaxSpeed: 60.0f,
        Capacity: 800,
        Price: 10000
    );

    public static readonly VehicleData CargoTruckData = new(
        MaxSpeed: 70.0f,
        Capacity: 900,
        Price: 11000
    );

    #endregion
    #endregion


    #region GameplayConstants

    public const int StartingMoney = 10_000;
    public const double BaseRoadPrice = 100;
    public const double BaseStationPrice = 500;
    public const double BaseTerraformationPrice = 500;
    
    public const double MushroomPricePerUnit = 200;

    #endregion
    
    static GameSettings()
    {

        if (ChunkSize % 2 != 0) throw new Exception("ChunkSize must be even.");
        if (FieldSize % 8 != 0) throw new Exception("FieldSize must be divisible by 8.");

        var tempArray = new float[FieldTypeThickness.Count];

        var height = FieldTypeThickness[FieldType.DEEP_LAVA_OCEAN];

        for (var i = 1; i < FieldTypeThickness.Count; i++)
        {
            tempArray[i - 1] = height;
            height += FieldTypeThickness.Values.ElementAt(i);
        }
        tempArray[FieldTypeThickness.Count - 1] = height;

        MaxFieldTypeHeights = [.. tempArray];
    }
}