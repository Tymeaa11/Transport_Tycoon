namespace VolcanicTransport.Model.World.Economy
{
    public class MushroomProducer : Factory
    {
        public MushroomProducer(List<Field> fields) : base(ProductType.NONE, new Product(ProductType.MUSHROOM, 0, 100, 5),new ProductBuffer(ProductType.NONE,0),new ProductBuffer(ProductType.MUSHROOM,5000),fields){}
    }
}
