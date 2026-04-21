using System.Text;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;

namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryBuilding : ISurface, IInspectable
    {
        #region Fields
        private readonly Factory _factoryReference;

        public string Name => _factoryReference.Name;
        [JsonIgnore]
        public ProductType BaseProduct => _factoryReference.BaseProduct;
        [JsonIgnore]
        public ProductType FinalProduct => _factoryReference.FinalProduct.ProductType;
        [JsonIgnore]
        public int BaseProductNeed => _factoryReference.BaseProductBuffer.MaxCapacity;
        [JsonIgnore]
        public int BaseProductAmount => _factoryReference.BaseProductBuffer.CurrentLoad;
        [JsonIgnore]
        public int FinalProductAmount => _factoryReference.FinalProductBuffer.CurrentLoad;
        #endregion
        #region Constructors
        public FactoryBuilding(Factory factory)
        {
            _factoryReference = factory;
        }

        [JsonConstructor]
        public FactoryBuilding(string factoryName)
        {
            var targets = World.Instance.Factories.Where(c => c.Name == factoryName).ToList();

            if (targets.Count != 1)
                throw new LoadingException();

            _factoryReference = targets[0];
        }
        #endregion
        #region Methods
        public string Inspect()
        {
            var info = new StringBuilder();
            info.AppendLine($"Factory: {Name}");

            if (BaseProduct != ProductType.NONE)
                info.AppendLine($"Base product need / amount:  {BaseProduct} {BaseProductNeed}/{BaseProductAmount}");

            info.AppendLine($"Finished product / amount: {FinalProduct} {FinalProductAmount}");

            return info.ToString();
        }
        #endregion
    }
}
