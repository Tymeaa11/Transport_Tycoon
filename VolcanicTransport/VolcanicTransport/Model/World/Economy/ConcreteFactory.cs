namespace VolcanicTransport.Model.World.Economy
{
    public class ConcreteFactory : Factory
    {
        public ConcreteFactory(List<Field> fields) : base(ProductType.ASH, new Product(ProductType.CONCREATE, 0, 100, 5), new ProductBuffer(ProductType.ASH, 5000), new ProductBuffer(ProductType.CONCREATE, 5000), fields) { }

    }
}
