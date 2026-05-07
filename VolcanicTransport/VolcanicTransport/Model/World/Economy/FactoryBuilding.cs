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

        public string FactoryName { get; private set; }
        [JsonIgnore]
        public ProductType BaseProduct => _factoryReference!.BaseProduct;
        [JsonIgnore]
        public ProductType FinalProduct => _factoryReference!.FinalProduct.ProductType;
        [JsonIgnore]
        public int BaseProductNeed => _factoryReference!.BaseProductBuffer.MaxCapacity;
        [JsonIgnore]
        public int BaseProductAmount => _factoryReference!.BaseProductBuffer.CurrentLoad;
        [JsonIgnore]
        public int FinalProductAmount => _factoryReference!.FinalProductBuffer.CurrentLoad;
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
                throw new LoadingException();

            _factoryReference = targets[0];

            var f = World.Instance.GetField(coordinate) ?? throw new LoadingException();
            targets[0].AddField(f);
        }
        #endregion
    }
}
