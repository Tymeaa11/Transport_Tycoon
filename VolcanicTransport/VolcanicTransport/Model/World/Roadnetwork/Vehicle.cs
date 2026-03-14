using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
namespace VolcanicTransport.Model.World.Roadnetwork
{
    public abstract class Vehicle
    {
        public string Name { get; protected set; }
        public ProductType CargoType { get; protected set; }
        public int Capacity { get; protected set; }
        public int CurrentLoad { get; protected set; }
        public int Price { get; protected set; }
        public int MaintenanceCost { get; protected set; }

        public double MaxSpeed { get; protected set; }
        public double CurrentSpeed { get; protected set; } 
        public bool IsActive { get; protected set; }
        public bool IsStuck { get; protected set; }

        public Coordinate CurrentCoordinate { get; protected set; }
        protected Route? Schedule { get; set; }
        protected GraphEdge? CurrentEdge { get; set; }

        public Vehicle(string name, double maxSpeed, int capacity, int price, ProductType type)
        {
            Name = name;
            MaxSpeed = maxSpeed;
            Capacity = capacity;
            Price = price;
            CargoType = type;
            MaintenanceCost = (int)(price * 0.05);

            IsActive = false;
            IsStuck = false;
            CurrentLoad = 0;

        }

        public void AssignSchedule(Route newRoute)
        {
            if (!IsActive) Schedule = newRoute;
        }

        public virtual void Activate()
        { 
            IsActive = true;
        }

        public virtual void Deactivate()
        {
            IsActive = false;
            CurrentLoad = 0;
            CurrentEdge = null;
        }

        public int Sell()
        {
            if (IsActive) Deactivate();
            return (int)(Price * 0.7);
        }

        private void Move(float deltaTime)
        {
            
        }

        private void RecalculatePath()
        {

        }

        public int Load(int amount)
        {
            int spaceLeft = Capacity - CurrentLoad;
            int taken = Math.Min(amount, spaceLeft);
            CurrentLoad += taken;
            return taken;
        }

        public int Unload(int amountNeeded)
        {
            int provided = Math.Min(CurrentLoad, amountNeeded);
            CurrentLoad -= provided;
            return provided;
        }

        public ProductType getType()
        {
            return CargoType;
        }

    }
}
