using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Station
    {
        private Coordinate coordinate;
        private string name;
        private ProductBuffer passangerBuffer;
        private List<Vehicle> vehicles;
    }
}
