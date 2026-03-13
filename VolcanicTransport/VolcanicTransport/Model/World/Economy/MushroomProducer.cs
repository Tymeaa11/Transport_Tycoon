namespace VolcanicTransport.Model.World.Economy
{
    public class MushroomProducer : Factory
    {
        public MushroomProducer(List<Field> fields) : base(ProductType.NONE,ProductType.MUSHROOM,new ProductBuffer(ProductType.NONE,0,0),new ProductBuffer(ProductType.MUSHROOM,200,0),fields){}
    }
}
