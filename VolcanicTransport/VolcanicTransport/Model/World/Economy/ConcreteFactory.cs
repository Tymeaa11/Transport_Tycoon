namespace VolcanicTransport.Model.World.Economy
{
    public class ConcreteFactory : Factory
    {
        public ConcreteFactory(List<Field> fields) : base(ProductType.ASH, ProductType.CONCREATE, new ProductBuffer(ProductType.ASH, 5000, 0), new ProductBuffer(ProductType.CONCREATE, 5000, 0), fields) { }

    }
}
