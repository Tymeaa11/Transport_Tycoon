using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_WPF.ViewModel
{
    public class VehicleViewModel : ViewModelBase
    {
        private readonly Vehicle _vehicle;

        public VehicleViewModel(Vehicle vehicle)
        {
            _vehicle = vehicle;
            _vehicle.StateUpdated += (sender, args) =>
            {
                OnPropertyChanged(nameof(VisualX));
                OnPropertyChanged(nameof(VisualY));
                OnPropertyChanged(nameof(VisualAngle));
                OnPropertyChanged(nameof(StateDisplay));
            };
        }



        public double VisualX => _vehicle.Position.X;
        public double VisualY => _vehicle.Position.Y;
        public float VisualAngle => _vehicle.Angle;
        public string GetName => _vehicle.Name;

        public Vehicle GetVehicle => _vehicle; 

        public string Type => _vehicle.Type.ToString();
        
        public string GetCapacity =>  _vehicle.Capacity.ToString(); 
        public string SpeedDisplay => (_vehicle.MaxSpeed*45).ToString() + " km/h";

        public string StateDisplay => _vehicle.State.ToString();

    }
}
