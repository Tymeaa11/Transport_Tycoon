using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Station(Coordinate coordinate, string name, ProductBuffer passangerBuffer, Product passengerDemand) : KnowsNeighbour(coordinate)
    {
        protected string name = name;
        protected readonly ProductBuffer passangerBuffer = passangerBuffer;
        protected Vehicle? vehicle = null;
        protected Product passengerDemand = passengerDemand;

        public abstract bool UnLoadProductFromVehicle();
        public bool Boarding()
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

        public bool UnBoarding()
        {
            if (vehicle is not { Type: ProductType.HUMAN }) 
                return false;

            //vehicle.UnBoard() //TODO//

            return true;
        }
    }
}
