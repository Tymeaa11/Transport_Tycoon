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
        public bool LoadProduct(Vehicle vehicle) // adott-e árut a járműnek
        {
            if (vehicle == null || vehicle.Type != _factory.FinalProduct.ProductType)
            {
                return false;
            }

            int amountFilled = _factory.FinalProductBuffer.FillVehicle(vehicle);

            return amountFilled != 0;
        }


        public override bool UnLoadProductFromVehicle(Vehicle vehicle) // kapott-e árut a járműtől
        {
            if (vehicle == null || vehicle.Type != _factory.BaseProduct)
            {
                return false;
            }

            int amountNeededForFactory = _factory.BaseProductBuffer.AmountNeeded();

            if (amountNeededForFactory == 0) { return false; }

            int provided = _factory.BaseProductBuffer.ReciveProduct(vehicle, amountNeededForFactory);

            return provided != 0;
        }
    }
}
