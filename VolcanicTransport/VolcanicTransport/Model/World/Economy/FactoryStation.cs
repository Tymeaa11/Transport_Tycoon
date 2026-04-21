using System.Text;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryStation(Coordinate coor, string name, Factory factory) : 
        Station(
            coor, 
            name, 
            new ProductBuffer(ProductType.HUMAN, 50), 
            new Product(ProductType.HUMAN, 0, 50, 5)
        )
        , IInspectable
    {
        #region Fields
        private readonly Factory _factory = factory;
        public string FactoryName => _factory.Name;

        [JsonIgnore]
        public ProductType GetFactoryNeeds => _factory.BaseProduct;
        [JsonIgnore]
        public ProductType GetFactoryFinishedProduct => _factory.FinalProduct.ProductType;
        [JsonIgnore]
        public int GetFactoryFinishedProductAmount => _factory.FinalProductBuffer.CurrentLoad;
        [JsonIgnore]
        public int GetFactoryBaseProductAmount => _factory.BaseProductBuffer.CurrentLoad;
        [JsonIgnore]
        public int GetFactoryNeedsAmount => _factory.BaseProductBuffer.MaxCapacity;
        #endregion

        #region Constructors
        public FactoryStation(City city, Coordinate coordinate, string name) :
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
        public FactoryStation(string cityName, Coordinate coordinate, string name, ProductBuffer passangerBuffer, Product passengerDemand)
            : base(coordinate, name, passangerBuffer, passengerDemand)
        {
            var targets = World.Instance.Cities.Where(c => c.Name == cityName).ToList();

            if (targets.Count != 1)
                throw new LoadingException();

            _city = targets[0];
        }
        #endregion

        #region Methods
        public float GetFactoryEfficiency(float time) => _factory.FinalProduct.GetFactoryEfficiency(time); // 0-1
        public double PricePerBaseProduct => GameSettings.GetPrice(_factory.BaseProduct);
        public int LoadProduct(Vehicle vehicle) // adott-e árut a járműnek
        {
            if (vehicle == null || (vehicle.CurrentLoad > 0 && vehicle.CurrentType != _factory.FinalProduct.ProductType))
            {
                return 0;
            }

            int amountFilled = _factory.FinalProductBuffer.FillVehicle(vehicle);

            return amountFilled;
        }


        public override int UnLoadProductFromVehicle(Vehicle vehicle) // kapott-e árut a járműtől
        {
            if (vehicle == null || vehicle.CurrentType != _factory.BaseProduct)
            {
                return 0;
            }

            int amountNeededForFactory = _factory.BaseProductBuffer.AmountNeeded();

            if (amountNeededForFactory == 0) { return 0; }

            int provided = _factory.BaseProductBuffer.ReciveProduct(vehicle, amountNeededForFactory);

            return provided;
        }

        
        public string Inspect()
        {
            var info = new StringBuilder();

            info.AppendLine($"Factory Station: {Name}");

            if (GetFactoryNeeds != ProductType.NONE)
                info.AppendLine($"Base product need / amount: {GetFactoryNeeds} {GetFactoryBaseProductAmount}/{GetFactoryNeedsAmount}");

            info.AppendLine($"Finished product / amount: {GetFactoryFinishedProduct} {GetFactoryFinishedProductAmount}");

            info.AppendLine($"People waiting: {WaitingPassengers}");

            return info.ToString();
        }
        #endregion
    }
}
