using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World.Economy
{
    public class ProductBuffer
    {
        private ProductType productType;
        private int maxCapacity;
        private int currentLoad;

        public ProductBuffer(ProductType productType, int maxCapacity, int currentLoad)
        {
            this.productType = productType;
            this.maxCapacity = maxCapacity;
            this.currentLoad = currentLoad;
        }

        public int ReciveProduct(ProductType type, int amount)
        {
            if (productType != type)
            {
                return 0;
            }
            if (maxCapacity - currentLoad > amount)
            {
                amount = maxCapacity - currentLoad;
                currentLoad += amount;
                return amount;

            }
            currentLoad += amount;
            return amount;
        }
        public void FillVehicle(Vehicle vehicle)
        {
            //TODO//
        }
    }
}
