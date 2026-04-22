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
    public class GameModel : IDisposable
    {
        public static World.World WorldInstance => World.World.Instance;
        private readonly ScalableTimer _mushroomGrowthTimer;
        private double _monthlyExpenseAccumulator = 0;

        #region Fields

        public System.Collections.ObjectModel.ObservableCollection<Route> SavedRoutes { get; } = [];
        public event EventHandler<VehicleArrivedEventArgs>? VehicleArrivedAtStation;
        public bool IsPaused { get; private set; }
        public double Time { get; private set; } = 0;
        private readonly ISaveFileManager _savefileManager;

        public double PlayerMoney { get; private set; }

        public event EventHandler? MoneyChanged;
        public event EventHandler? GameOver;
        //public event EventHandler? NewGame;
        //public event EventHandler? StationBought;
        public event EventHandler? RoadBought;
        public event EventHandler? VehicleBought;
        public event EventHandler? VehicleSold;
        public event EventHandler? GameAdvanced;
        public event EventHandler? GamePaused;
        public event EventHandler? GameUnpaused;
        public event EventHandler? TimescaleChanged;
        //public event EventHandler? FieldChanged;
        //public event EventHandler? VehicleSelectedIndex;
        public event EventHandler? OnPlacementFailed;

        #endregion

        #region Instance

        private static GameModel? _instance;

        private GameModel()
        {
            _savefileManager = new SaveFileManager();

            _instance = this;

            PlayerMoney = GameSettings.StartingMoney;

            _mushroomGrowthTimer = new ScalableTimer { TimeScale = 1 };
            _mushroomGrowthTimer.Elapsed += (s, e) =>
            {
                UpdateMushroomsOnTimer();
            };
        }

        private GameModel(int worldSize, int seed) : this()
        {
            World.World.Initialise(worldSize, seed);

            WorldInstance.GameWorldGenerator = new GameWorldGenerator(
                new TerrainHeightGenerator(),
                new MushroomGenerator(),
                new FactoryAndCityGenerator(GameSettings.CityCount, GameSettings.FactoryCount)
            );

            WorldInstance.Generate();
            _mushroomGrowthTimer.Start();
        }

        private GameModel(string fileName) : this()
        {
            LoadGame(fileName);
            _mushroomGrowthTimer.Start();
        }

        ~GameModel() { Dispose(); }

        public static GameModel Instance => _instance ?? throw new GameModelNotInitialisedException();

        public static void InitialiseNewGame(int worldSize, int seed)
        {
            _instance = new GameModel(worldSize, seed);
        }

        public static void InitialiseLoadedGame(string fileName)
        {
            _instance = new GameModel(fileName);
        }

        #endregion

        #region SavingAndLoading
        private void LoadGame(string filename)
        => (_, IsPaused, Time, PlayerMoney) = _savefileManager.LoadGame(filename);

        public void SaveGame(string filename)
            => _savefileManager.SaveGame(new ISaveFileManager.GameData(this), filename);
        #endregion

        #region  Methods


        private static double GetMushroomCosts(Field field)
        {
            var cost = 0d;

            if (field.Surface is not Mushroom mushroom) return cost;

            var stage = (double)mushroom.GrowthStage + 1;
            cost = stage * GameSettings.MushroomPricePerUnit;
            return cost;
        }

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
            if(World.World.IsInitialised())
                UpdateAllMushrooms(0.1);
        }
        public void Update(double deltaTime)
        {
            if (IsPaused) return;
            _monthlyExpenseAccumulator += deltaTime;

            if (_monthlyExpenseAccumulator >= 600.0)
            {
                HandleMonthlyExpenses();
                _monthlyExpenseAccumulator = 0;
            }

            WorldInstance.Update(deltaTime);
            foreach (Factory factory in WorldInstance.Factories)
            {
                factory.Update(deltaTime, (float)Time);
            }
            foreach (Station station in WorldInstance.Stations)
            {
                station.GetWaitingPassengers(deltaTime);
            }
            GameAdvanced?.Invoke(this, EventArgs.Empty);
            Time += deltaTime;
        }

        public bool BuyVehicle(Vehicle v)
        {
            if (TryPurchase(v.Price))
            {
                v.ArrivedAtStation += HandleVehicleArrived;

                WorldInstance.AddVehicle(v);
                VehicleBought?.Invoke(this, EventArgs.Empty);
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
            double totalExpense = 0;

            lock (WorldInstance.Vehicles)
            {
                foreach (var vehicle in WorldInstance.Vehicles)
                {
                    totalExpense += 500;
                }
            }

            if (totalExpense > 0)
            {
                bool able = TryPurchase(totalExpense);
                if (!able)
                {
                    Pause();
                    GameOver?.Invoke(this, EventArgs.Empty);
                }
                Debug.WriteLine($"Havi kiadások levonva: -{totalExpense}$ (Járművek száma: {WorldInstance.Vehicles.Count})");
            }
        }

        public bool IsBuildable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsBuildable() ?? false;
        public bool IsHeightenable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsHeightenable() ?? false;
        public bool IsLowerable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsLowerable() ?? false;

        private void CheckAndRegisterJunctions(Coordinate centerCoord)
        {
            Coordinate[] coordsToCheck = {
                centerCoord,
                new Coordinate(centerCoord.X, centerCoord.Y - 1), // Észak
                new Coordinate(centerCoord.X, centerCoord.Y + 1), // Dél
                new Coordinate(centerCoord.X + 1, centerCoord.Y), // Kelet
                new Coordinate(centerCoord.X - 1, centerCoord.Y)  // Nyugat
            };

            foreach (var c in coordsToCheck)
            {
                var surface = WorldInstance.GetField(c)?.Surface;

                if (surface is Road r && r.RoadType.HasFlag(RoadType.JUNCTION) || surface is Station)
                {
                    WorldInstance.Roadnetwork.RegisterNodeIfNeeded(c);
                }
            }
        }

        private Road? CanPlaceRoadHere(Coordinate coord, Field? field)
        {
            if (null == field)
                return null;

            if (!field.IsBuildable())
                return null;

            foreach (var dir in Direction.Directions)
            {
                Field? adjField = WorldInstance.GetField(coord + dir);

                if (adjField?.Surface is Bridge)
                {
                    System.Diagnostics.Debug.WriteLine("Építés megtagadva: Híd mellé nem kerülhet út!");
                    return null;
                }
            }

            //creating a temporal to see if a road can be place here
            Road tempRoad = new(coord);


            field.Surface = tempRoad;
            tempRoad.Update();


            if (tempRoad.RoadType != RoadType.INVALID) return tempRoad;

            field.Surface = null;
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

            road.Update();
            WorldInstance.UpdateRoadNetworkAround(coord);

            CheckAndRegisterJunctions(coord);
            WorldInstance.Roadnetwork.RebuildEdges();

            RoadBought?.Invoke(this, EventArgs.Empty);
        }

        public bool PlaceBridge(Coordinate start, Coordinate end, GameSettings.BridgeData bridgeType)
        {
            if (start.X != end.X && start.Y != end.Y) return false;

            int dx = Math.Abs(start.X - end.X);
            int dy = Math.Abs(start.Y - end.Y);
            int length = Math.Max(dx, dy) + 1;

            if (length < 3 || length > bridgeType.Length) return false;

            Field? startField = WorldInstance.GetField(start);
            Field? endField = WorldInstance.GetField(end);

            if (startField == null || endField == null) return false;
            if (startField.Type != endField.Type) return false;

            var tempStartRoad = new Road(start);
            var tempEndRoad = new Road(end);

            var originalStartSurface = startField.Surface;
            var originalEndSurface = endField.Surface;

            startField.Surface = tempStartRoad;
            endField.Surface = tempEndRoad;

            tempStartRoad.Update();
            tempEndRoad.Update();

            bool bridgeHeadsValid = tempStartRoad.RoadType != RoadType.INVALID &&
                                   tempEndRoad.RoadType != RoadType.INVALID &&
                                   tempStartRoad.TryUpdateNeighbours() &&
                                   tempEndRoad.TryUpdateNeighbours();

            if (!bridgeHeadsValid)
            {
                startField.Surface = originalStartSurface;
                endField.Surface = originalEndSurface;
                tempStartRoad.UpdateNeighbours();
                tempEndRoad.UpdateNeighbours();
                return false;
            }

            int stepX = start.X == end.X ? 0 : (end.X > start.X ? 1 : -1);
            int stepY = start.Y == end.Y ? 0 : (end.Y > start.Y ? 1 : -1);
            RoadType bridgeDir = stepX == 0 ? RoadType.STRAIGHT_NS : RoadType.STRAIGHT_EW;

            List<Coordinate> bridgeCoords = [];
            for (int i = 0; i < length; i++)
            {
                var c = new Coordinate(start.X + i * stepX, start.Y + i * stepY);
                bridgeCoords.Add(c);
                Field? f = WorldInstance.GetField(c);

                if (i > 0 && i < length - 1)
                {
                    if (f == null || f.Type >= startField.Type || f.Surface is Road)
                    {
                        startField.Surface = originalStartSurface;
                        endField.Surface = originalEndSurface;
                        tempStartRoad.UpdateNeighbours();
                        tempEndRoad.UpdateNeighbours();
                        return false;
                    }
                }
            }

            for (int i = 1; i < length - 1; i++)
            {
                Coordinate c = bridgeCoords[i];
                foreach (var dir in Direction.Directions)
                {
                    Coordinate adjCoord = c + dir;
                    if (bridgeCoords.Contains(adjCoord)) continue;
                    Field? adjField = WorldInstance.GetField(adjCoord);

                    if (adjField != null && adjField.Surface is Road)
                    {
                        startField.Surface = originalStartSurface;
                        endField.Surface = originalEndSurface;
                        tempStartRoad.UpdateNeighbours();
                        tempEndRoad.UpdateNeighbours();
                        return false;
                    }
                }
            }

            double actualPrice = (bridgeType.Price / bridgeType.Length) * length;
            if (!TryPurchase(actualPrice))
            {
                startField.Surface = originalStartSurface;
                endField.Surface = originalEndSurface;
                tempStartRoad.UpdateNeighbours();
                tempEndRoad.UpdateNeighbours();
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                Coordinate c = bridgeCoords[i];
                Field field = WorldInstance.GetField(c)!;

                if (i == 0 || i == length - 1)
                    PlaceRoad(c);
                else
                    field.Surface = bridgeType.Tier switch
                    {
                        0 => new BoneBridge(c, bridgeDir, startField.Type),
                        1 => new StoneBridge(c, bridgeDir, startField.Type),
                        2 => new SteelBridge(c, bridgeDir, startField.Type),
                        _ => throw new NotImplementedException()
                    };
            }

            foreach (var c in bridgeCoords)
            {
                if (WorldInstance.GetField(c)?.Surface is Road r) 
                    r.Update();
                WorldInstance.UpdateRoadNetworkAround(c);

                CheckAndRegisterJunctions(c);

            }

            WorldInstance.Roadnetwork.RebuildEdges();
            RoadBought?.Invoke(this, EventArgs.Empty);

            return true;
        }

        public bool PlaceStation(Coordinate coord)
        {
            var stationCost = GameSettings.BaseStationPrice;

            if (!IsBuildable(coord) || PlayerMoney < stationCost) return false;

            if (WorldInstance.Stations.Any(s => s.Coordinate.Distance(coord) <= 3)) return false;

            var field = WorldInstance.GetField(coord);
            if (field == null) return false;

            var hasValidNearRoad = Direction.Directions.Any(dir =>
            {
                var neighbor = WorldInstance.GetField(coord + dir);
                return neighbor?.Surface is Road && neighbor.Type == field.Type;
            });

            if (!hasValidNearRoad) return false;

            stationCost += GetMushroomCosts(field);

            var city = WorldInstance.Cities.FirstOrDefault(c => c.CenterCoordinate.Distance(coord) <= 4);
            var factory = WorldInstance.Factories.FirstOrDefault(f => f.OriginCoordinate.Distance(coord) <= 4);

            Station? newStation = null;
            if (city != null) newStation = new CityStation(city, coord, $"CityStation{coord}");
            else if (factory != null) newStation = new FactoryStation(coord, $"FactoryStation{coord}", factory);

            if (newStation == null || !TryPurchase(stationCost)) return false;


            field.Surface = newStation;
            newStation.Update();

            if (!newStation.TryUpdateNeighbours())
            {
                field.Surface = null;
                newStation.UpdateNeighbours();
                AddMoney(stationCost);
                return false;
            }


            WorldInstance.Stations.Add(newStation);

            WorldInstance.UpdateRoadNetworkAround(coord);

            CheckAndRegisterJunctions(coord);
            WorldInstance.Roadnetwork.RebuildEdges();

            var chunkCoord = WorldInstance.GetChunkCoordinate(coord);
            WorldInstance.UpdateChunk(chunkCoord);

            return true;
        }

        public static void AddStopToVehicle(Vehicle v, Station s)
        {
            // TODO what happens if v.Route is null?
            if (v.Route!.Stops.Count > 0 && v.Route.Stops.Last() == s) return;

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
        }

        private void HandleVehicleArrived(object? sender, VehicleArrivedEventArgs e)
        {
            Debug.WriteLine($"[GameModel Üzleti Logika] {e.Vehicle.Name} megérkezett a(z) {e.Station.Coordinate} állomásra!");

            Vehicle vehicle = e.Vehicle;
            Station station = e.Station;
            float currentTime = (float)Time;
            ProductType productType = vehicle.CurrentType;
            Debug.WriteLine($"Várakozók: {station.WaitingPassengers}, Szabad hely: {vehicle.Capacity - vehicle.CurrentLoad}");
            if (vehicle is CargoTruck or TankerTruck)
            {
                int accepted = station.UnLoadProductFromVehicle(vehicle);
                if (accepted != 0)
                {
                    Debug.WriteLine($"{accepted} egység leadva a járműről.");
                    double price = GameSettings.GetPrice(productType);
                    AddMoney(accepted * price);
                    Debug.WriteLine("Pénz hozzáadva!");
                }
                if (station is FactoryStation fs)
                {
                    int amount = fs.LoadProduct(vehicle);
                    if (amount > 0)
                    {
                        Debug.WriteLine($"{accepted} egység felvéve a járműre.");
                    }
                }
            }
            else
            {
                int amount = station.UnBoarding(vehicle);
                if (amount != 0)
                {
                    Debug.WriteLine($"{amount} ember leszállt a járműről.");
                    double price = GameSettings.GetPrice(productType);
                    AddMoney(amount * price);
                    Debug.WriteLine("Pénz hozzáadva!");
                }
                if (vehicle.Capacity > vehicle.CurrentLoad)
                {
                    int loadedAmount = station.Boarding(vehicle);
                    if (loadedAmount > 0)
                    {
                        Debug.WriteLine($"[FELVÉTEL] {loadedAmount} egység felvéve a járműre.");
                    }
                }
            }
            VehicleArrivedAtStation?.Invoke(this, e);
        }

        private void UpdateAllMushrooms(double deltaTime)
        {
            int samplesCount = (int)(GameSettings.SamplesCount * deltaTime);
            HashSet<Chunk> chunksToRedraw = [];
            Random rand = WorldInstance.SharedRandom;

            for (int i = 0; i < samplesCount; i++)
            {
                int x = WorldInstance.SharedRandom.Next(0, WorldInstance.SizeInFields.X);
                int y = WorldInstance.SharedRandom.Next(0, WorldInstance.SizeInFields.Y);
                Coordinate randomCoord = new(x, y);

                Field? field = WorldInstance.GetField(randomCoord);

                if (field == null) continue;

                if (field.Type is < FieldType.LOW_LANDS or > FieldType.HIGH_LANDS) continue;

                if (field.Surface is null && rand.NextDouble() < GameSettings.NewSpreadChance)
                {
                    field.Surface = new Mushroom(randomCoord, MushroomGrowthStage.SPROUT);
                }
                else if (field.Surface is Mushroom mushroom)
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
                WorldInstance.UpdateChunk(chunk.Coordinate);
            }
        }

        #endregion

        private void TerraformField(Coordinate coord, int deltaHeight)
        {
            var terraformationPrice = GameSettings.BaseTerraformationPrice;

            var field = WorldInstance.GetField(coord);

            if (field == null)
                return;

            if (!((deltaHeight == -1 && field.IsLowerable()) || (deltaHeight == 1 && field.IsHeightenable())))
                return;

            terraformationPrice += GetMushroomCosts(field);

            var newFieldType = (FieldType)((int)field.Type + deltaHeight);

            if (newFieldType > FieldType.HIGH_MOUNTAINS) return;
            if (!TryPurchase(terraformationPrice)) return;

            field.SetFieldTypeTo(newFieldType);
            var chunkCoord = WorldInstance.GetChunkCoordinate(coord);

            WorldInstance.UpdateChunk(chunkCoord);

        }
        public void HeightenField(Coordinate coord) => TerraformField(coord, +1);
        public void LowerField(Coordinate coord) => TerraformField(coord, -1);

        public void Dispose()
        {
            _mushroomGrowthTimer.Dispose();
        }
    }
}
