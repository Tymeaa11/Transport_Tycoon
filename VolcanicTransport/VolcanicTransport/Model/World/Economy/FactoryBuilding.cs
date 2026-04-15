using System.Text;

namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryBuilding(Factory factory) : ISurface, IInspectable
    {
        public string Name => factory.Name;

        public ProductType BaseProduct => factory.BaseProduct;

        public ProductType FinalProduct => factory.FinalProduct.ProductType;

        public int BaseProductNeed => factory.BaseProductBuffer.MaxCapacity;

        public int BaseProductAmount => factory.BaseProductBuffer.CurrentLoad;

        public int FinalProductAmount => factory.FinalProductBuffer.CurrentLoad;

        public string Inspect()
        {
            var info = new StringBuilder();
            info.AppendLine($"Factory: {factory.Name}");

            if (BaseProduct != ProductType.NONE)
                info.AppendLine($"Base product need / amount:  {BaseProduct} {BaseProductNeed}/{BaseProductAmount}");

            info.AppendLine($"Finished product / amount: {FinalProduct} {FinalProductAmount}");

            return info.ToString();
        }
    }
}
