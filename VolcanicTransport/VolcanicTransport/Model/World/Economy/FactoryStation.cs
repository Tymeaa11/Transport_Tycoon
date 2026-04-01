using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryStation(Coordinate coor, string name, Factory factory) : Station(coor, name, new ProductBuffer(ProductType.HUMAN, 50), new Product(ProductType.HUMAN, 0, 50, 5))
    {
        private readonly Factory _factory = factory;

        public ProductType GetFactoryNeeds => _factory.BaseProduct;
        public ProductType GetFactoryFinishedProduct => _factory.FinalProduct.ProductType;
        public float GetFactoryEfficiency(float time) => _factory.FinalProduct.GetFactoryEfficiency(time);
        public bool LoadProduct()
        {
            if (vehicle == null || vehicle.Type != _factory.FinalProduct.ProductType)
            {
                return false;
            }

            //int taken = vehicle.Load(factory.FinalProductBuffer.CurrentLoad());

            return true;
        }


        public override bool UnLoadProductFromVehicle()
        {
            if (vehicle == null || vehicle.Type != _factory.BaseProduct)
            {
                return false;
            }

            int amountNeededForFactory = _factory.BaseProductBuffer.AmountNeeded();

            if (amountNeededForFactory == 0) { return false; }

            int provided = vehicle.Unload(amountNeededForFactory);

            if (provided == 0)
            {
                return false;
            }

            _factory.BaseProductBuffer.ReceiveProduct(vehicle.Type, provided);

            return true;
        }
    }
}
