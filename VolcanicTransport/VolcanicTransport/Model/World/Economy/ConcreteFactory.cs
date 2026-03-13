namespace VolcanicTransport.Model.World.Economy
{
    public class ConcreteFactory : Factory
    {
        public ConcreteFactory(List<Field> fields) : base(ProductType.ASH, ProductType.CONCREATE, new ProductBuffer(ProductType.ASH, 200, 0), new ProductBuffer(ProductType.CONCREATE, 200, 0), fields) { }

    }
}
