namespace Transport.Model.World.Economy
{
    public class AshProducer : Factory
    {
        public AshProducer(List<Field> fields) : base(ProductType.NONE, ProductType.ASH, new ProductBuffer(ProductType.NONE, 0, 0), new ProductBuffer(ProductType.ASH, 200, 0), fields) { }

    }
}
