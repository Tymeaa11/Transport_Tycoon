using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class Bus(string name, ProductType type) : Vehicle(name, 90.0f, 50, 8500, type)
    {
    }
}
