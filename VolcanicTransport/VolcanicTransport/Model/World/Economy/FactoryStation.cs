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
        private Factory FactoryRef => _factory
            ?? throw new InvalidOperationException($"RestoreReference() not yet called for FactoryStation '{FactoryName}'.");

        [JsonIgnore]
        public ProductType GetFactoryNeeds => FactoryRef.BaseProduct;
        [JsonIgnore]
        public ProductType GetFactoryFinishedProduct => FactoryRef.FinalProduct.ProductType;
        [JsonIgnore]
        public int GetFactoryFinishedProductAmount => FactoryRef.FinalProductBuffer.CurrentLoad;
        [JsonIgnore]
        public int GetFactoryBaseProductAmount => FactoryRef.BaseProductBuffer.CurrentLoad;
        [JsonIgnore]
        public int GetFactoryNeedsAmount => FactoryRef.BaseProductBuffer.MaxCapacity;
        [JsonIgnore]
        public double PricePerBaseProduct => GameSettings.GetPrice(FactoryRef.BaseProduct);
        #endregion

        #region Constructors
        public FactoryStation(Coordinate coordinate, string stationName, Factory factory) :
        base(
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
            => FactoryRef.FinalProduct.GetFactoryEfficiency(time);

        public int LoadProduct(Vehicle vehicle)
        {
            if (vehicle == null || (vehicle.CurrentLoad > 0 && vehicle.CurrentType != FactoryRef.FinalProduct.ProductType))
            {
                return 0;
            }

            int amountFilled = FactoryRef.FinalProductBuffer.FillVehicle(vehicle);

            return amountFilled;
        }

        public override int UnLoadProductFromVehicle(Vehicle vehicle)
        {
            if (vehicle == null || vehicle.CurrentType != FactoryRef.BaseProduct)
            {
                return 0;
            }

            int amountNeededForFactory = FactoryRef.BaseProductBuffer.AmountNeeded();

            if (amountNeededForFactory == 0) { return 0; }

            int provided = FactoryRef.BaseProductBuffer.ReceiveProduct(vehicle, amountNeededForFactory);

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
                throw new PersistanceException();

            _factory = targets[0];
        }
        #endregion
    }
}
