using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class CargoTruck(string name, ProductType type) : Vehicle(name, 70.0f, 900, 11000, type)
    {
    }
}
