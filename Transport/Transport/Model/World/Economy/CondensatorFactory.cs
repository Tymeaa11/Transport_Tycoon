namespace Transport.Model.World.Economy
{
    public class CondensatorFactory : Factory
    {
        public CondensatorFactory(List<Field> fields) : base(ProductType.STEAM, ProductType.WATER, new ProductBuffer(ProductType.STEAM, 200, 0), new ProductBuffer(ProductType.WATER, 200, 0), fields) { }

    }
}
