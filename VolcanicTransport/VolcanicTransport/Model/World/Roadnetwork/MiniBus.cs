using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class MiniBus(string name, ProductType type) : Vehicle(name, 100.0f, 15, 6000, type)
    {
    }
}
