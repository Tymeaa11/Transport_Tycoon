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
    public const int MiniFieldSize = 4; // used for minimap
    public const int ChunkSize = 32; // should be even

    public const int DefaultWorldSize = 8;

    public const int FieldSizeP2 = FieldSize / 2;
    public const int FieldSizeP4 = FieldSize / 4;
    public const int FieldSizeP8 = FieldSize / 8;

    public const int ChunkSizeInPixels = ChunkSize * FieldSize;
    public const int MiniChunkSizeInPixels = ChunkSize * MiniFieldSize;

    public const int WorldSizeInFields = DefaultWorldSize * ChunkSize;

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
    public const double MinimumDistanceInFields = 15.0;

    // Keresési próbálkozások száma
    public const int MaxAttempts = 500;

    // Minimum távolság a világ szélétől
    public const int WorldEdgeBufferZone = 2;

    public const int CityCount = 10;
    public const int FactoryCount = 20;

    #endregion

    #region Mushrooms

    public const int SpreadChance = 50;
    public const double GrowthBaseChance = 0.2;
    public const double SpreadBaseChance = 0.2;
    public const double NewSpreadChance = 0.01;
    public const int SamplesCount = 250;

    #region MushroomGeneration 
    public const float MushroomPerlinFrequency = 0.07f;

    // Must be in increasing order
    public const float Stage0MinHeight = 0.615f;
    public const float Stage1MinHeight = 0.62f;
    public const float Stage2MinHeight = 0.63f;
    public const float Stage3MinHeight = 0.65f;
    #endregion

    #endregion
    #endregion

    #region Economy

    private static readonly Dictionary<ProductType, double> productPrices = new()
    {
        { ProductType.HUMAN, 15.0 },
        { ProductType.ASH, 10.0 },
        { ProductType.SULFUR, 20.0 },
        { ProductType.STEAM, 60.0 },
        { ProductType.WATER, 50.0 },
        { ProductType.BONE, 30.0 },
        { ProductType.CONCRETE, 70.0 },
        { ProductType.MUSHROOM, 25.0 },
        { ProductType.NONE, 0.0 }
    };

    public static double GetPrice(ProductType type) => productPrices.GetValueOrDefault(type, 0.0);

    public const double BaseProductionRate = 0.2;
    public const double PeopleGrowthRate = 0.01;
    public const double ChanceToUnboard = 0.5;

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
        FinalProduct: new Product(ProductType.ASH, 5, 20, 0, 0.02f),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
        );

    public static readonly FactoryData BoneProducerFactoryData = new(
        BaseProduct: ProductType.NONE,
        FinalProduct: new Product(ProductType.BONE, 100, 200, 0, 0.08f),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData MushroomProducerFactoryData = new(
        BaseProduct: ProductType.NONE,
        FinalProduct: new Product(ProductType.MUSHROOM, 50, 120, 0, 0.15f),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData SteamProducerFactoryData = new(
        BaseProduct: ProductType.NONE,
        FinalProduct: new Product(ProductType.STEAM, 150, 250, 0, 0.5f),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData SulfurProducerFactoryData = new(
        BaseProduct: ProductType.NONE,
        FinalProduct: new Product(ProductType.SULFUR, 30, 60, 0, 0.4f),
        BaseProductBufferCapacity: 0,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData ConcreteFactoryData = new(
        BaseProduct: ProductType.ASH,
        FinalProduct: new Product(ProductType.CONCRETE, 25, 400, 0, 0.03f),
        BaseProductBufferCapacity: 5000,
        FinalProductBufferCapacity: 5000
    );

    public static readonly FactoryData CondensatorFactoryData = new(
        BaseProduct: ProductType.STEAM,
        FinalProduct: new Product(ProductType.WATER, 40, 80, 0, 0.06f),
        BaseProductBufferCapacity: 5000,
        FinalProductBufferCapacity: 5000
    );
    #endregion

    #region Vehicles
    public readonly record struct VehicleData(
        List<ProductType> ProductTypes,
        float MaxSpeed,
        int Capacity,
        int Price)
    { }

    public static readonly VehicleData BusData = new(
        ProductTypes: [ProductType.HUMAN],
        MaxSpeed: 50.0f,
        Capacity: 30,
         Price: 6000
    );

    public static readonly VehicleData MiniBusData = new(
        ProductTypes: [ProductType.HUMAN],
        MaxSpeed: 80.0f,
        Capacity: 12,
        Price: 4000
    );

    public static readonly VehicleData TankerTruckData = new(
        ProductTypes: [ProductType.STEAM, ProductType.WATER, ProductType.CONCRETE],
        MaxSpeed: 60.0f,
        Capacity: 80,
        Price: 10000
    );

    public static readonly VehicleData MiniTankerTruckData = new(
    ProductTypes: [ProductType.STEAM, ProductType.WATER, ProductType.CONCRETE],
    MaxSpeed: 70.0f,
    Capacity: 40,
    Price: 7000
);

    public static readonly VehicleData CargoTruckData = new(
        ProductTypes: [ProductType.ASH, ProductType.SULFUR, ProductType.MUSHROOM, ProductType.BONE],
        MaxSpeed: 65.0f,
        Capacity: 80,
        Price: 11000
    );

    public static readonly VehicleData MiniCargoTruckData = new(
    ProductTypes: [ProductType.ASH, ProductType.SULFUR, ProductType.MUSHROOM, ProductType.BONE],
    MaxSpeed: 75.0f,
    Capacity: 40,
    Price: 8000
);

    #endregion
    #endregion


    #region GameplayConstants

    public const int StartingMoney = 100_000;
    public const double BaseRoadPrice = 100;
    public const double BaseStationPrice = 500;
    public const double BaseTerraformationPrice = 500;

    public const double MushroomPricePerUnit = 200;

    public const double SellRefundRate = 0.5;

    public const double MonthlyExpenseCycleSeconds = 600.0;

    public const double MonthlyVehicleMaintenanceCost = 500.0;



    #endregion

    #region Bridges
    public readonly record struct BridgeData(
        int Length, 
        float MaxSpeed,
        double Price, 
        string Name,
        int Tier
    );

    public static readonly BridgeData BoneBridge = new(
        Length: 5,
        MaxSpeed: 10.0f,
        Price: 100,
        "Csonthíd (5 mező) - 10 km/h",
        0
    );

    public static readonly BridgeData StoneBridge = new(
        Length: 9,
        MaxSpeed: 15.0f,
        Price: 200,
        "Kőhíd (9 mező) - 15 km/h",
        1
    );

    public static readonly BridgeData SteelBridge = new(
        Length: 13,
        MaxSpeed: 20.0f,
        Price: 300,
        "Acélhíd (13 mező) - 20 km/h",
        2
    );

    public static readonly BridgeData[] BridgeTypes = [BoneBridge, StoneBridge, SteelBridge];

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