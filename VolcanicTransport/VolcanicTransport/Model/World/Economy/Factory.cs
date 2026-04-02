using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Factory(ProductType baseProduct, Product finalProduct, ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, Coordinate coord)
    {
        public ProductType BaseProduct { get; } = baseProduct;
        public Product FinalProduct { get; } = finalProduct;
        public ProductBuffer BaseProductBuffer { get; } = baseProductBuffer;
        public ProductBuffer FinalProductBuffer { get; } = finalProductBuffer;
        public Coordinate OriginCoordinate { get; } = coord;

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
    
    public class AshProducer(Coordinate origin) : Factory(origin, GameSettings.AshProducerFactoryData) { }
    public class BoneProducer(Coordinate origin) : Factory(origin, GameSettings.BoneProducerFactoryData) { }
    public class MushroomProducer(Coordinate origin) : Factory(origin, GameSettings.MushroomProducerFactoryData) { }
    public class SteamProducer(Coordinate origin) : Factory(origin, GameSettings.SteamProducerFactoryData) { }
    public class SulfurProducer(Coordinate origin) : Factory(origin, GameSettings.SulfurProducerFactoryData) { }
    public class ConcreteFactory(Coordinate origin) : Factory(origin, GameSettings.ConcreteFactoryData) { }
    public class CondensatorFactory(Coordinate origin) : Factory(origin, GameSettings.CondensatorFactoryData) { }
    
}
