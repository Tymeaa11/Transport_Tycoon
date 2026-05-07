using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class VehicleArrivedEventArgs(Vehicle vehicle, Station station) : EventArgs
    {
        public Vehicle Vehicle { get; } = vehicle;
        public Station Station { get; } = station;
    }
}
