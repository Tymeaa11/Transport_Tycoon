namespace VolcanicTransport.Model.World.Economy
{
    public class CityBuilding(City city) : ISurface
    {
        public string Name => city.Name;
        public List<ProductType> ProductTypes => city.ProductTypes;
    }
}
