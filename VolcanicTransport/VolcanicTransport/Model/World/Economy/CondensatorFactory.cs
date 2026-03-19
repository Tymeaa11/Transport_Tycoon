using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class CondensatorFactory(Coordinate coord) : Factory(ProductType.STEAM, new Product(ProductType.WATER, 0, 100, 5), new ProductBuffer(ProductType.STEAM, 5000), new ProductBuffer(ProductType.WATER, 5000), coord)
    {
    }
}
