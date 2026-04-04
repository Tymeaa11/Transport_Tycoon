using System.Diagnostics;
using System.Threading.Channels;
using VolcanicTransport.Model.Exceptions;
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
        public static World.World WorldInstance => World.World.Instance;
        private readonly ScalableTimer _mushroomGrowthTimer;

        #region Fields

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

            PlayerMoney = 100000;

            _mushroomGrowthTimer = new ScalableTimer();
            _mushroomGrowthTimer.TimeScale = 1; 
            _mushroomGrowthTimer.Elapsed += (s, e) => {
                UpdateMushroomsOnTimer();
            };
            _mushroomGrowthTimer.Start();
        }

        public static GameModel Instance => _instance ?? throw new GameModelNotInitialisedException();

        public static void Initialise(int worldSize, int seed)
        {
            if (_instance != null) throw new InvalidOperationException("World already initialised");

            _instance = new GameModel(worldSize, seed);
        }
        #endregion

        #region  Methods
        public void Pause()
        {
            IsPaused = true;
            _mushroomGrowthTimer.TimeScale = 0;
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
            _mushroomGrowthTimer.TimeScale = 1;
            TimescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed2X()
        {
            UnPause();
            _mushroomGrowthTimer.TimeScale = 2;
            TimescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed4X()
        {
            UnPause();
            _mushroomGrowthTimer.TimeScale = 4;
            TimescaleChanged?.Invoke(this, EventArgs.Empty);
        }
        private void UpdateMushroomsOnTimer()
        {
            UpdateAllMushrooms(0.1);
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
            if (!TryPurchase(v.Price)) return false;
            
            WorldInstance.AddVehicle(v);
            VehicleBought?.Invoke(this, EventArgs.Empty);
            return true;
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
            double roadPrice = 50;
            const double mushroomPricePerUnit = 20;

            var field = WorldInstance.GetField(coord);
            if (null == field) 
                return;

            if (field.Surface is Mushroom mushroom)
            {
                var stage = (double)mushroom.GrowthStage+1;
                var extraCost = stage * mushroomPricePerUnit;
                roadPrice += extraCost;
            }

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
            double stationCost = 500;
            const double mushroomPricePerUnit = 20;

            if (!IsBuildable(coord) || PlayerMoney < stationCost) return false;

            var nearRoad = Direction.Directions.Any(dir => WorldInstance.GetField(coord + dir)?.Surface is Road);
            
            if (!nearRoad) return false;

            var field = WorldInstance.GetField(coord);

            if (field == null) return false;

            if (field.Surface is Mushroom mushroom)
            {
                var stage = (double)mushroom.GrowthStage+1;
                var extraCost = stage * mushroomPricePerUnit;
                stationCost += extraCost;
            }

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
            v.Route ??= new Route();

            v.Route.AddStop(s);
            Debug.WriteLine($"Megálló hozzáadva: {s.Coordinate}. Összesen: {v.Route.Stops.Count}");

            if (v.Route.Stops.Count < 2)
            {
                Debug.WriteLine("Várakozás a második megállóra...");
                return;
            }
            Debug.WriteLine("Két megálló megvan, gráf frissítése...");
            
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

        private void UpdateAllMushrooms(double deltaTime)
        {
            int samplesCount = (int)(100 * deltaTime);
            HashSet<Chunk> chunksToRedraw = new HashSet<Chunk>();

            for (int i = 0; i < samplesCount; i++)
            {
                int x = WorldInstance.SharedRandom.Next(0, WorldInstance.SizeInFields.X);
                int y = WorldInstance.SharedRandom.Next(0, WorldInstance.SizeInFields.Y);
                Coordinate randomCoord = new Coordinate(x, y);

                Field? field = WorldInstance.GetField(randomCoord);

                if (field?.Surface is Mushroom mushroom)
                {
                    (Coordinate? target, bool spread) = mushroom.UpdateMushroom(randomCoord);
                    if (spread)
                    {
                        if (target != null)
                        {
                            var chunk = WorldInstance.GetChunk(WorldInstance.GetChunkCoordinate((Coordinate)target));
                            if (chunk != null) chunksToRedraw.Add(chunk);
                        }
                    }
                }
            }

            foreach (Chunk? chunk in chunksToRedraw)
            {
                chunk.TriggerRerender();
            }
        }

        #endregion

        private void TerraformField (Coordinate coord, int deltaHeight)
        {
            double terraformingPrice = 50;
            const double mushroomPricePerUnit = 20;


            Field? field = WorldInstance.GetField(coord);
            if (null == field)
                return;

            if (! ((deltaHeight == -1 && field.IsLowerable()) || (deltaHeight == 1 && field.IsHeightenable())))
            {
                return;
            }

            if (field.Surface is Mushroom mushroom)
            {
                double stage = (double)mushroom.GrowthStage + 1;
                terraformingPrice += stage * mushroomPricePerUnit;
            }

            FieldType newFieldType = (FieldType)((int)field.Type + deltaHeight);

            if (FieldType.DEEP_LAVA_OCEAN <= newFieldType && newFieldType <= FieldType.HIGH_MOUNTAINS)
            {
                if (TryPurchase(terraformingPrice))
                {
                    field.SetFieldTypeTo(newFieldType);
                    var chunkCoord = WorldInstance.GetChunkCoordinate(coord);
                    WorldInstance.GetChunk(chunkCoord)?.TriggerRerender();
                }
            }

        }
        public void HeightenField(Coordinate coord) => TerraformField(coord, +1);
        public void LowerField(Coordinate coord) => TerraformField(coord, -1);

    }
}
