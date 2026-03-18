using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class TankerTruck(string name, ProductType type) : Vehicle(name, 60.0f, 800, 10000, type)
    {
    }
}
