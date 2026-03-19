using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Station(Coordinate coordinate, string name, ProductBuffer passangerBuffer, Product passengerDemand) : KnowsNeighbour(coordinate)
    {
        protected string name = name;
        protected ProductBuffer passangerBuffer = passangerBuffer;
        protected Vehicle? vehicle = null;
        protected Product PassengerDemand = passengerDemand;

        public abstract bool UnLoadProductFromVehicle();
        public bool Boarding()
        {
            if (vehicle == null || vehicle.Type != ProductType.HUMAN)
            {
                return false;
            }

            int waitingPassengers = passangerBuffer.CurrentLoad;

            if (waitingPassengers == 0)
            {
                return false;
            }

            int taken = passangerBuffer.FillVehicle(vehicle);

            if (taken == 0) return false;

            return true;
        }

        public bool UnBoarding()
        {
            if (vehicle == null || vehicle.Type != ProductType.HUMAN) { return false; }

            //vehicle.UnBoard() //TODO//

            return true;
        }
    }
}
