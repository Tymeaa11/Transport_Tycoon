using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class ConcreteFactory(Coordinate coord) : Factory(ProductType.ASH, new Product(ProductType.CONCRETE, 0, 100, 5), new ProductBuffer(ProductType.ASH, 5000), new ProductBuffer(ProductType.CONCRETE, 5000), coord)
    {
    }
}
