using System.Diagnostics;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.Services
{
    public class VehicleService
    {
        private readonly Func<World.World> _getWorld;
        private readonly EconomyService _economy;
        private readonly Func<double> _getTime;

        private World.World WorldInstance => _getWorld();

        public event EventHandler? VehicleBought;
        public event EventHandler? VehicleSold;
        public event EventHandler<VehicleArrivedEventArgs>? VehicleArrivedAtStation;

        public VehicleService(Func<World.World> getWorld, EconomyService economy, Func<double> getTime)
        {
            _getWorld = getWorld;
            _economy = economy;
            _getTime = getTime;
        }

        public bool BuyVehicle(Vehicle v)
        {
            if (_economy.TryPurchase(v.Price))
            {
                v.ArrivedAtStation += HandleVehicleArrived;
                WorldInstance.AddVehicle(v);
                VehicleBought?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void SellVehicle(Vehicle v)
        {
            if (!WorldInstance.HasVehicle(v)) return;

            v.ArrivedAtStation -= HandleVehicleArrived;
            v.ClearRoute();
            _economy.AddMoney(v.Price * GameSettings.SellRefundRate);
            WorldInstance.RemoveVehicle(v);
            VehicleSold?.Invoke(this, EventArgs.Empty);
        }

        public bool CreateAndStartVehicle(string vehicleType, string vehicleName, Route chosenRoute)
        {
            if (chosenRoute == null || chosenRoute.Stops.Count < 2)
                return false;

            Vehicle newVehicle = vehicleType switch
            {
                "CargoTruck" => new CargoTruck(vehicleName),
                "MiniCargoTruck" => new MiniCargoTruck(vehicleName),
                "TankerTruck" => new TankerTruck(vehicleName),
                "MiniTankerTruck" => new MiniTankerTruck(vehicleName),
                "MiniBus" => new MiniBus(vehicleName),
                _ => new Bus(vehicleName)
            };

            newVehicle.AssignNewRoute(chosenRoute);

            Station firstStation = chosenRoute.Stops[0];
            Station? secondStation = chosenRoute.Stops.Count > 1 ? chosenRoute.Stops[1] : null;
            newVehicle.CurrentStopIndex = secondStation != null ? 1 : 0;

            List<Road> path = [];
            if (secondStation != null)
            {
                var nodes = WorldInstance.Roadnetwork.NodeMap;
                RoadNode? startNode = nodes.Values.FirstOrDefault(n => n.Coordinate == firstStation.Coordinate);
                RoadNode? endNode = nodes.Values.FirstOrDefault(n => n.Coordinate == secondStation.Coordinate);

                if (startNode != null && endNode != null)
                    path = Pathfinder.Instance.FindPath(startNode, endNode) ?? [];
            }

            if (path.Count == 0 || path.First().Coordinate != firstStation.Coordinate)
                path.Insert(0, firstStation);

            if (secondStation != null && path.Last().Coordinate != secondStation.Coordinate)
                path.Add(secondStation);

            newVehicle.StartJourney(path, false, firstStation);
            return BuyVehicle(newVehicle);
        }

        public static void AddStopToVehicle(Vehicle v, Station s)
        {
            if (v.Route == null || (v.Route.Stops.Count > 0 && v.Route.Stops.Last() == s)) return;

            v.Route.AddStop(s);
            Debug.WriteLine($"Megálló hozzáadva: {s.Coordinate}. Összesen: {v.Route.Stops.Count}");

            var graph = World.World.Instance.Roadnetwork;
            graph.RegisterNodeIfNeeded(s.Coordinate);
            graph.RebuildEdges();

            if (v.State == VehicleState.Waiting)
                v.TryStartNextRoute();

            v.TriggerRouteChanged();
        }

        public void HandleVehicleArrived(object? sender, VehicleArrivedEventArgs e)
        {
            Debug.WriteLine($"[VehicleService] {e.Vehicle.Name} megérkezett a(z) {e.Station.Coordinate} állomásra!");

            Vehicle vehicle = e.Vehicle;
            Station station = e.Station;
            ProductType productType = vehicle.CurrentType;

            if (vehicle is CargoTruck or TankerTruck or MiniCargoTruck or MiniTankerTruck)
            {
                int accepted = station.UnLoadProductFromVehicle(vehicle);
                if (accepted != 0)
                {
                    _economy.AddMoney(accepted * GameSettings.GetPrice(productType));
                    Debug.WriteLine($"{accepted} egység leadva, pénz hozzáadva.");
                }
                if (station is FactoryStation fs)
                {
                    int amount = fs.LoadProduct(vehicle);
                    if (amount > 0) Debug.WriteLine($"{amount} egység felvéve a gyárból.");
                }
            }
            else
            {
                int amount = station.UnBoarding(vehicle);
                if (amount != 0)
                {
                    _economy.AddMoney(amount * GameSettings.GetPrice(productType));
                    Debug.WriteLine($"{amount} ember leszállt.");
                }
                if (vehicle.Capacity > vehicle.CurrentLoad)
                {
                    int loaded = station.Boarding(vehicle);
                    if (loaded > 0) Debug.WriteLine($"{loaded} ember felszállt.");
                }
            }

            VehicleArrivedAtStation?.Invoke(this, e);
        }
    }
}
