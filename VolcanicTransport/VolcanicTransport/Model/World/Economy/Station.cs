using VolcanicTransport.Model.TerrainGeneration.Layers;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Station(Coordinate coordinate, string name, ProductBuffer passangerBuffer, Product passengerDemand) : KnowsNeighbour(coordinate)
    {
        protected string name = name;
        protected ProductBuffer passangerBuffer = passangerBuffer;
        protected Product PassengerDemand = passengerDemand;

        protected bool isOccupied = false;
        public bool IsOccupied { get { return isOccupied; } }
        public string Name { get { return name; } }
        public int WaitingPassengers => passangerBuffer.CurrentLoad;

        private double _passengerAccumulator = 0;
        public int GetWaitingPassengers(double deltaTime)
        {
            _passengerAccumulator += deltaTime * GameSettings.PeopleGrowthRate;

            if (_passengerAccumulator >= 1.0)
            {
                int newPeople = (int)_passengerAccumulator;
                int left = passangerBuffer.AddAmount(newPeople);
                _passengerAccumulator -= left;
            }
            return passangerBuffer.CurrentLoad;
        }
        public int GetPricePerPassenger(float time)
        {
            return PassengerDemand.GetDemand(time);
        }
        public abstract int UnLoadProductFromVehicle(Vehicle vehicle);
        public int Boarding(Vehicle vehicle) // mennyi ember szállt fel
        {
            if (vehicle.CurrentLoad > 0 && vehicle.CurrentType != ProductType.HUMAN)
            {
                return 0;
            }

            var waitingPassengers = passangerBuffer.CurrentLoad;

            if (waitingPassengers == 0)
            {
                return 0;
            }

            var taken = passangerBuffer.FillVehicle(vehicle);

            return taken;
        }

        public int UnBoarding(Vehicle vehicle) // mennyi ember szállt le
        {
            if (vehicle.CurrentType != ProductType.HUMAN)
            {
                return 0;
            }

            int amount = vehicle.Unload(WaitingPassengers);

            return amount;
        }
    }
}
