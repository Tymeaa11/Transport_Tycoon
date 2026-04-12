namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryBuilding : ISurface
    {
        private Factory factory;

        public string Name => factory.Name;

        public ProductType BaseProduct => factory.BaseProduct;

        public ProductType FinalProduct => factory.FinalProduct.ProductType;

        public int BaseProductAmount => factory.BaseProductBuffer.AmountNeeded();

        public int FinalProductAmount => factory.FinalProductBuffer.CurrentLoad;

        public FactoryBuilding(Factory factory)
        {
            this.factory = factory;
        }
    }
}
