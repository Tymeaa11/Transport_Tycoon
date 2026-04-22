using System.Text.Json.Serialization;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Station(Coordinate coordinate, string name, ProductBuffer passangerBuffer, Product passengerDemand, double passengerAccumulator = 0) : KnowsNeighbour(coordinate)
    {
        #region Fields
        public string StationName { get; protected set; } = name;
        [JsonInclude]
        protected ProductBuffer passangerBuffer = passangerBuffer;
        [JsonInclude]
        protected Product PassengerDemand = passengerDemand;
        public bool IsOccupied { get; protected set; }
        [JsonIgnore]
        public int WaitingPassengers => passangerBuffer.CurrentLoad;

        [JsonInclude]
        private double _passengerAccumulator = passengerAccumulator;
        #endregion

        #region Methods
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

            var rnd = new Random();
            int leavingCount = 0;
            int currentPassengers = vehicle.CurrentLoad;

            for (int i = 0; i < currentPassengers; i++)
            {
                if (rnd.NextDouble() < GameSettings.ChanceToUnboard)
                {
                    leavingCount++;
                }
            }

            int actualUnloaded = vehicle.Unload(leavingCount);

            return actualUnloaded;
        }
        #endregion
    }
}
