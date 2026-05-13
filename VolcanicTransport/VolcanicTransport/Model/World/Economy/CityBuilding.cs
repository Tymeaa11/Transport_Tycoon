using System.Text;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Persistence;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityBuilding : ISurface, IInspectable, IContainsReference
    {
        #region Fields
        public string CityName { get; private set; }

        [JsonIgnore]
        private City? _cityReference;

        private City CityRef => _cityReference
            ?? throw new InvalidOperationException($"RestoreReference() not yet called for CityBuilding '{CityName}'.");

        [JsonIgnore]
        public List<ProductType> ProductTypes => CityRef.ProductTypes;
        #endregion
        #region Constructors

        public CityBuilding(City city)
        {
            _cityReference = city;
            CityName = city.Name;
        }

        [JsonConstructor]
        public CityBuilding(string cityName)
        {
            CityName = cityName;
        }
        #endregion
        #region Methods

        public string Inspect()
        {
            var info = new StringBuilder();
            info.AppendLine($"City: {CityName}");
            info.AppendLine("Product needs:");

            foreach (ProductType pt in ProductTypes)
                info.AppendLine($"  - {pt}");

            return info.ToString();
        }

        public void RestoreReference(Coordinate coordinate)
        {
            var targets = World.Instance.Cities.Where(c => c.Name == CityName).ToList();

            if (targets.Count != 1)
                throw new PersistanceException();

            _cityReference = targets[0];

            var f = World.Instance.GetField(coordinate) ?? throw new PersistanceException();
            targets[0].AddField(f);
        }
        #endregion
    }
}
