using System.Collections.ObjectModel;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class VehicleManager
    {
        public ObservableCollection<Vehicle> Vehicles { get; } = [];

        private readonly Dictionary<Coordinate, List<Vehicle>> _spatialGrid = new();

        public IReadOnlyList<Vehicle> GetVehiclesOnField(Coordinate coord)
        {
            if (_spatialGrid.TryGetValue(coord, out var vehicles))
            {
                return vehicles;
            }
            return Array.Empty<Vehicle>();
        }

        public void RegisterVehicleOnField(Vehicle vehicle, Coordinate coord)
        {
            if (!_spatialGrid.TryGetValue(coord, out var vehicles))
            {
                vehicles = new List<Vehicle>();
                _spatialGrid[coord] = vehicles;
            }
            if (!vehicles.Contains(vehicle))
            {
                vehicles.Add(vehicle);
            }
        }

        public void UnregisterVehicleFromField(Vehicle vehicle, Coordinate coord)
        {
            if (_spatialGrid.TryGetValue(coord, out var vehicles))
            {
                vehicles.Remove(vehicle);
                if (vehicles.Count == 0)
                {
                    _spatialGrid.Remove(coord);
                }
            }
        }

        public void AddVehicle(Vehicle vehicle)
        {
            Vehicles.Add(vehicle);
        }

        public void RemoveVehicle(Vehicle vehicle)
        {
            Vehicles.Remove(vehicle);
        }

        public void Update(double deltaTime)
        {
            var currentVehicles = Vehicles.ToList();
            foreach (var v in currentVehicles)
            {
                v.Update(deltaTime);
            }
        }
    }
}
