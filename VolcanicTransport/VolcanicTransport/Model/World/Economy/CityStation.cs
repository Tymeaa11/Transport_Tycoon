using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityStation(City city, Coordinate coordinate, string name) : Station(coordinate, name, new ProductBuffer(ProductType.HUMAN, 50), new Product(ProductType.HUMAN, 0, 50, 5))
    {
        private readonly City _city = city;

        public override int UnLoadProductFromVehicle(Vehicle vehicle)
        {
            if (vehicle == null || !_city.IsProductNeeded(vehicle.CurrentType))
            {
                return 0;
            }

            var amount = vehicle.CurrentLoad;
            var provided = vehicle.Unload(amount);

            return provided;
        }
    }
}
