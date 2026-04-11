using System.Text.Json.Serialization;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    [method: JsonConstructor]
    public class ProductBuffer(ProductType productType, int maxCapacity, int currentLoad = 0)
    {
        #region Fields
        public ProductType ProductType { get; } = productType;
        public int MaxCapacity { get; } = maxCapacity;
        public int CurrentLoad { get; private set; } = currentLoad;

        #endregion

        public int AmountNeeded() => MaxCapacity - CurrentLoad;
        /*
        public int ReciveProduct(ProductType type, int amount) //visszatérési érték: amennyit átvett 
        {
            if (ProductType != type) return 0;

            var canReceive = Math.Min(amount, MaxCapacity - CurrentLoad);

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

            var taken = vehicle.Load(CurrentLoad);

            CurrentLoad -= taken;

            return taken;
        }
    }
}
