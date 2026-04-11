using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class VehicleManager
    {
        public ObservableCollection<Vehicle> Vehicles { get; } = new ObservableCollection<Vehicle>();

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
