using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
namespace VolcanicTransport.Model.World.Roadnetwork
{
    public abstract class Vehicle
    {
        protected string name;
        protected ProductType type;
        protected Route route;
        protected double currentSpeed;
        protected double maxSpeed;
        protected int capacity;
        protected int currentLoad;
        protected GraphEdge currentEdge;
        protected Coordinate currentCoordinate;
        protected int maintenanceCost;
        protected int price;
        protected bool active;

        public Vehicle(string name, double maxSpeed, int capacity, int price)
        {
            this.name = name;
            this.maxSpeed = maxSpeed;
            this.capacity = capacity;
            this.price = price;
            currentSpeed = 0;
            this.maintenanceCost = (int)(price * 0.05);
            currentLoad = 0;
            active = false;

        }

        public int Price() => price;

        public string VehicleName() => name;

        public void Activate()
        {
            active = true;
        }

        public void DeActivate()
        {
            active = false;
        }

        private void Move(float deltaTime)
        {
            if (currentEdge == null || route == null) return;
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += 2.0 * deltaTime;
                if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;
            }

            double distanceTravelled = (currentSpeed / 3.6) * deltaTime;

            // Itt jönne a logika, ami frissíti a currentCoordinate értéket 
            // az aktuális él (currentEdge) mentén.
            //UpdatePosition(distanceTravelled);
        }

        private void RecalculatePath()
        {

        }

        public virtual (bool, int) Load(int amount)
        {
            if (currentLoad < capacity)
            {
                int spaceLeft = capacity - currentLoad;
                if (spaceLeft > amount)
                {
                    currentLoad = capacity;
                    return (true, amount - spaceLeft);
                }
                else
                {
                    currentLoad += amount;
                    return (true, 0);
                }
            }
            return (false, amount);
        }

        public virtual (bool, int) UnLoad(int need)
        {
            if (currentLoad > 0)
            {
                if (need > currentLoad)
                {
                    int amountLeft = currentLoad - need;
                    currentLoad = 0;
                    return (true, amountLeft);
                }
                else
                {
                    currentLoad -= need;
                    return (true, 0);
                }
            }
            return (false, need);
        }
    }
}
