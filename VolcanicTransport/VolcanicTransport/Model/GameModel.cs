using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model
{
    public class GameModel
    {
        private double _playerMoney;
        private bool _isPaused = false;
        private readonly DateTime _currentTime;
        private readonly ISaveFileManager _savefileManager;
        private readonly ScalableTimer _gameTickTimer;

        public double PlayerMoney {  get { return _playerMoney; } }
        public World.World WorldInstance { get => World.World.Instance; }

        public event EventHandler? moneyChanged;
        public event EventHandler? gameOver;
        public event EventHandler? newGame;
        public event EventHandler? stationBought;
        public event EventHandler? roadBought;
        public event EventHandler? vehicleBought;
        public event EventHandler? vehicleSelled;
        public event EventHandler? gameAdvanced;
        public event EventHandler? gamePaused;
        public event EventHandler? gameUnpaused;
        public event EventHandler? timescaleChanged;
        public event EventHandler? fieldChanged;
        public event EventHandler? vehicleSelectedIndex;



        #region Instance
        public class GameModelNotInitialisedException : Exception { }

        private static GameModel? _instance;

        private GameModel(int worldSize)
        {
            World.World.Initialise(worldSize);
            _savefileManager = new SaveFileManager();

            WorldInstance.GameWorldGenerator = new GameWorldGenerator(
                new TerrainHeightGenerator(),
                new MushroomGenerator(),
                new FactoryAndCityGenerator(5, 10)
                );
            WorldInstance.Generate();

            _gameTickTimer = new ScalableTimer();
            _gameTickTimer.TimeScale = 1;
            _gameTickTimer.Elapsed += (s, e) => OnTimerTick();
            _gameTickTimer.Start();
            _playerMoney = 10000;
        }

        private void OnTimerTick()
        {
            if (_isPaused) return;
            Update();
        }

        public static GameModel Instance => _instance ?? throw new GameModelNotInitialisedException();

        public static void Initialise(int worldSize)
        {
            if (_instance != null) throw new InvalidOperationException("World already initialised");

            _instance = new GameModel(worldSize);
        }
        #endregion


        public void Pause()
        {
            _isPaused = true;
            _gameTickTimer.Stop();
            gamePaused?.Invoke(this, EventArgs.Empty);
        }

        public void UnPause()
        {
            _isPaused = false;
            _gameTickTimer.Start();
            gameUnpaused?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed1X() { _gameTickTimer.TimeScale = 1; timescaleChanged?.Invoke(this, EventArgs.Empty); }
        public void ChangeTimeSpeed2X() { _gameTickTimer.TimeScale = 2; timescaleChanged?.Invoke(this, EventArgs.Empty); }
        public void ChangeTimeSpeed4X() { _gameTickTimer.TimeScale = 4; timescaleChanged?.Invoke(this, EventArgs.Empty); }

        public void Update()
        {
            WorldInstance.Update(1.0);
            gameAdvanced?.Invoke(this, EventArgs.Empty);
        }

        public bool BuyVehicle(Vehicle v)
        {
            if (TryPurchase(v.Price))
            {
                WorldInstance.AddVehicle(v);
                vehicleBought?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void SellVehicle(Vehicle v)
        {
            if (WorldInstance.HasVehicle(v))
            {
                AddMoney(v.Price * 0.5);
                WorldInstance.RemoveVehicle(v);
                vehicleSelled?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TryPurchase(double amount)
        {
            if (_playerMoney >= amount)
            {
                _playerMoney -= amount;
                moneyChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void AddMoney(double amount)
        {
            _playerMoney += amount;
            moneyChanged?.Invoke(this, EventArgs.Empty);
        }

        //public void SaveGame() => savefileManager?.Save(this);
        //public void LoadGame() => savefileManager?.Load(this);

        private void HandleMonthlyExpenses()
        {
            // Levonja a fenntartási költségeket
            CheckBankruptcy();
        }

        private void CheckBankruptcy()
        {
            if (_playerMoney < 0)
            {
                gameOver?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsBuildable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsBuildable() ?? false;

        public bool PlaceRoad(Coordinate coord)
        {
            const double roadPrice = 50;

            if (IsBuildable(coord) && TryPurchase(roadPrice))
            {
                bool success = WorldInstance.PlaceRoad(coord);
                if (success)
                {
                    roadBought?.Invoke(this, EventArgs.Empty);
                    WorldInstance.UpdateRoadNetworkAround(coord);
                    Field? f = WorldInstance.GetField(coord);
                    if (f != null) 
                    {
                        WorldInstance.Roadnetwork.RegisterNodeIfNeeded(f);
                        WorldInstance.Roadnetwork.RebuildEdges();
                    }
                    return true;
                }
                else
                {
                    AddMoney(roadPrice);
                }
            }
            return false;
        }
        public bool PlaceStation(Coordinate coord)
        {
            const int stationCost = 500;

            if (!IsBuildable(coord) || _playerMoney < stationCost) return false;

            bool nearRoad = false;
            Coordinate[] directions = { Direction.North, Direction.South, Direction.East, Direction.West };

            foreach (var dir in directions)
            {
                if (WorldInstance.GetField(coord + dir)?.Surface is Road)
                {
                    nearRoad = true;
                    break;
                }
            }
            if (!nearRoad) return false;

            var city = WorldInstance.Cities.FirstOrDefault(c => c.CenterCoordinate.Distance(coord) <= 4);

            var factory = WorldInstance.Factories.FirstOrDefault(f => f.OriginCoordinate.Distance(coord) <= 3);

            Station? newStation = null;
            if (city != null) newStation = new CityStation(city, coord, "CityStation");
            if (factory != null) newStation = new FactoryStation(coord, "FactoryStation", factory);

            if (newStation != null && TryPurchase(stationCost))
            {
                WorldInstance.GetField(coord).Surface = newStation;
                WorldInstance.Stations.Add(newStation);

                var field = WorldInstance.GetField(coord);
                WorldInstance.Roadnetwork.RegisterNodeIfNeeded(field);
                WorldInstance.Roadnetwork.RebuildEdges();

                var chunkCoord = WorldInstance.GetChunkCoordinate(coord);
                WorldInstance.GetChunk(chunkCoord)?.TriggerRerender();

                return true;
            }

            return false;
        }

        public void AddStopToVehicle(Vehicle v, Station s)
        {

            if (v.Route == null) v.Route = new Route();

            v.Route.AddStop(s);
            System.Diagnostics.Debug.WriteLine($"Megálló hozzáadva: {s.Coordinate}. Összesen: {v.Route.Stops.Count}");

            if (v.Route.Stops.Count < 2)
            {
                System.Diagnostics.Debug.WriteLine("Várakozás a második megállóra...");
                return;
            }
            System.Diagnostics.Debug.WriteLine("Két megálló megvan, gráf frissítése...");

            var graph = WorldInstance.Roadnetwork;
            graph.RegisterNodeIfNeeded(v.Route.Stops[v.Route.Stops.Count - 2].Field);
            graph.RegisterNodeIfNeeded(s.Field);
            graph.RebuildEdges();

            if (graph.NodeMap.TryGetValue(v.Route.Stops[v.Route.Stops.Count - 2].Field, out var startNode) &&
                graph.NodeMap.TryGetValue(s.Field, out var targetNode))
            {
                var path = Pathfinder.FindPath(startNode, targetNode);
                if (path != null && path.Count > 0)
                {
                    v.StartJourney(path);
                    System.Diagnostics.Debug.WriteLine("Siker! Busz indul.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Pathfinder: Nem található összeköttetés az utak között.");
                }
            }
            
        }

    }
}
