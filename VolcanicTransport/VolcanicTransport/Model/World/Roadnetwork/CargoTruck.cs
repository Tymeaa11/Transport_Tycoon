using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class CargoTruck : Vehicle
    {
        public CargoTruck(string name, ProductType type) : base(name, 70.0, 900, 11000, type) { }
    }
}
