using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_WPF.ViewModel
{
    public class VehicleViewModel : ViewModelBase
    {
        private readonly Vehicle _vehicle;

        public VehicleViewModel(Vehicle vehicle)
        {
            _vehicle = vehicle;

            _vehicle.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Vehicle.VisualPosition))
                {
                    OnPropertyChanged(nameof(PixelX));
                    OnPropertyChanged(nameof(PixelY));
                }
            };
        }

        public double PixelX => _vehicle.VisualPosition.X;
        public double PixelY => _vehicle.VisualPosition.Y;

        public string GetName => _vehicle.Name;

        public Vehicle GetVehicle => _vehicle;

        public string Type => _vehicle.Type.ToString();

        public string GetCapacity => _vehicle.Capacity.ToString();
        public string SpeedDisplay => (_vehicle.MaxSpeed * 45).ToString() + " km/h";

        public string StateDisplay => _vehicle.State.ToString();

    }
}
