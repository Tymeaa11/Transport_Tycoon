using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryStation(Coordinate coor, string name, Factory factory) : Station(coor, name, new ProductBuffer(ProductType.HUMAN, 50), new Product(ProductType.HUMAN, 0, 50, 5))
    {
        private readonly Factory _factory = factory;

        public ProductType GetFactoryNeeds => _factory.BaseProduct;
        public ProductType GetFactoryFinishedProduct => _factory.FinalProduct.ProductType;
        public float GetFactoryEfficiency(float time) => _factory.FinalProduct.GetFactoryEfficiency(time); // 0-1
        public double PricePerBaseProduct => GameSettings.GetPrice(_factory.BaseProduct);
        public int LoadProduct(Vehicle vehicle) // adott-e árut a járműnek
        {
            if (vehicle == null || vehicle.CurrentType != _factory.FinalProduct.ProductType)
            {
                return 0;
            }

            int amountFilled = _factory.FinalProductBuffer.FillVehicle(vehicle);

            return amountFilled;
        }


        public override int UnLoadProductFromVehicle(Vehicle vehicle) // kapott-e árut a járműtől
        {
            if (vehicle == null || vehicle.CurrentType != _factory.BaseProduct)
            {
                return 0;
            }

            int amountNeededForFactory = _factory.BaseProductBuffer.AmountNeeded();

            if (amountNeededForFactory == 0) { return 0; }

            int provided = _factory.BaseProductBuffer.ReciveProduct(vehicle, amountNeededForFactory);

            return provided;
        }
    }
}
