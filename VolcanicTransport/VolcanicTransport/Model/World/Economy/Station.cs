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
        public abstract bool Boarding();
        public bool IsCompatible() => true;
    }
}
