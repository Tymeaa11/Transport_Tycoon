namespace VolcanicTransport.Model.World.Economy
{
    public class SteamProducer : Factory
    {
        public SteamProducer(List<Field> fields) : base(ProductType.NONE, new Product(ProductType.STEAM, 0, 100, 5), new ProductBuffer(ProductType.NONE, 0), new ProductBuffer(ProductType.STEAM, 5000), fields) { }
    }
}
