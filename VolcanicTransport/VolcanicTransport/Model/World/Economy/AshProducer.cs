using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class AshProducer(Coordinate origin) : Factory(ProductType.NONE, new Product(ProductType.ASH, 0, 100, 5), new ProductBuffer(ProductType.NONE, 0), new ProductBuffer(ProductType.ASH, 5000), origin)
    {
    }
}
