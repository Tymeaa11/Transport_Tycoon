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
    public abstract class Factory(string name, ProductType baseProduct, Product finalProduct, ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, Coordinate originCoordinate)
    {
        public string Name { get; } = name;
        public ProductType BaseProduct { get; } = baseProduct;
        public Product FinalProduct { get; } = finalProduct;
        public ProductBuffer BaseProductBuffer { get; } = baseProductBuffer;
        public ProductBuffer FinalProductBuffer { get; } = finalProductBuffer;
        public Coordinate OriginCoordinate { get; } = originCoordinate;

        private readonly List<Field> _factoryFields = [];

        private double _productionAccumulator = 0;

        protected Factory(string name, Coordinate origin, GameSettings.FactoryData factoryData) : this(
            name,
            factoryData.BaseProduct,
            new Product(factoryData.FinalProduct),
            new ProductBuffer(factoryData.BaseProduct, factoryData.BaseProductBufferCapacity),
            new ProductBuffer(factoryData.FinalProduct.ProductType, factoryData.FinalProductBufferCapacity), origin
            )
        { }

        public void AddField(Field f)
        {
            _factoryFields.Add(f);
        }

        public void Update(double deltaTime, float totalTime)
        {
            float efficiency = FinalProduct.GetFactoryEfficiency(totalTime);

            // A tényleges termelés az adott pillanatban
            double currentProduction = GameSettings.BaseProductionRate * efficiency;

            // Ezt adjuk hozzá az akkumulátorhoz
            _productionAccumulator += deltaTime * currentProduction;

            if (_productionAccumulator >= 1.0)
            {
                int producedCount = (int)_productionAccumulator;
                _productionAccumulator -= producedCount;
            }
        }
    }

    public class AshProducer : Factory
    {
        [JsonConstructor]
        public AshProducer(string name, ProductType baseProduct, Product finalProduct,
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer,
            Coordinate originCoordinate)
            : base(name, baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate)
        { }

        public AshProducer(string name, Coordinate origin)
            : base(name, origin, GameSettings.AshProducerFactoryData)
        { }
    }
    public class BoneProducer : Factory
    {
        [JsonConstructor]
        public BoneProducer(string name, ProductType baseProduct, Product finalProduct,
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer,
            Coordinate originCoordinate)
            : base(name, baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate)
        { }

        public BoneProducer(string name, Coordinate origin)
            : base(name, origin, GameSettings.AshProducerFactoryData)
        { }
    }
    public class MushroomProducer : Factory
    {
        [JsonConstructor]
        public MushroomProducer(string name, ProductType baseProduct, Product finalProduct,
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer,
            Coordinate originCoordinate)
            : base(name, baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate)
        { }

        public MushroomProducer(string name, Coordinate origin)
            : base(name, origin, GameSettings.AshProducerFactoryData)
        { }
    }
    public class SteamProducer : Factory
    {
        [JsonConstructor]
        public SteamProducer(string name, ProductType baseProduct, Product finalProduct,
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer,
            Coordinate originCoordinate)
            : base(name, baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate)
        { }

        public SteamProducer(string name, Coordinate origin)
            : base(name, origin, GameSettings.AshProducerFactoryData)
        { }
    }
    public class SulfurProducer : Factory
    {
        [JsonConstructor]
        public SulfurProducer(string name, ProductType baseProduct, Product finalProduct,
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer,
            Coordinate originCoordinate)
            : base(name, baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate)
        { }

        public SulfurProducer(string name, Coordinate origin)
            : base(name, origin, GameSettings.AshProducerFactoryData)
        { }
    }
    public class ConcreteFactory : Factory
    {
        [JsonConstructor]
        public ConcreteFactory(string name, ProductType baseProduct, Product finalProduct,
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer,
            Coordinate originCoordinate)
            : base(name, baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate)
        { }

        public ConcreteFactory(string name, Coordinate origin)
            : base(name, origin, GameSettings.AshProducerFactoryData)
        { }
    }
    public class CondensatorFactory : Factory
    {
        [JsonConstructor]
        public CondensatorFactory(string name, ProductType baseProduct, Product finalProduct,
            ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer,
            Coordinate originCoordinate)
            : base(name, baseProduct, finalProduct, baseProductBuffer, finalProductBuffer, originCoordinate)
        { }

        public CondensatorFactory(string name, Coordinate origin)
            : base(name, origin, GameSettings.AshProducerFactoryData)
        { }
    }

}
