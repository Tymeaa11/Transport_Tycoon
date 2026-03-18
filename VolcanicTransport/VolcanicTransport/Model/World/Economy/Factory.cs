using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Factory(ProductType baseProduct, Product finalProduct, ProductBuffer baseProductBuffer, ProductBuffer finalProductBuffer, Coordinate coord)
    {
        public ProductType BaseProduct { get; } = baseProduct;
        public Product FinalProduct { get; } = finalProduct;
        public ProductBuffer BaseProductBuffer { get; } = baseProductBuffer;
        public ProductBuffer FinalProductBuffer { get; } = finalProductBuffer;
        public Coordinate OriginCoordinate { get; } = coord;

        private readonly List<Field> _factoryFields = [];

        public void AddField(Field f)
        {
            //ELLENŐRZÉSEK TODO//
            _factoryFields.Add(f);
        }
    }
}
