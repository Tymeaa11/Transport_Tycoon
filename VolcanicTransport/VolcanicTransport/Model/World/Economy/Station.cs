namespace VolcanicTransport.Model.World.Economy
{
    public abstract class Station implements KnowsNe
    {
        private Coordinate coordinate;
        private string name;
        private ProductBuffer passangerBuffer;
        private List<Vehicle> vehicles;
    }
}
