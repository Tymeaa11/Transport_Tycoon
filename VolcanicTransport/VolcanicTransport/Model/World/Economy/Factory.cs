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

        private double _productionAccumulator = 0;

        public void AddField(Field f)
        {
            //ELLENŐRZÉSEK TODO//
            _factoryFields.Add(f);
        }

        public void Update(double deltaTime, float totalTime)
        {
            float efficiency = FinalProduct.GetFactoryEfficiency(totalTime);

            // 1 termék / másodperc
            double baseProductionRate = 1.0;

            // A tényleges termelés az adott pillanatban
            double currentProduction = baseProductionRate * efficiency;

            // Ezt adjuk hozzá az akkumulátorhoz
            _productionAccumulator += deltaTime * currentProduction;

            if (_productionAccumulator >= 1.0)
            {
                int producedCount = (int)_productionAccumulator;
                _productionAccumulator -= producedCount;
            }
        }
    }
}
