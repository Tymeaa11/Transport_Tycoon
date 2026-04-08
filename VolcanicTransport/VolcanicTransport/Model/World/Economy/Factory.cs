using System.Text.Json.Serialization;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(AshProducer), "ash")]
    [JsonDerivedType(typeof(BoneProducer), "bone")]
    [JsonDerivedType(typeof(MushroomProducer), "mushroom")]
    [JsonDerivedType(typeof(SteamProducer), "steam")]
    [JsonDerivedType(typeof(SulfurProducer), "sulfur")]
    [JsonDerivedType(typeof(ConcreteFactory), "concrete")]
    [JsonDerivedType(typeof(CondensatorFactory), "condensator")]
    public abstract class Factory(ProductType baseProduct, Product finalProduct, ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, Coordinate originCoordinate)
    {
        public ProductType BaseProduct { get; } = baseProduct;
        public Product FinalProduct { get; } = finalProduct;
        public ProductBuffer BaseProductBuffer { get; } = baseProductBuffer;
        public ProductBuffer FinalProductBuffer { get; } = finalProductBuffer;
        public Coordinate OriginCoordinate { get; } = originCoordinate;

        private readonly List<Field> _factoryFields = [];

        public Factory(Coordinate origin, GameSettings.FactoryData factoryData) : this(
            factoryData.BaseProduct,
            new Product(factoryData.FinalProduct),
            new ProductBuffer(factoryData.BaseProduct, factoryData.BaseProductBufferCapacity),
            new ProductBuffer(factoryData.FinalProduct.ProductType, factoryData.FinalProductBufferCapacity), origin
            )
        { }

        public void AddField(Field f)
        {
            //ELLENŐRZÉSEK TODO//
            _factoryFields.Add(f);
        }
    }

    public class AshProducer : Factory
    {
        [JsonConstructor]
        public AshProducer(ProductType baseProduct, Product finalProduct, 
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, 
            Coordinate originCoordinate) 
            : base(baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate) 
        { }

        public AshProducer(Coordinate origin) 
            : base(origin, GameSettings.AshProducerFactoryData) 
        { }
    }
    public class BoneProducer : Factory
    {
        [JsonConstructor]
        public BoneProducer(ProductType baseProduct, Product finalProduct, 
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, 
            Coordinate originCoordinate) 
            : base(baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate) 
        { }

        public BoneProducer(Coordinate origin) 
            : base(origin, GameSettings.AshProducerFactoryData) 
        { }
    }
    public class MushroomProducer : Factory
    {
        [JsonConstructor]
        public MushroomProducer(ProductType baseProduct, Product finalProduct, 
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, 
            Coordinate originCoordinate) 
            : base(baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate) 
        { }

        public MushroomProducer(Coordinate origin) 
            : base(origin, GameSettings.AshProducerFactoryData) 
        { }
    }
    public class SteamProducer : Factory
    {
        [JsonConstructor]
        public SteamProducer(ProductType baseProduct, Product finalProduct, 
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, 
            Coordinate originCoordinate) 
            : base(baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate) 
        { }

        public SteamProducer(Coordinate origin) 
            : base(origin, GameSettings.AshProducerFactoryData) 
        { }
    }
    public class SulfurProducer : Factory
    {
        [JsonConstructor]
        public SulfurProducer(ProductType baseProduct, Product finalProduct, 
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, 
            Coordinate originCoordinate) 
            : base(baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate) 
        { }

        public SulfurProducer(Coordinate origin) 
            : base(origin, GameSettings.AshProducerFactoryData) 
        { }
    }
    public class ConcreteFactory : Factory
    {
        [JsonConstructor]
        public ConcreteFactory(ProductType baseProduct, Product finalProduct, 
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, 
            Coordinate originCoordinate) 
            : base(baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate) 
        { }

        public ConcreteFactory(Coordinate origin) 
            : base(origin, GameSettings.AshProducerFactoryData) 
        { }
    }
    public class CondensatorFactory : Factory
    {
        [JsonConstructor]
        public CondensatorFactory(ProductType baseProduct, Product finalProduct, 
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, 
            Coordinate originCoordinate) 
            : base(baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate) 
        { }

        public CondensatorFactory(Coordinate origin) 
            : base(origin, GameSettings.AshProducerFactoryData) 
        { }
    }
    
}
