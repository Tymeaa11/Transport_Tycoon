using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class MiniBus : Vehicle
    {
        public MiniBus(string name, ProductType type) : base(name, 100.0f, 15, 6000, type)
        {

        }
    }
}
