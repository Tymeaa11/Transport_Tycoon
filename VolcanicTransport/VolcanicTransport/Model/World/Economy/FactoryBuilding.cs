namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryBuilding(Factory factory) : ISurface
    {
        public string Name => factory.Name;

        public ProductType BaseProduct => factory.BaseProduct;

        public ProductType FinalProduct => factory.FinalProduct.ProductType;

        public int BaseProductNeed => factory.BaseProductBuffer.MaxCapacity;

        public int BaseProductAmount => factory.BaseProductBuffer.CurrentLoad;

        public int FinalProductAmount => factory.FinalProductBuffer.CurrentLoad;
    }
}
