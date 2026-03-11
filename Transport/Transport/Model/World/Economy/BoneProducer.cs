namespace Transport.Model.World.Economy
{
    public class BoneProducer : Factory
    {
        public BoneProducer(List<Field> fields) : base(ProductType.NONE, ProductType.BONE, new ProductBuffer(ProductType.NONE, 0, 0), new ProductBuffer(ProductType.BONE, 200, 0), fields) { }

    }
}
