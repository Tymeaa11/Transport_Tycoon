namespace VolcanicTransport.Model.World.Economy
{
    public class SulfurProducer : Factory
    {
        public SulfurProducer(List<Field> fields) : base(ProductType.NONE, new Product(ProductType.SULFUR, 0, 100), new ProductBuffer(ProductType.NONE, 0), new ProductBuffer(ProductType.SULFUR, 5000), fields) { }

    }
}
