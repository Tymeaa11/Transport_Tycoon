using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class SteamProducer : Factory
    {
        public SteamProducer(Coordinate coord) : base(ProductType.NONE, new Product(ProductType.STEAM, 0, 100, 5), new ProductBuffer(ProductType.NONE, 0), new ProductBuffer(ProductType.STEAM, 5000), coord) { }
    }
}
