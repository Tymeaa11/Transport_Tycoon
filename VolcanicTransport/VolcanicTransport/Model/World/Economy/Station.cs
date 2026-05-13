using System.Text.Json.Serialization;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Station(Coordinate coordinate, string name, ProductBuffer passangerBuffer, Product passengerDemand, double passengerAccumulator = 0) : Road(coordinate)
    {
        #region Fields
        public string StationName { get; protected set; } = name;
        [JsonInclude]
        protected ProductBuffer PassengerBuffer = passangerBuffer;
        [JsonInclude]
        protected Product PassengerDemand = passengerDemand;
        [JsonIgnore]
        public int WaitingPassengers => PassengerBuffer.CurrentLoad;
        public bool IsOccupied { get; set; }

        [JsonInclude]
        private double passengerAccumulator = passengerAccumulator;
        #endregion

        #region Methods
        public int GetWaitingPassengers(double deltaTime)
        {
            passengerAccumulator += deltaTime * GameSettings.PeopleGrowthRate;

            if (passengerAccumulator >= 1.0)
            {
                int newPeople = (int)passengerAccumulator;
                int left = PassengerBuffer.AddAmount(newPeople);
                passengerAccumulator -= left;
            }
            return PassengerBuffer.CurrentLoad;
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

            var waitingPassengers = PassengerBuffer.CurrentLoad;

            if (waitingPassengers == 0)
            {
                return 0;
            }

            var taken = PassengerBuffer.FillVehicle(vehicle);

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
