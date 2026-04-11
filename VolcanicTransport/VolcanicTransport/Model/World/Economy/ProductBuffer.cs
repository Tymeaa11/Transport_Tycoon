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

        public int ReciveProduct(Vehicle vehicle, int amount) //visszatérési érték: amennyit átvett 
        {
            if (vehicle.CurrentType != ProductType) return 0;

            int provided = vehicle.Unload(amount);
            int capacity = MaxCapacity - CurrentLoad;
            if (capacity < provided)
            {
                int plus = provided - capacity;
                vehicle.Load(plus, ProductType);
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
            if (vehicle == null || CurrentLoad <= 0 || vehicle.CurrentType != ProductType) { return 0; }

            var taken = vehicle.Load(CurrentLoad, ProductType);

            CurrentLoad -= taken;

            return taken;
        }
    }
}
