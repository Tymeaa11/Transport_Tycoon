using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class ProductBuffer(ProductType productType, int maxCapacity)
    {
        private readonly ProductType _productType = productType;
        private readonly int _maxCapacity = maxCapacity;
        public int CurrentLoad { get; private set; } = 0;

        public int AmountNeeded() => _maxCapacity - CurrentLoad;

        public int ReciveProduct(ProductType type, int amount) //visszatérési érték: amennyit átvett 
        {
            if (_productType != type) return 0;

            int canReceive = Math.Min(amount, _maxCapacity - CurrentLoad);

            CurrentLoad += canReceive;

            return canReceive;
        }
        public int FillVehicle(Vehicle vehicle)
        {
            if (vehicle == null || CurrentLoad <= 0) { return 0; }

            int taken = vehicle.Load(CurrentLoad);

            CurrentLoad -= taken;

            return taken;
        }
    }
}
