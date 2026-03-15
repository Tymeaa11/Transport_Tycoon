using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class TankerTruck : Vehicle
    {
        public TankerTruck(string name, ProductType type) : base(name, 60.0f, 800, 10000, type)
        {

        }
    }
}
