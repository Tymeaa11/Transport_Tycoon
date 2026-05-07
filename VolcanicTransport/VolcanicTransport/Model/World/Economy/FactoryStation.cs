using System.Text;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Persistance;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryStation : Station, IInspectable, IContainsReference
    {
        #region Fields
        public string FactoryName { get; private set; }

        [JsonIgnore]
        private Factory? _factory;
        [JsonIgnore]
        public ProductType GetFactoryNeeds => _factory!.BaseProduct;
        [JsonIgnore]
        public ProductType GetFactoryFinishedProduct => _factory!.FinalProduct.ProductType;
        [JsonIgnore]
        public int GetFactoryFinishedProductAmount => _factory!.FinalProductBuffer.CurrentLoad;
        [JsonIgnore]
        public int GetFactoryBaseProductAmount => _factory!.BaseProductBuffer.CurrentLoad;
        [JsonIgnore]
        public int GetFactoryNeedsAmount => _factory!.BaseProductBuffer.MaxCapacity;
        [JsonIgnore]
        public double PricePerBaseProduct => GameSettings.GetPrice(_factory!.BaseProduct);
        #endregion

        #region Constructors
        public FactoryStation(Coordinate coordinate, string stationName, Factory factory) :
        base (
            coordinate,
            stationName,
            new ProductBuffer(ProductType.HUMAN, 50),
            new Product(ProductType.HUMAN, 0, 50, 5)
        )
        {
            _factory = factory;
            FactoryName = factory.Name;
        }

        [JsonConstructor]
        public FactoryStation(string factoryName, Coordinate coordinate, string stationName, ProductBuffer passengerBuffer, Product passengerDemand, double passengerAccumulator)
            : base(coordinate, stationName, passengerBuffer, passengerDemand, passengerAccumulator)
        {
            FactoryName = factoryName;
        }
        #endregion

        #region Methods
        public float GetFactoryEfficiency(float time) 
            => _factory!.FinalProduct.GetFactoryEfficiency(time); // 0-1
        
        public int LoadProduct(Vehicle vehicle) // adott-e árut a járműnek
        {
            if (vehicle == null || (vehicle.CurrentLoad > 0 && vehicle.CurrentType != _factory!.FinalProduct.ProductType))
            {
                return 0;
            }

            int amountFilled = _factory!.FinalProductBuffer.FillVehicle(vehicle);

            return amountFilled;
        }

        public override int UnLoadProductFromVehicle(Vehicle vehicle) // kapott-e árut a járműtől
        {
            if (vehicle == null || vehicle.CurrentType != _factory!.BaseProduct)
            {
                return 0;
            }

            int amountNeededForFactory = _factory.BaseProductBuffer.AmountNeeded();

            if (amountNeededForFactory == 0) { return 0; }

            int provided = _factory.BaseProductBuffer.ReceiveProduct(vehicle, amountNeededForFactory);

            return provided;
        }

        
        public string Inspect()
        {
            var info = new StringBuilder();

            info.AppendLine($"Factory Station: {StationName}");

            if (GetFactoryNeeds != ProductType.NONE)
                info.AppendLine($"Base product need / amount: {GetFactoryNeeds} {GetFactoryBaseProductAmount}/{GetFactoryNeedsAmount}");

            info.AppendLine($"Finished product / amount: {GetFactoryFinishedProduct} {GetFactoryFinishedProductAmount}");

            info.AppendLine($"People waiting: {WaitingPassengers}");

            return info.ToString();
        }

        public void RestoreReference(Coordinate coordinate)
        {
            var targets = World.Instance.Factories.Where(c => c.Name == FactoryName).ToList();

            if (targets.Count != 1)
                throw new LoadingException();

            _factory = targets[0];
        }
        #endregion
    }
}
