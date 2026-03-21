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

        public string Name => _vehicle.Name;

        // Később ide jöhet a forgatás is, ha a téglalapot az út irányába akarod állítani
        // public double Rotation => ... 
    }
}
