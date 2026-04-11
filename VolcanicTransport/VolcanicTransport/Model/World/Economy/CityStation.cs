using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityStation(City city, Coordinate coordinate, string name) : Station(coordinate, name, new ProductBuffer(ProductType.HUMAN, 50), new Product(ProductType.HUMAN, 0, 50, 5))
    {
        private readonly City _city = city;

        public override bool UnLoadProductFromVehicle(Vehicle vehicle)
        {
            if (vehicle == null || !_city.IsProductNeeded(vehicle.Type))
            {
                return false;
            }

            var amount = vehicle.CurrentLoad;
            var provided = vehicle.Unload(amount);

            return provided != 0;
            //int moneyGiven = city.RecieveProduct(vehicle.getType(), provided);

            return true;
        }
    }
}
