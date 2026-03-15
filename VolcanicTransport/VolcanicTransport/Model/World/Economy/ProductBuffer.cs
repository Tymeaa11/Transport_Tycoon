using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class ProductBuffer
    {
        private ProductType productType;
        private int maxCapacity;
        private int currentLoad;

        public ProductBuffer(ProductType productType, int maxCapacity)
        {
            this.productType = productType;
            this.maxCapacity = maxCapacity;
            this.currentLoad = 0;
        }

        public int amountNeeded()
        {
            return this.maxCapacity - this.currentLoad; 
        }
        public int CurrentLoad() { return this.currentLoad; }
        public int ReciveProduct(ProductType type, int amount) //visszatérési érték: amennyit átvett 
        {
            if (productType != type) return 0;

            int canReceive = Math.Min(amount, maxCapacity - currentLoad);

            currentLoad += canReceive;

            return canReceive;
        }
        public int FillVehicle(Vehicle vehicle)
        {
            if ( vehicle == null || this.currentLoad <= 0 ) { return 0; }

            int taken = vehicle.Load(this.currentLoad);

            currentLoad -= taken;

            return taken;
        }
    }
}
