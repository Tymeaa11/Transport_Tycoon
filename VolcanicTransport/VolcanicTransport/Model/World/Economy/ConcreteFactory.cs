using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class ConcreteFactory(Coordinate coord) : Factory(ProductType.ASH, new Product(ProductType.CONCREATE, 0, 100, 5), new ProductBuffer(ProductType.ASH, 5000), new ProductBuffer(ProductType.CONCREATE, 5000), coord)
    {
    }
}
