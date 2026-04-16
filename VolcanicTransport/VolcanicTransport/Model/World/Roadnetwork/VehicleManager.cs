using System.Collections.ObjectModel;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class VehicleManager
    {
        public ObservableCollection<Vehicle> Vehicles { get; } = [];

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
