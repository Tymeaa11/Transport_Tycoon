using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class CityStation : Station
    {
        private City city;

        public CityStation(City city, Coordinate coordinate, string name) : base(coordinate, name, new ProductBuffer(ProductType.HUMAN, 50), new Product(ProductType.HUMAN, 0, 50, 5))
        {
            this.city = city;
        }

        public override bool Boarding()
        {
            throw new NotImplementedException();
        }

        public override bool UnLoadProductFromVehicle()
        {
            throw new NotImplementedException();
        }
    }
}
