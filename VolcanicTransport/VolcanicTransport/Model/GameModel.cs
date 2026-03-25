using System.Collections.ObjectModel;
using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.TerrainGeneration.Generators;
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
        public event EventHandler? onPlacementFailed;



        #region Instance
        public class GameModelNotInitialisedException : Exception { }

        private static GameModel? _instance;

        private GameModel(int worldSize, int seed)
        {
            World.World.Initialise(worldSize, seed);
            _savefileManager = new SaveFileManager();

            _instance = this;

            WorldInstance.GameWorldGenerator = new GameWorldGenerator(
                new TerrainHeightGenerator(),
                new MushroomGenerator(),
                new FactoryAndCityGenerator(5, 10)
                );
            WorldInstance.Generate();

            _playerMoney = 100000;
        }

        public static GameModel Instance => _instance ?? throw new GameModelNotInitialisedException();

        public static void Initialise(int worldSize, int seed)
        {
            if (_instance != null) throw new InvalidOperationException("World already initialised");

            _instance = new GameModel(worldSize, seed);
        }
        #endregion


        public void Pause()
        {
            _isPaused = true;
            gamePaused?.Invoke(this, EventArgs.Empty);
        }

        public void UnPause()
        {
            _isPaused = false;
            gameUnpaused?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed1X()
        {
            UnPause();
            timescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed2X()
        {
            UnPause();
            timescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed4X()
        {
            UnPause();
            timescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        /*
        public void Update()
        {
            if (_isPaused) return;
            WorldInstance.Update(1.0);
            gameAdvanced?.Invoke(this, EventArgs.Empty);
        }*/

        public void Update(double deltaTime)
        {
            if (_isPaused) return;
            WorldInstance.Update(deltaTime);
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

        public void OnRoadBecameJunction(object? sender, Road.FieldEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[ESEMÉNY] Új kereszteződés alakult ki itt: {e.Coordinate}");
            WorldInstance.Roadnetwork.RegisterNodeIfNeeded(e.Coordinate);
        }

        private Road? CanPlaceRoadHere(Coordinate coord, Field field)
        {
            if (null == field)
                return null;

            if (!field.IsBuildable())
                return null;

            //creating a temporal to see if a road can be place here
            Coordinate a = coord;
            Road tempRoad = new(coord);

            tempRoad.RoadLayoutChanged += OnRoadBecameJunction;

            field.Surface = tempRoad;
            tempRoad.Update();
            

            if (tempRoad.RoadType == RoadType.INVALID)
            {
                field.Surface = null;

                tempRoad.RoadLayoutChanged -= OnRoadBecameJunction;
                onPlacementFailed?.Invoke(this, EventArgs.Empty);
                return null;
            }

            return tempRoad;
        }


        public void PlaceRoad(Coordinate coord)
        {
            double roadPrice = 50;
            const double mushroomPricePerUnit = 20;
            double extraCost = 0;

            Field? field = WorldInstance.GetField(coord);
            if (null == field) 
                return;

            if (field.Surface is Mushroom mushroom)
            {
                double stage = (double)mushroom.GrowthStage+1;
                extraCost = stage * mushroomPricePerUnit;
                roadPrice += extraCost;
            }

            if (!TryPurchase(roadPrice)) 
                return;

            Road? road = CanPlaceRoadHere(coord, field);
            if (null == road)
            {
                AddMoney(roadPrice);
                return;
            }


            if (!road.TryUpdateNeighbours())
            {
                field.Surface = null;
                road.UpdateNeighbours();

                AddMoney(roadPrice);
                onPlacementFailed?.Invoke(this, EventArgs.Empty);
                return;
            }
            roadBought?.Invoke(this, EventArgs.Empty);
            WorldInstance.UpdateRoadNetworkAround(coord);
        }

        public bool PlaceStation(Coordinate coord)
        {
            double stationCost = 500;
            const double mushroomPricePerUnit = 20;
            double extraCost = 0;

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

            Field? field = WorldInstance.GetField(coord);

            if (field == null) return false;

            if (field.Surface is Mushroom mushroom)
            {
                double stage = (double)mushroom.GrowthStage+1;
                extraCost = stage * mushroomPricePerUnit;
                stationCost += extraCost;
            }

            var city = WorldInstance.Cities.FirstOrDefault(c => c.CenterCoordinate.Distance(coord) <= 4);

            var factory = WorldInstance.Factories.FirstOrDefault(f => f.OriginCoordinate.Distance(coord) <= 4);

            Station? newStation = null;
            if (city != null) newStation = new CityStation(city, coord, "CityStation");
            if (factory != null) newStation = new FactoryStation(coord, "FactoryStation", factory);

            if (newStation != null && TryPurchase(stationCost))
            {
                WorldInstance.GetField(coord).Surface = newStation;
                WorldInstance.Stations.Add(newStation);

                WorldInstance.Roadnetwork.RegisterNodeIfNeeded(coord);

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
            //graph.RegisterNodeIfNeeded(v.Route.Stops[v.Route.Stops.Count - 2].Field);
            graph.RegisterNodeIfNeeded(s.Coordinate);
            graph.RebuildEdges();

            //if (graph.NodeMap.TryGetValue(v.Route.Stops[v.Route.Stops.Count - 2].Field, out var startNode) &&
            //    graph.NodeMap.TryGetValue(s.Field, out var targetNode))
            //{
                //var path = Pathfinder.FindPath(startNode, targetNode);
               // if (path != null && path.Count > 0)
              //  {
                //    v.StartJourney(path);
               //     System.Diagnostics.Debug.WriteLine("Siker! Busz indul.");
              //  }
               // else
              //  {
              //      System.Diagnostics.Debug.WriteLine("Pathfinder: Nem található összeköttetés az utak között.");
               // }
           // }
            
        }

    }
}
