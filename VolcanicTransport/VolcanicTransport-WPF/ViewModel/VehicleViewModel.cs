using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_WPF.ViewModel
{
    public class VehicleViewModel : ViewModelBase
    {
        private readonly Vehicle _vehicle;

        public ObservableCollection<string> ScheduleList { get; } = new ObservableCollection<string>();

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
                    if (_vehicle.PendingRoute != null)
                    {
                        ScheduleList.Insert(0, "[FÜGGŐBEN - Érkezés után aktiválódik]");
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

        public string Type => _vehicle.Type.ToString();

        public string GetCapacity => _vehicle.Capacity.ToString();
        public string SpeedDisplay => (_vehicle.MaxSpeed * 45).ToString() + " km/h";

        public string StateDisplay => _vehicle.State.ToString();

    }
}
