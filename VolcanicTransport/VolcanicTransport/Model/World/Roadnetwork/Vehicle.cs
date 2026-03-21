using System.ComponentModel;
using System.Numerics;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
namespace VolcanicTransport.Model.World.Roadnetwork
{
    public abstract class Vehicle(string name, float maxSpeed, int capacity, int price, ProductType type) : INotifyPropertyChanged
    {
        public string Name { get; } = name;
        public ProductType Type { get; protected set; } = type;
        public int CurrentLoad { get; protected set; } = 0;
        public int Price { get; } = price;
        protected Route? route = null;

        protected float currentSpeed = 0;
        protected float maxSpeed = maxSpeed;
        protected int Capacity = capacity;

        protected int maintenanceCost = (int)(price * 0.05);
        protected bool active = false;
        public VehicleState State { get; protected set; } = VehicleState.Moving;

        protected List<Field> currentPath = [];
        protected int currentPathIndex;
        public Field? CurrentField { get; protected set; }

        protected RoadEdge? currentEdge = null;
        protected Coordinate? currentCoordinate = null;

        protected List<Vector2> currentWaypoints = [];
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

        public void Update(double deltaTime)
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
                currentSpeed += 50.0f * (float)deltaTime;
                if (currentSpeed > targetSpeed) currentSpeed = targetSpeed;
            }
            else if (currentSpeed > targetSpeed)
            {
                currentSpeed -= 100.0f * (float)deltaTime;
                if (currentSpeed < targetSpeed) currentSpeed = targetSpeed;
            }
            float distanceToTravel = (currentSpeed / 3.6f) * (float)deltaTime;

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
                Vector2 direction = new(targetWaypoint.X - VisualPosition.X, targetWaypoint.Y - VisualPosition.Y);
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
                if (CurrentField == null) throw new Exception();
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

            if (CurrentField == null) throw new Exception();
            ReleaseJunctionLock(CurrentField);

            CurrentField.VehiclesOnField.Remove(this);
            nextField.VehiclesOnField.Add(this);

            CurrentField = nextField;
            currentPathIndex++;

            LoadWaypointsForField();
            return true;
        }

        private void LoadWaypointsForField()
        {
            currentWaypoints.Clear();
            currentWaypointIndex = 0;

            Field? previousField = (currentPathIndex > 0) ? currentPath[currentPathIndex - 1] : null;
            Field? nextField = (currentPathIndex < currentPath.Count - 1) ? currentPath[currentPathIndex + 1] : null;



            if (CurrentField == null) throw new Exception();

            string entryDirection = GetRelativeDirection(CurrentField, previousField);
            string exitDirection = GetRelativeDirection(CurrentField, nextField);

            string pathKey = $"{entryDirection}_To_{exitDirection}";

            if (previousField == null) pathKey = $"Start_To_{exitDirection}";
            if (nextField == null) pathKey = $"{entryDirection}_To_End";

            if (WaypointManager.Paths.TryGetValue(pathKey, out List<Vector2>? localPoints))
            {
                float startX = CurrentField.Coordinate.X * 64;
                float startY = CurrentField.Coordinate.Y * 64;

                Vector2 fieldOffset = new(startX, startY);

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
            if (CurrentField == null) throw new Exception();
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
        }

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
    }
}
