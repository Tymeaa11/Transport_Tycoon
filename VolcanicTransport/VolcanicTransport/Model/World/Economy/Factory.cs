namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Factory
    {
        private ProductType baseProduct;
        private Product finalProduct;
        private ProductBuffer baseProductBuffer;
        private ProductBuffer finalProductBuffer;
        private List<Field> factoryFields;

        public Factory(ProductType baseProduct, Product finalProduct, ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, List<Field> factoryFields)
        {
            this.baseProduct = baseProduct;
            this.finalProduct = finalProduct;
            this.baseProductBuffer = baseProductBuffer;
            this.finalProductBuffer = finalProductBuffer;
            this.factoryFields = factoryFields;
        }

        public ProductType getBaseProduct() { return baseProduct; }
        public Product getFinalProduct() { return finalProduct; }

        public ProductBuffer getFinalProductBuffer() { return finalProductBuffer; }

        public ProductBuffer getBaseProductBuffer() { return baseProductBuffer; }
    }
}
