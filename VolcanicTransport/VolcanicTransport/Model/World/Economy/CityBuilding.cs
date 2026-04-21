using System.Text;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityBuilding : ISurface, IInspectable
    {
        #region Fields
        public string Name => _cityReference.Name;

        [JsonIgnore]
        public List<ProductType> ProductTypes => _cityReference.ProductTypes;
        [JsonIgnore]
        private readonly City _cityReference;
        #endregion
        #region Constructors

        public CityBuilding(City city)
        {
            _cityReference = city;
        }

        public CityBuilding(string cityName)
        {
            var targets = World.Instance.Cities.Where(c => c.Name == cityName).ToList();

            if (targets.Count != 1) 
                throw new LoadingException();

            _cityReference = targets[0];
        }
        #endregion
        #region Methods

        public string Inspect()
        {
            var info = new StringBuilder();
            info.AppendLine($"City: {Name}");
            info.AppendLine("Product needs:");

            foreach (ProductType pt in ProductTypes)
                info.AppendLine($"  - {pt}");

            return info.ToString();
        }
        #endregion
    }
}
