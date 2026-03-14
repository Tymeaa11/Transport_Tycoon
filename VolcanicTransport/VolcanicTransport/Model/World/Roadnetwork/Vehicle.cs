using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
namespace VolcanicTransport.Model.World.Roadnetwork
{
    public abstract class Vehicle : INotifyPropertyChanged
    {
        protected string name;
        protected ProductType type;
        protected Route? route = null;

        protected float currentSpeed;
        protected float maxSpeed;
        protected int Capacity;
        protected int CurrentLoad;
        protected int maintenanceCost;
        protected int price;
        protected bool active;
        public VehicleState State { get; protected set; } = VehicleState.Moving;

        protected List<Field> currentPath;
        protected int currentPathIndex;
        public Field CurrentField { get; protected set; }

        protected RoadEdge? currentEdge = null;
        protected Coordinate? currentCoordinate = null;

        protected List<Vector2> currentWaypoints = new List<Vector2>();
        protected int currentWaypointIndex = 0;

        private Vector2 _visualPosition;
        public Vector2 VisualPosition
        {
            get => _visualPosition;
            set
            {
                _visualPosition = value;
                OnPropertyChanged(nameof(VisualPosition));
            }
        }

        public Vehicle(string name, float maxSpeed, int capacity, int price, ProductType type)
        {
            this.name = name;
            this.maxSpeed = maxSpeed;
            this.Capacity = capacity;
            this.price = price;
            currentSpeed = 0;
            this.maintenanceCost = (int)(price * 0.05);
            CurrentLoad = 0;
            active = false;

        }

        public void AssignSchedule(Route newRoute)
        public int Price() => price;
        public virtual void Activate()
        public string VehicleName() => name;
        public virtual void Deactivate()
        public void Activate()
            IsActive = false;
            active = true;

        public int Sell()
        public void DeActivate()
            if (IsActive) Deactivate();
            active = false;

        public void StartJourney(List<Field> path)
        {
            if (currentEdge == null || route == null) return;
            if (path == null || path.Count == 0) return;

            currentPath = path;
            currentPathIndex = 0;
            CurrentField = currentPath[0];

            CurrentField.VehiclesOnField.Add(this);

            LoadWaypointsForField();

            if (currentWaypoints.Count > 0)
            {
                VisualPosition = currentWaypoints[0];
            }

            State = VehicleState.Moving;
        }

        public void Update(float deltaTime)
        {
            if (State != VehicleState.Moving || currentPath == null || currentWaypoints.Count == 0) return;

            float targetSpeed = maxSpeed;
            Vehicle? vehicleAhead = GetVehicleAhead();

            if (vehicleAhead != null)
            {
                float distanceToFront = Vector2.Distance(this.VisualPosition, vehicleAhead.VisualPosition);
                if (distanceToFront < 20.0f)
                {
                    targetSpeed = Math.Min(targetSpeed, vehicleAhead.currentSpeed);
                }
            }

            if (currentSpeed < targetSpeed)
            {
                currentSpeed += 50.0f * deltaTime;
                if (currentSpeed > targetSpeed) currentSpeed = targetSpeed;
            }
            else if (currentSpeed > targetSpeed)
            {
                currentSpeed -= 100.0f * deltaTime;
                if (currentSpeed < targetSpeed) currentSpeed = targetSpeed;
            }
            float distanceToTravel = (currentSpeed / 3.6f) * deltaTime;

            while (distanceToTravel > 0)
            {
                if (currentWaypointIndex >= currentWaypoints.Count)
                {
                    if (!TryTransitionToNextField())
                    {
                        currentSpeed = 0;
                        break;
                    }
                }

                Vector2 targetWaypoint = currentWaypoints[currentWaypointIndex];
                Vector2 direction = new Vector2(targetWaypoint.X - VisualPosition.X, targetWaypoint.Y - VisualPosition.Y);
                float distanceToWaypoint = direction.Length();

                if (distanceToTravel >= distanceToWaypoint)
                {
                    VisualPosition = targetWaypoint;
                    distanceToTravel -= distanceToWaypoint;
                    currentWaypointIndex++;
                }
                else
                {
                    direction = Vector2.Normalize(direction);
                    VisualPosition += direction * distanceToTravel;
                    distanceToTravel = 0f;
                }
            }
        }

        private bool TryTransitionToNextField()
        {
            if (currentPathIndex + 1 >= currentPath.Count)
            {
                State = VehicleState.Loading;
                ReleaseJunctionLock(CurrentField);
                return false; 
            }
            Field nextField = currentPath[currentPathIndex + 1];
            bool isNextJunction = IsFieldJunction(nextField);

            if (isNextJunction)
            {
                if (nextField.ReservedBy != null && nextField.ReservedBy != this)
                {
                    return false;
                }
                nextField.ReservedBy = this;
            }

            ReleaseJunctionLock(CurrentField); 
            
            CurrentField.VehiclesOnField.Remove(this);
            nextField.VehiclesOnField.Add(this);

            CurrentField = nextField;
            currentPathIndex++;

            LoadWaypointsForField();
            return true;
        }

        public virtual (bool, int) Load(int amount)
        private void LoadWaypointsForField()
        {
            currentWaypoints.Clear();
            currentWaypointIndex = 0;

            Field? previousField = (currentPathIndex > 0) ? currentPath[currentPathIndex - 1] : null;
            Field? nextField = (currentPathIndex < currentPath.Count - 1) ? currentPath[currentPathIndex + 1] : null;

            string entryDirection = GetRelativeDirection(CurrentField, previousField);
            string exitDirection = GetRelativeDirection(CurrentField, nextField);

            string pathKey = $"{entryDirection}_To_{exitDirection}";

            if (previousField == null) pathKey = $"Start_To_{exitDirection}";
            if (nextField == null) pathKey = $"{entryDirection}_To_End";

            if (WaypointManager.Paths.TryGetValue(pathKey, out List<Vector2>? localPoints))
            {
                float startX = CurrentField.Coordinate.X * 64;
                float startY = CurrentField.Coordinate.Y * 64;

                Vector2 fieldOffset = new Vector2(startX, startY);

                foreach (var p in localPoints)
                {
                    currentWaypoints.Add(p + fieldOffset);
                }
            }
        }

        private string GetRelativeDirection(Field center, Field target)
        {
            if (target == null) return "None";
            if (target.Coordinate.Y < center.Coordinate.Y) return "North";
            if (target.Coordinate.Y > center.Coordinate.Y) return "South";
            if (target.Coordinate.X > center.Coordinate.X) return "East";
            if (target.Coordinate.X < center.Coordinate.X) return "West";
            return "None";
        }

        private Vehicle? GetVehicleAhead()
        {
            foreach (var other in CurrentField.VehiclesOnField)
            {
                if (other != this && other.State == VehicleState.Moving)
                {
                    if (other.currentPathIndex >= this.currentPathIndex)
                    {
                        return other;
                    }
                }
            }
            return null;
        }
        private void ReleaseJunctionLock(Field field)
        {
            if (IsFieldJunction(field) && field.ReservedBy == this)
            {
                field.ReservedBy = null;
            }

        public virtual (bool, int) UnLoad(int need)
        private bool IsFieldJunction(Field field)
        {
            if (field.Surface is Road road)
            {
                return road.RoadType.HasFlag(RoadType.JUNCTION);
            }
            return false;
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


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


}
