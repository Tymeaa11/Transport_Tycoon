using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class BoneProducer(Coordinate coord) : Factory(ProductType.NONE, new Product(ProductType.BONE, 0, 100, 5), new ProductBuffer(ProductType.NONE, 0), new ProductBuffer(ProductType.BONE, 5000), coord)
    {
    }
}
