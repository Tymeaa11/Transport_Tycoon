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
        public bool LoadProduct()
        {
            if (vehicles.Count > 0)
            {
                Vehicle firstVehicle = vehicles[0];
                                                  
                return true;
            }
            return false;
        }


        public override bool UnLoadProduct() { /* Rakodás logika */ return true; }
        public override bool Boarding() { /* Felszállás logika */ return true; }
    }
}
