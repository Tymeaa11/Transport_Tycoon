using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Factory
    {
        private ProductType baseProduct;
        private Product finalProduct;
        private ProductBuffer baseProductBuffer;
        private ProductBuffer finalProductBuffer;
        private List<Field> factoryFields;
        private Coordinate originCoordinate;

        public Factory(ProductType baseProduct, Product finalProduct, ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, Coordinate coord)
        {
            this.baseProduct = baseProduct;
            this.finalProduct = finalProduct;
            this.baseProductBuffer = baseProductBuffer;
            this.finalProductBuffer = finalProductBuffer;
            this.originCoordinate = coord;
            this.factoryFields = new List<Field>();
        }

        public Coordinate OriginCoordinate { get { return originCoordinate; } }

        public void AddField(Field f)
        {
            //ELLENŐRZÉSEK TODO//
            factoryFields.Add(f);
        }

        public ProductType getBaseProduct() { return baseProduct; }
        public Product getFinalProduct() { return finalProduct; }

        public ProductBuffer getFinalProductBuffer() { return finalProductBuffer; }

        public ProductBuffer getBaseProductBuffer() { return baseProductBuffer; }
    }
}
