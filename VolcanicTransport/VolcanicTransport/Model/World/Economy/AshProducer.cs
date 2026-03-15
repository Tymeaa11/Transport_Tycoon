namespace VolcanicTransport.Model.World.Economy
{
    public class AshProducer : Factory
    {
        public AshProducer(List<Field> fields) : base(ProductType.NONE, new Product(ProductType.ASH, 0, 100), new ProductBuffer(ProductType.NONE, 0), new ProductBuffer(ProductType.ASH, 5000), fields) { }

    }
}
