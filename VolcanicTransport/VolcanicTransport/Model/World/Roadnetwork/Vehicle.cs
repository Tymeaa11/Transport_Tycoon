using System.Numerics;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
namespace VolcanicTransport.Model.World.Roadnetwork
{
    public abstract class Vehicle(string name, float maxSpeed, int capacity, int price, List<ProductType> allType)
    {

        public Vehicle(string name, GameSettings.VehicleData vehicleData)
        : this(name, vehicleData.MaxSpeed, vehicleData.Capacity, vehicleData.Price, vehicleData.ProductTypes) { }

        public string Name { get; } = name;
        public List<ProductType> AllType { get { return allType; } }

        protected ProductType currentType = ProductType.NONE;
        public ProductType CurrentType
        {
            get { return currentType; }
        }
        public int CurrentLoad { get; protected set; } = 0;
        public int Price { get; } = price;
        protected Route? route = null;

        public Route? Route { get { return route; } set { route = value; } }
        public Route? PendingRoute { get; set; } = null;

        protected float currentSpeed = 0;
        public float CurrentSpeed => currentSpeed;

        protected float maxSpeed = maxSpeed;
        protected int capacity = capacity;

        protected double waitTimer = 0;
        protected const double LOAD_TIME = 20.0;

        public int CurrentStopIndex { get; set; } = 1;

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

        protected readonly List<Vector2> currentWaypoints = [];
        protected int currentWaypointIndex = 0;

        public Vector2 Position { get; protected set; }
        public float Angle { get; protected set; }

        public event EventHandler? StateUpdated;
        public event EventHandler? RouteChanged;
        public event EventHandler<VehicleArrivedEventArgs>? ArrivedAtStation;

        public void StartJourney(List<Road> path, bool alreadyOnRoad = true, Station? currentStation = null)
        {
            //if (route == null) return;
            if (path == null || path.Count == 0) return;

            currentPath = path;
            currentPathIndex = 0;
            CurrentRoad = currentPath[0];

            currentSpeed = maxSpeed;
            StateUpdated?.Invoke(this, EventArgs.Empty);

            LoadWaypointsForField();

            if (alreadyOnRoad)
            {

                if (currentWaypoints.Count > 0)
                {
                    Position = currentWaypoints[0];
                }
                else
                {
                    Position = new Vector2(CurrentRoad.Coordinate.X * GameSettings.FieldSize, CurrentRoad.Coordinate.Y * GameSettings.FieldSize);
                    System.Diagnostics.Debug.WriteLine($"FIGYELMEZTETÉS: Nincs Waypoint adat ehhez az úthoz! Busz lerakva a {Position} pixelre.");
                }
            }
            else if (currentStation != null)
            {

                float stationCenterX = (currentStation.Coordinate.X * GameSettings.FieldSize) + GameSettings.FieldSizeP2;
                float stationCenterY = (currentStation.Coordinate.Y * GameSettings.FieldSize) + GameSettings.FieldSizeP2;

                currentWaypoints.Insert(0, new System.Numerics.Vector2(stationCenterX, stationCenterY));
                Position = new System.Numerics.Vector2(stationCenterX, stationCenterY);

            }

            State = VehicleState.Moving;
            StateUpdated?.Invoke(this, EventArgs.Empty);
        }


        public void Update(double deltaTime)
        {
            if (State == VehicleState.Loading)
            {
                currentSpeed = 0;
                StateUpdated?.Invoke(this, EventArgs.Empty);
                waitTimer += deltaTime;
                if (waitTimer >= LOAD_TIME)
                {
                    waitTimer = 0;
                    if (currentPathIndex + 1 < currentPath.Count)
                    {
                        State = VehicleState.Moving;
                        currentSpeed = maxSpeed;
                        StateUpdated?.Invoke(this, EventArgs.Empty);
                    }
                    else HandleRouteCycle();
                }
                return;
            }

            if (State != VehicleState.Moving || currentPath == null || currentWaypoints.Count == 0)
            {
                State = VehicleState.Waiting;
                StateUpdated?.Invoke(this, EventArgs.Empty);
                return;
            }

            currentSpeed = maxSpeed;
            StateUpdated?.Invoke(this, EventArgs.Empty);
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
                Vector2 diff = target - Position;
                float dist = diff.Length();

                if (dist > 0.001f)
                {
                    Angle = (float)(System.Math.Atan2(diff.Y, diff.X) * (180.0 / System.Math.PI));
                }

                if (dist < 0.01f)
                {
                    Position = target;
                    currentWaypointIndex++;
                }
                else if (distanceToTravel >= dist)
                {
                    Position = target;
                    distanceToTravel -= dist;
                    currentWaypointIndex++;
                }
                else
                {
                    Position += Vector2.Normalize(diff) * distanceToTravel;
                    distanceToTravel = 0;
                }
            }
            StateUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void HandleRouteCycle()
        {
            if (PendingRoute != null && PendingRoute.Stops.Count >= 2)
            {
                Route = PendingRoute;
                PendingRoute = null;
                CurrentStopIndex = 0;
                System.Diagnostics.Debug.WriteLine($"{Name} elolvasta az új menetrendet! Váltás a {Route.Stops[0].Coordinate} állomásra.");
                State = VehicleState.Moving;
            }
            else
            {
                if (Route == null || Route.Stops.Count < 2)
                {
                    State = VehicleState.Waiting;
                    StateUpdated?.Invoke(this, EventArgs.Empty);
                    System.Diagnostics.Debug.WriteLine($"{Name} várakozik további megállókra...");
                    return;
                }
                CurrentStopIndex = (CurrentStopIndex + 1) % Route.Stops.Count;
            }

            Station targetStation = Route.Stops[CurrentStopIndex];
            System.Diagnostics.Debug.WriteLine($"{Name} tervezés a következő pontra: -> {targetStation.Coordinate}");

            var graph = World.Instance.Roadnetwork;

            RoadNode? targetNode = graph.NodeMap.Values.FirstOrDefault(n => n.Coordinate == targetStation.Coordinate);

            RoadNode? startNode = null;
            if (CurrentRoad != null)
            {
                startNode = graph.NodeMap.Values
                    .OrderBy(n => Math.Abs(n.Coordinate.X - CurrentRoad.Coordinate.X) + Math.Abs(n.Coordinate.Y - CurrentRoad.Coordinate.Y))
                    .FirstOrDefault();
            }



            if (startNode != null && targetNode != null)
            {
                List<Road>? newPath = Pathfinder.FindPath(startNode, targetNode);

                if (newPath != null && newPath.Count > 0)
                {
                    Station? currentStation = World.Instance.GetField(CurrentRoad!.Coordinate)?.Surface as Station;
                    StartJourney(newPath, false, currentStation);
                }
                else
                {
                    State = VehicleState.Waiting;
                    System.Diagnostics.Debug.WriteLine($"[HIBA] Nincs aszfaltos út a következő megállóig!");
                }
            }
            else
            {
                State = VehicleState.Waiting;
                System.Diagnostics.Debug.WriteLine($"[HIBA] A gráf nem találja a megállót vagy a busz jelenlegi helyét!");
            }
            StateUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void TryStartNextRoute()
        {
            if (State == VehicleState.Waiting && Route != null && Route.Stops.Count >= 2)
            {
                HandleRouteCycle();
            }
        }

        private bool TryTransitionToNextRoad()
        {
            if (currentPathIndex + 1 >= currentPath.Count)
            {
                State = VehicleState.Loading;
                StateUpdated?.Invoke(this, EventArgs.Empty);
                waitTimer = 0;
                if (CurrentRoad != null)
                    ReleaseJunctionLock(CurrentRoad);

                if (Route != null && Route.Stops.Count > 0)
                {
                    Station arrivedStation = Route.Stops[CurrentStopIndex];
                    ArrivedAtStation?.Invoke(this, new VehicleArrivedEventArgs(this, arrivedStation));
                }
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
                Position = currentWaypoints[0];
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

            float startX = CurrentRoad.Coordinate.X * GameSettings.FieldSize;
            float startY = CurrentRoad.Coordinate.Y * GameSettings.FieldSize;

            if (WaypointManager.Paths.TryGetValue(pathKey, out List<Vector2>? localPoints))
            {
                Vector2 fieldOffset = new(startX, startY);

                foreach (var p in localPoints)
                {
                    currentWaypoints.Add(p + fieldOffset);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"HIÁNYZÓ WAYPOINT KULCS: {entryDir} -> {exitDir}");

                currentWaypoints.Add(new Vector2(startX + GameSettings.FieldSizeP2, startY + GameSettings.FieldSizeP2));
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

        public int Load(int amount, ProductType type)
        {
            int spaceLeft = capacity - CurrentLoad;
            int taken = Math.Min(amount, spaceLeft);
            CurrentLoad += taken;
            if (currentType == ProductType.NONE)
            {
                currentType = type;
                StateUpdated?.Invoke(this, EventArgs.Empty);
            }
            return taken;
        }

        public int Unload(int amountNeeded)
        {
            int provided = Math.Min(CurrentLoad, amountNeeded);
            CurrentLoad -= provided;
            if (CurrentLoad == 0)
            {
                currentType = ProductType.NONE;
                StateUpdated?.Invoke(this, EventArgs.Empty);
            }
            return provided;
        }

        public void ClearRoute()
        {
            State = VehicleState.Waiting;
            currentSpeed = 0;
            currentPath.Clear();
            currentWaypoints.Clear();
            Route = null;
            CurrentStopIndex = 1;
            StateUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void AssignNewRoute(Route newRoute)
        {
            if (State == VehicleState.Waiting)
            {
                Route = newRoute;
                CurrentStopIndex = -1;
                HandleRouteCycle();
            }
            else
            {
                PendingRoute = newRoute;
            }
            RouteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void TriggerRouteChanged()
        {
            RouteChanged?.Invoke(this, EventArgs.Empty);
        }

        public class VehicleArrivedEventArgs(Vehicle vehicle, Station station) : EventArgs
        {
            public Vehicle Vehicle { get; } = vehicle;
            public Station Station { get; } = station;
        }
    }
}
