using System.Text;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityBuilding(City city) : ISurface, IInspectable
    {
        public string Name => city.Name;
        public List<ProductType> ProductTypes => city.ProductTypes;

        public string Inspect()
        {
            var info = new StringBuilder();
            info.AppendLine($"City: {Name}");
            info.AppendLine("Product needs:");

            foreach (ProductType pt in ProductTypes)
                info.AppendLine($"  - {pt}");

            return info.ToString();
        }
    }
}
