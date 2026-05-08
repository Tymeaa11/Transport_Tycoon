using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class VehicleManager : IVehicleManager
    {
        private readonly List<Vehicle> _vehicles = [];

        public IReadOnlyList<Vehicle> GetVehicles() => _vehicles.AsReadOnly();

        private readonly Dictionary<Coordinate, List<Vehicle>> _spatialGrid = [];

        public IReadOnlyList<Vehicle> GetVehiclesOnField(Coordinate coord)
        {
            if (_spatialGrid.TryGetValue(coord, out var vehicles))
            {
                return vehicles.AsReadOnly();
            }
            return [];
        }

        public void RegisterVehicleOnField(Vehicle vehicle, Coordinate coord)
        {
            if (!_spatialGrid.TryGetValue(coord, out var vehicles))
            {
                vehicles = [];
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
            _vehicles.Add(vehicle);
        }

        public void RemoveVehicle(Vehicle vehicle)
        {
            _vehicles.Remove(vehicle);
        }

        public void Update(double deltaTime)
        {
            var currentVehicles = _vehicles.ToList();
            foreach (var v in currentVehicles)
            {
                v.Update(deltaTime);
            }
        }
    }
}
