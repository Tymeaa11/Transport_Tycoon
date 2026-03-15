using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class Bus : Vehicle
    {
        public Bus(string name, ProductType type) : base(name, 90.0f, 50, 8500, type) { }
    }
}
