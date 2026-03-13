namespace VolcanicTransport.Model.World.Economy
{
    public class SulfurProducer : Factory
    {
        public SulfurProducer(List<Field> fields) : base(ProductType.NONE, ProductType.SULFUR, new ProductBuffer(ProductType.NONE, 0, 0), new ProductBuffer(ProductType.SULFUR, 200, 0), fields) { }

    }
}
