using System.Numerics;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Persistance;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
namespace VolcanicTransport.Model.World.Roadnetwork
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Bus), "bus")]
    [JsonDerivedType(typeof(MiniBus), "mini_bus")]
    [JsonDerivedType(typeof(TankerTruck), "tanker_truck")]
    [JsonDerivedType(typeof(CargoTruck), "cargo_truck")]
    public abstract class Vehicle(string name, GameSettings.VehicleData vehicleData) : IContainsReference
    {
        protected Vehicle(string name, GameSettings.VehicleData vehicleData, ProductType currentType,
                    int currentLoad, int currentStopIndex, VehicleState state, float posX, float posY,
                    float angle, PathDirection currentEntry, PathDirection currentExit,
                    double waitTimer, int currentPathIndex) : this(name, vehicleData)
        {
            CurrentType = currentType;
            CurrentLoad = currentLoad;
            CurrentStopIndex = currentStopIndex;
            State = state;
            PosX = posX; PosY = posY;
            Angle = angle;
            CurrentEntry = currentEntry;
            CurrentExit = currentExit;
            this.waitTimer = waitTimer;
            this.currentPathIndex = currentPathIndex;
        }

        [JsonInclude]
        public List<Coordinate>? SavedPathCoordinates { get; set; }

        [JsonInclude]
        public string? RouteName { get; set; }

        [JsonInclude] public double WaitTimer => waitTimer;
        [JsonInclude] public int CurrentPathIndex => currentPathIndex;

        [JsonInclude]
        public float PosX { 
            get => Position.X; 
            set => Position = new Vector2(value, Position.Y); 
        }
        [JsonInclude]
        public float PosY { 
            get => Position.Y; 
            set => Position = new Vector2(Position.X, value); 
        }


        [JsonIgnore]
        private readonly GameSettings.VehicleData vehicleData = vehicleData;

        [JsonIgnore]
        public List<ProductType> AllType => vehicleData.ProductTypes;

        [JsonIgnore]
        public int Price => vehicleData.Price;

        [JsonIgnore]
        public float MaxSpeed => vehicleData.MaxSpeed;

        [JsonIgnore]
        public int Capacity => vehicleData.Capacity;

        [JsonIgnore]
        protected int MaintenanceCost => (int)(Price * 0.05);


        public string Name { get; } = name;
        public ProductType CurrentType { get; protected set; } = ProductType.NONE;

        [JsonIgnore]
        public int CurrentLoad { get; protected set; } = 0;

        [JsonIgnore]
        public Route? Route { get; set; }
        public Route? PendingRoute { get; set; } = null;
        public float CurrentSpeed { get; protected set; } = 0;

        protected double waitTimer = 0;
        protected const double LOAD_TIME = 20.0;

        public int CurrentStopIndex { get; set; } = 1;

        protected bool active = false;
        public VehicleState State { get; set; } = VehicleState.Waiting;

        protected List<Road> currentPath = [];
        protected int currentPathIndex;
        [JsonIgnore]
        public Road? CurrentRoad { get; protected set; }

        protected RoadEdge? currentEdge = null;
        protected Coordinate? currentCoordinate = null;

        protected readonly List<Vector2> currentWaypoints = [];
        protected int currentWaypointIndex = 0;

        [JsonIgnore]
        public Vector2 Position { get; protected set; }
        public float Angle { get; set; }

        public PathDirection CurrentEntry { get; protected set; }
        public PathDirection CurrentExit { get; protected set; }

        public event EventHandler? StateUpdated;
        public event EventHandler? RouteChanged;
        public event EventHandler<VehicleArrivedEventArgs>? ArrivedAtStation;

        public void StartJourney(List<Road> path, bool alreadyOnRoad = true, Station? currentStation = null)
        {
            //if (route == null) return;
            if (path == null || path.Count == 0) return;

            if (CurrentRoad != null)
            {
                World.Instance.VehicleManager.UnregisterVehicleFromField(this, CurrentRoad.Coordinate);

                if (CurrentRoad is Station oldStation) oldStation.IsOccupied = false;
            }

            currentPath = path;
            currentPathIndex = 0;
            CurrentRoad = currentPath[0];

            CurrentEntry = PathDirection.Start;
            CurrentExit = currentPath.Count > 1 ? GetRelativeDirection(CurrentRoad.Coordinate, currentPath[1].Coordinate) : PathDirection.End;
            World.Instance.VehicleManager.RegisterVehicleOnField(this, CurrentRoad.Coordinate);

            if (CurrentRoad is Station startStation)
            {
                startStation.IsOccupied = true;
            }

            CurrentSpeed = MaxSpeed;
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
                CurrentSpeed = 0;
                StateUpdated?.Invoke(this, EventArgs.Empty);
                waitTimer += deltaTime;
                if (waitTimer >= LOAD_TIME)
                {
                    waitTimer = 0;
                    if (currentPathIndex + 1 < currentPath.Count)
                    {
                        State = VehicleState.Moving;
                        CurrentSpeed = MaxSpeed;
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

            CurrentSpeed = MaxSpeed;

            if (CurrentRoad is Bridge bridge)
            {
                CurrentSpeed = Math.Min(MaxSpeed, bridge.SpeedLimit);
            }

            else if (currentPathIndex + 1 < currentPath.Count && null != CurrentRoad)
            {
                Road nextRoad = currentPath[currentPathIndex + 1];
                var currentField = World.Instance.GetField(CurrentRoad.Coordinate);
                var nextField = World.Instance.GetField(nextRoad.Coordinate);

                if (currentField != null && nextField != null)
                {

                    if (nextField.GetHeightDifference(currentField) == -1)
                    {
                        CurrentSpeed *= 0.4f;
                    }
                    
                }
            }

            Vehicle? ahead = GetVehicleAhead();
            if (ahead != null)
            {
                float dist = Vector2.Distance(Position, ahead.Position);
                float safeFollowDistance = GameSettings.FieldSize * 1.0f;

                if (dist <= safeFollowDistance)
                {
                    CurrentSpeed = Math.Min(CurrentSpeed, ahead.CurrentSpeed);
                }
            }

            StateUpdated?.Invoke(this, EventArgs.Empty);
            float distanceToTravel = (CurrentSpeed / 3.6f) * (float)deltaTime;

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

            if (CurrentRoad != null && CurrentRoad.Coordinate == targetStation.Coordinate)
            {
                System.Diagnostics.Debug.WriteLine($"{Name} már a célállomáson van, ugrás a következőre!");
                CurrentStopIndex = (CurrentStopIndex + 1) % Route.Stops.Count;
                targetStation = Route.Stops[CurrentStopIndex];
            }

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
                    if (newPath.Last().Coordinate != targetStation.Coordinate)
                    {
                        if (World.Instance.GetField(targetStation.Coordinate)?.Surface is Road destRoad)
                        {
                            newPath.Add(destRoad);
                        }
                    }

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


                if (Route != null && Route.Stops.Count > 0)
                {
                    Station arrivedStation = Route.Stops[CurrentStopIndex];
                    ArrivedAtStation?.Invoke(this, new VehicleArrivedEventArgs(this, arrivedStation));
                }
                return false;
            }

            Road nextRoad = currentPath[currentPathIndex + 1];
            PathDirection nextEntry = GetRelativeDirection(nextRoad.Coordinate, CurrentRoad!.Coordinate);
            Road? roadAfterNext = (currentPathIndex + 2 < currentPath.Count) ? currentPath[currentPathIndex + 2] : null;
            PathDirection nextExit = roadAfterNext == null ? PathDirection.End : GetRelativeDirection(nextRoad.Coordinate, roadAfterNext.Coordinate);

            if (nextRoad is Station nextStation && nextStation.IsOccupied) return false;

            var vehiclesOnNext = World.Instance.VehicleManager.GetVehiclesOnField(nextRoad.Coordinate);
            foreach (var other in vehiclesOnNext)
            {
                if (other.CurrentEntry == nextEntry) return false;

                if (IsFieldJunction(nextRoad))
                {
                    if (PathsCross(nextEntry, nextExit, other.CurrentEntry, other.CurrentExit)) return false;
                }
                else
                {
                    if (vehiclesOnNext.Count >= 2) return false;
                }
            }

            if (CurrentRoad is Station oldStation) oldStation.IsOccupied = false;
            World.Instance.VehicleManager.UnregisterVehicleFromField(this, CurrentRoad.Coordinate);

            CurrentRoad = nextRoad;
            currentPathIndex++;
            CurrentEntry = nextEntry;
            CurrentExit = nextExit;

            World.Instance.VehicleManager.RegisterVehicleOnField(this, CurrentRoad.Coordinate);

            if (CurrentRoad is Station enteredStation && currentPathIndex == currentPath.Count - 1)
            {
                enteredStation.IsOccupied = true;
            }

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
            if (currentPathIndex + 1 < currentPath.Count)
            {
                Road nextRoad = currentPath[currentPathIndex + 1];
                PathDirection ourNextEntry = GetRelativeDirection(nextRoad.Coordinate, CurrentRoad!.Coordinate);

                var vehiclesNext = World.Instance.VehicleManager.GetVehiclesOnField(nextRoad.Coordinate);
                foreach (var other in vehiclesNext)
                {
                    if (other.CurrentEntry == ourNextEntry)
                    {
                        return other;
                    }
                }
            }
            return null;
        }

        private int GetDirectionIndex(PathDirection dir)
        {
            return dir switch
            {
                PathDirection.North => 0,
                PathDirection.East => 1,
                PathDirection.South => 2,
                PathDirection.West => 3,
                _ => -1
            };
        }

        private bool PathsCross(PathDirection entry1, PathDirection exit1, PathDirection entry2, PathDirection exit2)
        {
            if (exit1 == exit2 && exit1 != PathDirection.End && exit1 != PathDirection.Start) return true;
            if (entry1 == entry2) return true;

            int a1 = GetDirectionIndex(entry1);
            int b1 = GetDirectionIndex(exit1);
            int a2 = GetDirectionIndex(entry2);
            int b2 = GetDirectionIndex(exit2);

            if (a1 == -1 || b1 == -1 || a2 == -1 || b2 == -1) return false;
            if (a1 == b2 || b1 == a2) return false;

            int p1 = Math.Min(a1, b1);
            int p2 = Math.Max(a1, b1);
            int q1 = Math.Min(a2, b2);
            int q2 = Math.Max(a2, b2);

            return p1 < q1 && q1 < p2 && p2 < q2;
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
            int spaceLeft = Capacity - CurrentLoad;
            int taken = Math.Min(amount, spaceLeft);
            CurrentLoad += taken;
            if (CurrentType == ProductType.NONE)
            {
                CurrentType = type;
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
                CurrentType = ProductType.NONE;
                StateUpdated?.Invoke(this, EventArgs.Empty);
            }
            return provided;
        }

        public void ClearRoute()
        {
            if (CurrentRoad != null)
                World.Instance.VehicleManager.UnregisterVehicleFromField(this, CurrentRoad.Coordinate);

            State = VehicleState.Waiting;
            CurrentSpeed = 0;
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

        public void RestoreReference(Coordinate coordinate)
        {
            var world = World.Instance;

            if (!string.IsNullOrEmpty(RouteName))
            {
                Route = world.SavedRoutes.FirstOrDefault(r => r.Name == RouteName);
            }

            if (SavedPathCoordinates != null)
            {
                currentPath.Clear();
                foreach (var coord in SavedPathCoordinates)
                {
                    if (world.GetField(coord)?.Surface is Road road)
                    {
                        currentPath.Add(road);
                    }
                }
            }

            if (currentPath.Count > 0 && currentPathIndex < currentPath.Count)
            {
                CurrentRoad = currentPath[currentPathIndex];
            }

            if (CurrentRoad != null)
            {
                world.VehicleManager.RegisterVehicleOnField(this, CurrentRoad.Coordinate);
            }



            LoadWaypointsForField();
        }

        public void PrepareForSave()
        {
            SavedPathCoordinates = [.. currentPath.Select(r => r.Coordinate)];
            RouteName = Route?.Name;
        }
    }
}
