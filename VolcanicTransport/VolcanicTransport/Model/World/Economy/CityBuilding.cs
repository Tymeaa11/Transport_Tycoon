namespace VolcanicTransport.Model.World.Economy
{
    public class CityBuilding : ISurface
    {
        private City city;

        public string Name => city.Name;

        public List<ProductType> ProductTypes => city.ProductTypes;

        public CityBuilding(City city)
        {
            this.city = city;
        }
    }
}
