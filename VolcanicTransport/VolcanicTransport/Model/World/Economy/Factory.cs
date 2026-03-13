namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Factory
    {
        private ProductType baseProduct;
        private ProductType finalProduct;
        private ProductBuffer baseProductBuffer;
        private ProductBuffer finalProductBuffer;
        private List<Field> factoryFields;

        public Factory(ProductType baseProduct, ProductType finalProduct, ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, List<Field> factoryFields)
        {
            this.baseProduct = baseProduct;
            this.finalProduct = finalProduct;
            this.baseProductBuffer = baseProductBuffer;
            this.finalProductBuffer = finalProductBuffer;
            this.factoryFields = factoryFields;
        }
    }
}
