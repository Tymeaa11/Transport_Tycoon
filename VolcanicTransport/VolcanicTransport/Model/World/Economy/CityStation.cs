using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityStation(City city, Coordinate coordinate, string name) : Station(coordinate, name, new ProductBuffer(ProductType.HUMAN, 50), new Product(ProductType.HUMAN, 0, 50, 5))
    {
        private readonly City _city = city;

        public override bool UnLoadProductFromVehicle()
        {
            if (vehicle == null || !_city.IsProductNeeded(vehicle.Type))
            {
                return false;
            }

            int amount = vehicle.CurrentLoad;

            int provided = vehicle.Unload(amount);

            if (provided == 0)
            {
                return false;
            }

            //int moneyGiven = city.RecieveProduct(vehicle.getType(), provided);
            //hogy legyen a pénz?
            return true;
        }
    }
}
