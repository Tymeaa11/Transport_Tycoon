using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class SulfurProducer(Coordinate coord) : Factory(ProductType.NONE, new Product(ProductType.SULFUR, 0, 100, 5), new ProductBuffer(ProductType.NONE, 0), new ProductBuffer(ProductType.SULFUR, 5000), coord)
    {
    }
}
