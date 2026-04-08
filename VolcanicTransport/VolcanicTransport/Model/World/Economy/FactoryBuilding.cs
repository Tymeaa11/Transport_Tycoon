namespace VolcanicTransport.Model.World.Economy
{
    public class FactoryBuilding(string factoryName) : ISurface
    {
        public string FactoryName { get; } = factoryName;
    }
}
