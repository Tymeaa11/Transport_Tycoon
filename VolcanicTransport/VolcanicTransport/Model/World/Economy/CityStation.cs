using System.Text;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Persistance;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityStation : Station, IInspectable, IContainsReference
    {
        #region Fields
        public string CityName { get; private set; }

        [JsonIgnore]
        private City? _city;

        private City CityRef => _city
            ?? throw new InvalidOperationException($"RestoreReference() not yet called for CityStation '{CityName}'.");

        [JsonIgnore]
        public List<ProductType> GetCityProductNeeds => CityRef.ProductTypes;
        #endregion

        #region Constructors
        public CityStation(City city, Coordinate coordinate, string stationName) :
            base(
                coordinate,
                stationName,
                new ProductBuffer(ProductType.HUMAN, 50),
                new Product(ProductType.HUMAN, 0, 50, 5)
            )
        {
            _city = city;
            CityName = city.Name;
        }

        [JsonConstructor]
        public CityStation(string cityName, Coordinate coordinate, string stationName, ProductBuffer passengerBuffer, Product passengerDemand, double passengerAccumulator)
            : base(coordinate, stationName, passengerBuffer, passengerDemand, passengerAccumulator)
        {
            CityName = cityName;
        }
        #endregion

        #region Methods
        public override int UnLoadProductFromVehicle(Vehicle vehicle)
        {
            if (vehicle == null || !CityRef.IsProductNeeded(vehicle.CurrentType))
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

            info.AppendLine($"City Station: {StationName}");
            info.AppendLine("Product needs:");

            foreach (ProductType pt in GetCityProductNeeds)
                info.AppendLine($"  - {pt}");

            info.AppendLine($"People waiting: {WaitingPassengers}");

            return info.ToString();
        }

        public void RestoreReference(Coordinate coordinate)
        {
            var targets = World.Instance.Cities.Where(c => c.Name == CityName).ToList();

            if (targets.Count != 1)
                throw new PersistanceException();

            _city = targets[0];
        }
        #endregion
    }
}
