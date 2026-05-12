using System.Text;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Persistance;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryBuilding : ISurface, IInspectable, IContainsReference
    {
        #region Fields
        private Factory? _factoryReference;

        private Factory FactoryRef => _factoryReference
            ?? throw new InvalidOperationException($"RestoreReference() not yet called for FactoryBuilding '{FactoryName}'.");

        public string FactoryName { get; private set; }
        [JsonIgnore]
        public ProductType BaseProduct => FactoryRef.BaseProduct;
        [JsonIgnore]
        public ProductType FinalProduct => FactoryRef.FinalProduct.ProductType;
        [JsonIgnore]
        public int BaseProductNeed => FactoryRef.BaseProductBuffer.MaxCapacity;
        [JsonIgnore]
        public int BaseProductAmount => FactoryRef.BaseProductBuffer.CurrentLoad;
        [JsonIgnore]
        public int FinalProductAmount => FactoryRef.FinalProductBuffer.CurrentLoad;
        #endregion
        #region Constructors
        public FactoryBuilding(Factory factory)
        {
            _factoryReference = factory;
            FactoryName = factory.Name;
        }

        [JsonConstructor]
        public FactoryBuilding(string factoryName)
        {
            FactoryName = factoryName;
        }
        #endregion
        #region Methods
        public string Inspect()
        {
            var info = new StringBuilder();
            info.AppendLine($"Factory: {FactoryName}");

            if (BaseProduct != ProductType.NONE)
                info.AppendLine($"Base product need / amount:  {BaseProduct} {BaseProductNeed}/{BaseProductAmount}");

            info.AppendLine($"Finished product / amount: {FinalProduct} {FinalProductAmount}");

            return info.ToString();
        }

        public void RestoreReference(Coordinate coordinate)
        {
            var targets = World.Instance.Factories.Where(c => c.Name == FactoryName).ToList();

            if (targets.Count != 1)
                throw new PersistanceException();

            _factoryReference = targets[0];

            var f = World.Instance.GetField(coordinate) ?? throw new PersistanceException();
            targets[0].AddField(f);
        }
        #endregion
    }
}
