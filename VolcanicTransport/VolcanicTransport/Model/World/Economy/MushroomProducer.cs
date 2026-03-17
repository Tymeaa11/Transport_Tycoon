using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class MushroomProducer : Factory
    {
        public MushroomProducer(Coordinate coord) : base(ProductType.NONE, new Product(ProductType.MUSHROOM, 0, 100, 5),new ProductBuffer(ProductType.NONE,0),new ProductBuffer(ProductType.MUSHROOM,5000),coord){}
    }
}
