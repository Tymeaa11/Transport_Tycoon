namespace VolcanicTransport.Model.World.Economy
{
    public class CondensatorFactory : Factory
    {
        public CondensatorFactory(List<Field> fields) : base(ProductType.STEAM, ProductType.WATER, new ProductBuffer(ProductType.STEAM, 5000, 0), new ProductBuffer(ProductType.WATER, 5000, 0), fields) { }

    }
}
