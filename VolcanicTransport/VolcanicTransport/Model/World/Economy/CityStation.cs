using System.Text;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityStation : Station, IInspectable
    {
        #region Fields
        public string CityName => _city.Name;

        [JsonIgnore]
        private readonly City _city;

        [JsonIgnore]
        public List<ProductType> GetCityProductNeeds => _city.ProductTypes;
        #endregion

        #region Constructors
        public CityStation(City city, Coordinate coordinate, string name) :
            base(
                coordinate,
                name,
                new ProductBuffer(ProductType.HUMAN, 50),
                new Product(ProductType.HUMAN, 0, 50, 5)
            )
        {
            _city = city;
        }

        [JsonConstructor]
        public CityStation(string cityName, Coordinate coordinate, string name, ProductBuffer passangerBuffer, Product passengerDemand)
            : base(coordinate, name, passangerBuffer, passengerDemand)
        {
            var targets = World.Instance.Cities.Where(c => c.Name == cityName).ToList();

            if (targets.Count != 1)
                throw new LoadingException();

            _city = targets[0];
        }
        #endregion

        #region Methods
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

        public string Inspect()
        {
            var info = new StringBuilder();

            info.AppendLine($"City Station: {Name}");
            info.AppendLine("Product needs:");

            foreach (ProductType pt in GetCityProductNeeds)
                info.AppendLine($"  - {pt}");

            info.AppendLine($"People waiting: {WaitingPassengers}");

            return info.ToString();
        }
        #endregion
    }
}
