using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public interface IVehicleManager
    {
        IReadOnlyList<Vehicle> GetVehicles();
        IReadOnlyList<Vehicle> GetVehiclesOnField(Coordinate coord);
        void RegisterVehicleOnField(Vehicle vehicle, Coordinate coord);
        void UnregisterVehicleFromField(Vehicle vehicle, Coordinate coord);
        void AddVehicle(Vehicle vehicle);
        void RemoveVehicle(Vehicle vehicle);
        void Update(double deltaTime);
    }
}
