using System.Diagnostics;
using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.TerrainGeneration.Generators;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using static VolcanicTransport.Model.World.Roadnetwork.Vehicle;

namespace VolcanicTransport.Model
{
    public class GameModel
    {
        public static World.World WorldInstance => World.World.Instance;

        #region Fields

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
        public event EventHandler<VehicleArrivedEventArgs>? VehicleArrivedAtStation;
        public bool IsPaused { get; private set; }
        public double Time { get; private set; } = 0;
        private readonly ISaveFileManager _savefileManager;

        public double PlayerMoney { get; private set; }

        public event EventHandler? MoneyChanged;
        public event EventHandler? GameOver;
        public event EventHandler? NewGame;
        public event EventHandler? StationBought;
        public event EventHandler? RoadBought;
        public event EventHandler? VehicleBought;
        public event EventHandler? VehicleSold;
        public event EventHandler? GameAdvanced;
        public event EventHandler? GamePaused;
        public event EventHandler? GameUnpaused;
        public event EventHandler? TimescaleChanged;
        public event EventHandler? FieldChanged;
        public event EventHandler? VehicleSelectedIndex;
        public event EventHandler? OnPlacementFailed;
        #endregion

        #region Instance

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

            PlayerMoney = GameSettings.StartingMoney;
        }

        public static GameModel Instance => _instance ?? throw new GameModelNotInitialisedException();

        public static void Initialise(int worldSize, int seed)
        {
            if (_instance != null) throw new InvalidOperationException("World already initialised");

            _instance = new GameModel(worldSize, seed);
        }
        #endregion

        #region SavingAndLoading
        public void LoadGame(string filename)
        => (_, IsPaused, Time, PlayerMoney) = _savefileManager.LoadGame(filename);

        public void SaveGame(string filename)
            => _savefileManager.SaveGame(new ISaveFileManager.GameData(this), filename);
        #endregion

        #region  Methods
        public void GenerateWorld() => WorldInstance.Generate();

        private static double GetMushroomCosts(Field field)
        {
            var cost = 0d;
            
            if (field.Surface is not Mushroom mushroom) return cost;
            
            var stage = (double)mushroom.GrowthStage+1;
            cost = stage * GameSettings.MushroomPricePerUnit;
            return cost;
        }
        
        public void Pause()
        {
            IsPaused = true;
            GamePaused?.Invoke(this, EventArgs.Empty);
        }

        public void UnPause()
        {
            IsPaused = false;
            GameUnpaused?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed1X()
        {
            UnPause();
            TimescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed2X()
        {
            UnPause();
            TimescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed4X()
        {
            UnPause();
            TimescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Update(double deltaTime)
        {
            if (IsPaused) return;
            WorldInstance.Update(deltaTime);
            GameAdvanced?.Invoke(this, EventArgs.Empty);
            Time += deltaTime;
        }

        public bool BuyVehicle(Vehicle v)
        {
            if (TryPurchase(v.Price))
            {
                v.ArrivedAtStation += HandleVehicleArrived;

                WorldInstance.AddVehicle(v);
                vehicleBought?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void SellVehicle(Vehicle v)
        {
            if (!WorldInstance.HasVehicle(v)) return;

            AddMoney(v.Price * 0.5);
            WorldInstance.RemoveVehicle(v);
            VehicleSold?.Invoke(this, EventArgs.Empty);
        }

        public bool TryPurchase(double amount)
        {
            if (!(PlayerMoney >= amount)) return false;

            PlayerMoney -= amount;
            MoneyChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private void AddMoney(double amount)
        {
            PlayerMoney += amount;
            MoneyChanged?.Invoke(this, EventArgs.Empty);
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
            if (PlayerMoney < 0)
                GameOver?.Invoke(this, EventArgs.Empty);
        }

        public bool IsBuildable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsBuildable() ?? false;
        public bool IsHeightenable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsHeightenable() ?? false;
        public bool IsLowerable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsLowerable() ?? false;

        public static void OnRoadBecameJunction(object? sender, Road.FieldEventArgs e)
        {
            Debug.WriteLine($"[ESEMÉNY] Új kereszteződés alakult ki itt: {e.Coordinate}");
            WorldInstance.Roadnetwork.RegisterNodeIfNeeded(e.Coordinate);
        }

        private Road? CanPlaceRoadHere(Coordinate coord, Field? field)
        {
            if (null == field)
                return null;

            if (!field.IsBuildable())
                return null;

            //creating a temporal to see if a road can be place here
            Road tempRoad = new(coord);

            tempRoad.RoadLayoutChanged += OnRoadBecameJunction;

            field.Surface = tempRoad;
            tempRoad.Update();


            if (tempRoad.RoadType != RoadType.INVALID) return tempRoad;

            field.Surface = null;
            tempRoad.RoadLayoutChanged -= OnRoadBecameJunction;
            OnPlacementFailed?.Invoke(this, EventArgs.Empty);
            return null;

        }


        public void PlaceRoad(Coordinate coord)
        {
            var roadPrice = GameSettings.BaseRoadPrice;
            
            var field = WorldInstance.GetField(coord);
            if (null == field)
                return;

            roadPrice += GetMushroomCosts(field);

            if (!TryPurchase(roadPrice))
                return;

            var road = CanPlaceRoadHere(coord, field);
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
                OnPlacementFailed?.Invoke(this, EventArgs.Empty);
                return;
            }
            RoadBought?.Invoke(this, EventArgs.Empty);
            WorldInstance.UpdateRoadNetworkAround(coord);
        }

        public bool PlaceStation(Coordinate coord)
        {
            var stationCost = GameSettings.BaseStationPrice;

            if (!IsBuildable(coord) || PlayerMoney < stationCost) return false;

            var nearRoad = Direction.Directions.Any(dir => WorldInstance.GetField(coord + dir)?.Surface is Road);

            if (!nearRoad) return false;

            var field = WorldInstance.GetField(coord);

            if (field == null) return false;

            stationCost += GetMushroomCosts(field);

            var city = WorldInstance.Cities.FirstOrDefault(c => c.CenterCoordinate.Distance(coord) <= 4);

            var factory = WorldInstance.Factories.FirstOrDefault(f => f.OriginCoordinate.Distance(coord) <= 4);

            Station? newStation = null;
            if (city != null) newStation = new CityStation(city, coord, "CityStation");
            if (factory != null) newStation = new FactoryStation(coord, "FactoryStation", factory);

            if (newStation == null || !TryPurchase(stationCost)) return false;

            WorldInstance.GetField(coord)!.Surface = newStation;
            WorldInstance.Stations.Add(newStation);

            WorldInstance.Roadnetwork.RegisterNodeIfNeeded(coord);

            var chunkCoord = WorldInstance.GetChunkCoordinate(coord);
            WorldInstance.GetChunk(chunkCoord)?.TriggerRerender();

            return true;
        }

        public static void AddStopToVehicle(Vehicle v, Station s)
        {

            if (v.Route.Stops.Count > 0 && v.Route.Stops.Last() == s) return;

            v.Route.AddStop(s);
            Debug.WriteLine($"Megálló hozzáadva: {s.Coordinate}. Összesen: {v.Route.Stops.Count}");

            var graph = WorldInstance.Roadnetwork;
            graph.RegisterNodeIfNeeded(s.Coordinate);
            graph.RebuildEdges();

            if (v.State == VehicleState.Waiting)
            {
                v.TryStartNextRoute();
            }
            v.TriggerRouteChanged();
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

        private void HandleVehicleArrived(object? sender, VehicleArrivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[GameModel Üzleti Logika] {e.Vehicle.Name} megérkezett a(z) {e.Station.Coordinate} állomásra!");

            double ticketIncome = 150;
            AddMoney(ticketIncome);
            System.Diagnostics.Debug.WriteLine($"[GameModel] Játékos kapott {ticketIncome}$-t a fuvarért.");

            // load-unload stb

            // tova a viewmodellnek ha kell
            VehicleArrivedAtStation?.Invoke(this, e);
        }

        #endregion

        private void TerraformField(Coordinate coord, int deltaHeight)
        {
            var terraformationPrice = GameSettings.BaseTerraformationPrice;

            var field = WorldInstance.GetField(coord);
            
            if (field == null)
                return;

            if (! ((deltaHeight == -1 && field.IsLowerable()) || (deltaHeight == 1 && field.IsHeightenable())))
                return;
            
            terraformationPrice += GetMushroomCosts(field);

            var newFieldType = (FieldType)((int)field.Type + deltaHeight);

            if (newFieldType > FieldType.HIGH_MOUNTAINS) return;
            if (!TryPurchase(terraformationPrice)) return;
            
            field.SetFieldTypeTo(newFieldType);
            var chunkCoord = WorldInstance.GetChunkCoordinate(coord);
            WorldInstance.GetChunk(chunkCoord)?.TriggerRerender();

        }
        public void HeightenField(Coordinate coord) => TerraformField(coord, +1);
        public void LowerField(Coordinate coord) => TerraformField(coord, -1);


    }
}
