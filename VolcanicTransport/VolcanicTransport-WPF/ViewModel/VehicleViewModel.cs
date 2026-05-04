using System.Collections.ObjectModel;
using VolcanicTransport.Model.World.Roadnetwork;
using System.Windows.Media;

namespace VolcanicTransport_WPF.ViewModel
{
    public class VehicleViewModel : ViewModelBase
    {
        private readonly Vehicle _vehicle;

        public ObservableCollection<string> ScheduleList { get; } = [];

        public Brush VehicleColor
        {
            get
            {
                return _vehicle switch
                {
                    MiniBus => Brushes.Khaki,
                    Bus => Brushes.Gold,

                    MiniTankerTruck => Brushes.LightSkyBlue,
                    TankerTruck => Brushes.RoyalBlue,

                    MiniCargoTruck => Brushes.LightGreen,
                    CargoTruck => Brushes.ForestGreen,

                    _ => Brushes.Gray
                };
            }
        }

        public VehicleViewModel(Vehicle vehicle)
        {
            _vehicle = vehicle;
            _vehicle.StateUpdated += (sender, args) =>
            {
                OnPropertyChanged(nameof(VisualX));
                OnPropertyChanged(nameof(VisualY));
                OnPropertyChanged(nameof(VisualAngle));
                OnPropertyChanged(nameof(StateDisplay));
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(GetCurrentCapacity));
                OnPropertyChanged(nameof(SpeedDisplay));
            };
            _vehicle.RouteChanged += (sender, args) => RefreshScheduleList();
            RefreshScheduleList();
        }

        private void RefreshScheduleList()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ScheduleList.Clear();
                var activeRoute = _vehicle.PendingRoute ?? _vehicle.Route;

                if (activeRoute != null && activeRoute.Stops.Count > 0)
                {
                    for (int i = 0; i < activeRoute.Stops.Count; i++)
                    {
                        var stop = activeRoute.Stops[i];
                        string prefix = (i == _vehicle.CurrentStopIndex && _vehicle.PendingRoute == null) ? "➔ " : "   ";
                        ScheduleList.Add($"{prefix}Állomás: {stop.Coordinate.X}, {stop.Coordinate.Y}");
                    }
                }
                else
                {
                    ScheduleList.Add("Nincs menetrend megadva.");
                }
            });
        }



        public double VisualX => _vehicle.Position.X;
        public double VisualY => _vehicle.Position.Y;
        public float VisualAngle => _vehicle.Angle;
        public string GetName => _vehicle.Name;

        public Vehicle GetVehicle => _vehicle;

        public string Type => _vehicle.CurrentType.ToString();

        public string GetCurrentCapacity => (_vehicle.Capacity - _vehicle.CurrentLoad).ToString();
        public string SpeedDisplay => (_vehicle.CurrentSpeed).ToString() + " km/h";

        public string StateDisplay => _vehicle.State.ToString();

    }
}
