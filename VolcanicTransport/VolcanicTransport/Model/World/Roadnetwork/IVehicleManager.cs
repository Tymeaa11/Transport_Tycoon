using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public interface IVehicleManager
    {
        IReadOnlyList<Vehicle> GetVehiclesOnField(Coordinate coord);
        void RegisterVehicleOnField(Vehicle vehicle, Coordinate coord);
        void UnregisterVehicleFromField(Vehicle vehicle, Coordinate coord);
    }
}
