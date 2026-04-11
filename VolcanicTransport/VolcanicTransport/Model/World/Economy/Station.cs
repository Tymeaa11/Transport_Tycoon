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

        public abstract bool UnLoadProductFromVehicle(Vehicle vehicle);
        public bool Boarding(Vehicle vehicle)
        {
            if (vehicle is not { Type: ProductType.HUMAN })
            {
                return false;
            }

            var waitingPassengers = passangerBuffer.CurrentLoad;

            if (waitingPassengers == 0)
            {
                return false;
            }

            var taken = passangerBuffer.FillVehicle(vehicle);

            return taken != 0;
        }

        public bool UnBoarding(Vehicle vehicle, float totaltime)
        {
            if (vehicle is not { Type: ProductType.HUMAN })
                return false;

            vehicle.Unload(GetWaitingPassengers(totaltime));

            return true;
        }
    }
}
