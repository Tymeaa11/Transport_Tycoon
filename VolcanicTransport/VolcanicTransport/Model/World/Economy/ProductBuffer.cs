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

        public int ReceiveProduct(ProductType type, int amount) //visszatérési érték: amennyit átvett 
        {
            if (ProductType != type) return 0;

            var canReceive = Math.Min(amount, MaxCapacity - CurrentLoad);

            CurrentLoad += canReceive;

            return canReceive;
        }
        public int FillVehicle(Vehicle? vehicle)
        {
            if (vehicle == null || CurrentLoad <= 0) { return 0; }

            var taken = vehicle.Load(CurrentLoad);

            CurrentLoad -= taken;

            return taken;
        }
    }
}
