using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryStation : Station
    {
        private Factory factory;
        public FactoryStation(Coordinate coor, string name, Factory factory) : base(coor, name, new ProductBuffer(ProductType.HUMAN, 50), new Product(ProductType.HUMAN, 0, 50, 5)) 
        {
            this.factory = factory;
        }
        public ProductType GetFactoryNeeds() => factory.getBaseProduct();
        public ProductType GetFactoryFinishedProduct() => factory.getFinalProduct().ProductType;
        public float GetFactoryEfficiency() => factory.getFinalProduct().GetFactoryEfficiency();
        public bool LoadProductToVehicle()
        {
            if (vehicle == null || vehicle.getType() != factory.getFinalProduct().ProductType)
            {
                return false;
            }

            int taken = vehicle.Load(factory.getFinalProductBuffer().CurrentLoad());

            return true;
        }


        public override bool UnLoadProductFromVehicle() 
        {
            if (vehicle == null || vehicle.getType() != factory.getBaseProduct())
            {
                return false;
            }

            int amountNeededForFactory = factory.getBaseProductBuffer().amountNeeded();

            if (amountNeededForFactory == 0) { return false; }

            int provided = vehicle.Unload(amountNeededForFactory);

            if (provided == 0)
            {
                return false;
            }

            factory.getBaseProductBuffer().ReciveProduct(vehicle.getType(), provided);

            return true;
        }
    }
}
