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

        public int GetWaitingPassengers(float totalTime)
        {
            int MaxNewPassengers = 20;
            return (int)(PassengerDemand.GetPassengerEfficiency(totalTime) * MaxNewPassengers);
        }
        public int GetPricePerPassenger(float time)
        {
            return PassengerDemand.GetDemand(time);
        }
        public abstract int UnLoadProductFromVehicle(Vehicle vehicle);
        public int Boarding(Vehicle vehicle)
        {
            if (vehicle is not { CurrentType: ProductType.HUMAN })
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

        public bool UnBoarding(Vehicle vehicle, float totaltime)
        {
            if (vehicle is not { CurrentType: ProductType.HUMAN })
                return false;

            vehicle.Unload(GetWaitingPassengers(totaltime));

            return true;
        }
    }
}
