using System.ComponentModel;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class ProductBuffer(ProductType productType, int maxCapacity)
    {
        private readonly ProductType _productType = productType;
        private readonly int _maxCapacity = maxCapacity;
        public int CurrentLoad { get; private set; } = 0;

        public int AmountNeeded() => _maxCapacity - CurrentLoad;
        /*
        public int ReciveProduct(ProductType type, int amount) //visszatérési érték: amennyit átvett 
        {
            if (_productType != type) return 0;

            int canReceive = Math.Min(amount, _maxCapacity - CurrentLoad);

            CurrentLoad += canReceive;

            return canReceive;
        }*/
        public int ReciveProduct(Vehicle vehicle, int amount) //visszatérési érték: amennyit átvett 
        {
            if (vehicle.Type != _productType) return 0;

            int provided = vehicle.Unload(amount);
            int capacity = _maxCapacity - CurrentLoad;
            if (capacity < provided)
            {
                int plus = provided - capacity;
                vehicle.Load(plus);
                CurrentLoad += capacity;
                return capacity;
            } else
            {
                CurrentLoad += provided;
                return provided;
            }
        }

        public int FillVehicle(Vehicle vehicle) //visszatérési érték: amennyit leadott
        {
            if (vehicle == null || CurrentLoad <= 0 || vehicle.Type != _productType) { return 0; }

            int taken = vehicle.Load(CurrentLoad);

            CurrentLoad -= taken;

            return taken;
        }
    }
}
