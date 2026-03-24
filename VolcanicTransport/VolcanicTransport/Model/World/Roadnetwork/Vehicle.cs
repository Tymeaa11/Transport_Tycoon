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

        private float _visualAngle;
        public float VisualAngle
        {
            get => _visualAngle;
            set
            {
                if (_visualAngle != value)
                {
                    _visualAngle = value;
                    OnPropertyChanged(nameof(VisualAngle));
                }
            }
        }


        public Route? Route { get { return route; } set { route = value; } }

        protected float currentSpeed = 0;
        protected float maxSpeed = maxSpeed;
        protected int capacity = capacity;

        public float MaxSpeed { get { return maxSpeed; } }
        public int Capacity { get { return capacity; } }

        protected int maintenanceCost = (int)(price * 0.05);
        protected bool active = false;
        public VehicleState State { get; protected set; } = VehicleState.Waiting;

        protected List<Road> currentPath = [];
        protected int currentPathIndex;
        public Road? CurrentRoad { get; protected set; }

        protected RoadEdge? currentEdge = null;
        protected Coordinate? currentCoordinate = null;

        protected List<Vector2> currentWaypoints = [];
        protected int currentWaypointIndex = 0;

        public float VisualX => VisualPosition.X;
        public float VisualY => VisualPosition.Y;

        private Vector2 _visualPosition;
        public Vector2 VisualPosition
        {
            get => _visualPosition;
            set
            {
                _visualPosition = value;
                OnPropertyChanged(nameof(VisualPosition));
                OnPropertyChanged(nameof(VisualX));
                OnPropertyChanged(nameof(VisualY));
            }
        }



        public void StartJourney(List<Road> path)
        {
            //if (route == null) return;
            if (path == null || path.Count == 0) return;

            currentPath = path;
            currentPathIndex = 0;
            CurrentRoad = currentPath[0];

            currentSpeed = maxSpeed;

            LoadWaypointsForField();

            if (currentWaypoints.Count > 0)
            {
                VisualPosition = currentWaypoints[0];
            }
            else {
                VisualPosition = new System.Numerics.Vector2(CurrentRoad.Coordinate.X * 32, CurrentRoad.Coordinate.Y * 32);
                System.Diagnostics.Debug.WriteLine($"FIGYELMEZTETÉS: Nincs Waypoint adat ehhez az úthoz! Busz lerakva a {VisualPosition} pixelre.");
            }

            State = VehicleState.Moving;
        }

        protected double waitTimer = 0;
        protected const double LOAD_TIME = 2.0;

        public void Update(double deltaTime)
        {
            if (State == VehicleState.Loading)
            {
                currentSpeed = 0;
                waitTimer += deltaTime;
                if (waitTimer >= LOAD_TIME)
                {
                    waitTimer = 0;
                    if (currentPathIndex + 1 < currentPath.Count)
                    {
                        State = VehicleState.Moving;
                        currentSpeed = maxSpeed;
                    }
                    else HandleRouteCycle();
                }
                return;
            }

            if (State != VehicleState.Moving || currentPath == null || currentWaypoints.Count == 0) return;

            currentSpeed = maxSpeed;
            float distanceToTravel = (currentSpeed / 3.6f) * (float)deltaTime;

            int safetyCounter = 0;
            while (distanceToTravel > 0 && safetyCounter < 10)
       
      {
                safetyCounter++;
                if (currentWaypointIndex >= currentWaypoints.Count)
                {
                    if (!TryTransitionToNextRoad())
                    {
                        distanceToTravel = 0;
                        continue;
                    }
                    if (currentWaypointIndex >= currentWaypoints.Count) { distanceToTravel = 0; continue; }
                }

                Vector2 target = currentWaypoints[currentWaypointIndex];
                Vector2 diff = target - VisualPosition;
                float dist = diff.Length();

                if (dist > 0.001f)
                {
                    VisualAngle = (float)(System.Math.Atan2(diff.Y, diff.X) * (180.0 / System.Math.PI));
                }

                if (dist < 0.01f)
                {
                    VisualPosition = target;
                    currentWaypointIndex++;
                }
                else if (distanceToTravel >= dist)
                {
                    VisualPosition = target;
                    distanceToTravel -= dist;
                    currentWaypointIndex++;
                }
                else
                {
                    VisualPosition += Vector2.Normalize(diff) * distanceToTravel;
                    distanceToTravel = 0;
                }
            }
        }

        private void HandleRouteCycle()
        {
            System.Diagnostics.Debug.WriteLine($"{Name} elérte a végállomást. Törlés a világból...");
            World.Instance.RemoveVehicle(this);
        }

        private bool TryTransitionToNextRoad()
        {
            if (currentPathIndex + 1 >= currentPath.Count)
            {
                State = VehicleState.Loading;
                if (CurrentRoad != null)
                    ReleaseJunctionLock(CurrentRoad);
                return false;
            }
            Road nextRoad = currentPath[currentPathIndex + 1];
            bool isNextJunction = IsFieldJunction(nextRoad);

            if (isNextJunction)
            {
                if (nextRoad.IsReserved)
                {
                    return false;
                }
                nextRoad.IsReserved = true;
            }

            if (CurrentRoad == null) throw new Exception();
            ReleaseJunctionLock(CurrentRoad);

            CurrentRoad = nextRoad;
            currentPathIndex++;

            LoadWaypointsForField();
            if (currentWaypoints.Count > 0)
            {
                VisualPosition = currentWaypoints[0];
                currentWaypointIndex = 1;
            }

            return true;
        }

        private void LoadWaypointsForField()
        {
            currentWaypoints.Clear();
            currentWaypointIndex = 0;

            Road? previousRoad = (currentPathIndex > 0) ? currentPath[currentPathIndex - 1] : null;
            Road? nextRoad = (currentPathIndex < currentPath.Count - 1) ? currentPath[currentPathIndex + 1] : null;

            if (CurrentRoad == null) throw new Exception();

            PathDirection entryDir = previousRoad == null ? PathDirection.Start : GetRelativeDirection(CurrentRoad.Coordinate, previousRoad.Coordinate);
            PathDirection exitDir = nextRoad == null ? PathDirection.End : GetRelativeDirection(CurrentRoad.Coordinate, nextRoad.Coordinate);

            var pathKey = (entryDir, exitDir);

            if (WaypointManager.Paths.TryGetValue(pathKey, out List<Vector2>? localPoints))
            {
                float startX = CurrentRoad.Coordinate.X * WaypointManager.TILE_SIZE;
                float startY = CurrentRoad.Coordinate.Y * WaypointManager.TILE_SIZE;

                Vector2 fieldOffset = new(startX, startY);

                foreach (var p in localPoints)
                {
                    currentWaypoints.Add(p + fieldOffset);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"HIÁNYZÓ WAYPOINT KULCS: {entryDir} -> {exitDir}");

                //Vészmegoldás

                float startX = CurrentRoad.Coordinate.X * WaypointManager.TILE_SIZE;
                float startY = CurrentRoad.Coordinate.Y * WaypointManager.TILE_SIZE;

                currentWaypoints.Add(new Vector2(startX + 16, startY + 16));
            }
        }

        private PathDirection GetRelativeDirection(Coordinate center, Coordinate target)
        {
            if (target.Y < center.Y) return PathDirection.North;
            if (target.Y > center.Y) return PathDirection.South;
            if (target.X > center.X) return PathDirection.East;
            if (target.X < center.X) return PathDirection.West;

            return PathDirection.Start;
        }

        private Vehicle? GetVehicleAhead()
        {
            //TODO// globális járműkezelő
            return null;
        }
        private void ReleaseJunctionLock(Road road)
        {
            if (IsFieldJunction(road))
            {
                road.IsReserved = false;
            }
        }

        private bool IsFieldJunction(Road road)
        {

            return road.RoadType.HasFlag(RoadType.JUNCTION);
        }

        public int Load(int amount)
        {
            int spaceLeft = capacity - CurrentLoad;
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
