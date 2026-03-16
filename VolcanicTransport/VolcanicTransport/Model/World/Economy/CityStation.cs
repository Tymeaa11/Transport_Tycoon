using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityStation : Station
    {
        private City city;

        public CityStation(City city, Coordinate coordinate, string name) : base(coordinate, name, new ProductBuffer(ProductType.HUMAN, 50), new Product(ProductType.HUMAN, 0, 50, 5))
        {
            this.city = city;
        }

        public override bool UnLoadProductFromVehicle()
        {
            if (vehicle == null || !city.IsProductNeeded(vehicle.getType()))
            {
                return false;
            }

            int amount = vehicle.CurrenLoad();

            int provided = vehicle.Unload(amount);

            if (provided == 0)
            {
                return false;
            }

            int moneyGiven = city.RecieveProduct(vehicle.getType(), provided);
            //hogy legyen a pénz?
            return true;
        }
    }
}
