using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Station : KnowsNeighbour
    {
        protected string name;
        protected ProductBuffer passangerBuffer;
        protected Vehicle? vehicle;
        protected Product PassengerDemand;


        public Station(Coordinate coordinate, string name, ProductBuffer passangerBuffer, Product passengerDemand) : base(coordinate)
        {
            this.name = name;
            this.passangerBuffer = passangerBuffer;
            this.vehicle = null;
            PassengerDemand = passengerDemand;
        }

        public abstract bool UnLoadProductFromVehicle();
        public bool Boarding()
        {
            if (vehicle == null || vehicle.getType() != ProductType.HUMAN)
            {
                return false;
            }

            int waitingPassengers = passangerBuffer.CurrentLoad();

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
            if (vehicle == null || vehicle.getType() != ProductType.HUMAN) { return false; }

            //vehicle.UnBoard() //TODO//

            return true;
        }
    }
}
