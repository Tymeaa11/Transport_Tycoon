namespace VolcanicTransport.Model.World.Economy
{
    public class BoneProducer : Factory
    {
        public BoneProducer(List<Field> fields) : base(ProductType.NONE, new Product(ProductType.BONE, 0, 100), new ProductBuffer(ProductType.NONE, 0), new ProductBuffer(ProductType.BONE, 5000), fields) { }

    }
}
