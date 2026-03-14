namespace VolcanicTransport.Model.World.Economy
{
    public class BoneProducer : Factory
    {
        public BoneProducer(List<Field> fields) : base(ProductType.NONE, ProductType.BONE, new ProductBuffer(ProductType.NONE, 0, 0), new ProductBuffer(ProductType.BONE, 5000, 0), fields) { }

    }
}
