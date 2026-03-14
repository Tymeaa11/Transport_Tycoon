namespace VolcanicTransport.Model.World.Economy
{
    public class SteamProducer : Factory
    {
        public SteamProducer(List<Field> fields) : base(ProductType.NONE, ProductType.STEAM, new ProductBuffer(ProductType.NONE, 0, 0), new ProductBuffer(ProductType.STEAM, 5000, 0), fields) { }
    }
}
