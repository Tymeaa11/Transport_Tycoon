namespace VolcanicTransport.Model.World.Economy
{
    public class CityBuilding(string cityName) : ISurface
    {
        public string CityName { get; } = cityName;
    }
}
