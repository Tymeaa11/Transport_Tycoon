using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Persistance;
using VolcanicTransport.Model.Services;
using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.TerrainGeneration.Generators;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model
{

    public class GameModel : IDisposable
    {
        #region Fields

        public static World.World WorldInstance => World.World.Instance;

        private readonly EconomyService _economy;
        private readonly BuildingService _building;
        private readonly VehicleService _vehicles;
        private readonly TimerService _timer;
        private readonly SaveLoadService _saveLoad;

        public double Time => _timer.Time;

        public double PlayerMoney => _economy.PlayerMoney;

        public bool IsPaused => _timer.IsPaused;

        #endregion

        #region Events

        public event EventHandler<VehicleArrivedEventArgs>? VehicleArrivedAtStation
        {
            add => _vehicles.VehicleArrivedAtStation += value;
            remove => _vehicles.VehicleArrivedAtStation -= value;
        }

        public event EventHandler? MoneyChanged
        {
            add => _economy.MoneyChanged += value;
            remove => _economy.MoneyChanged -= value;
        }

        public event EventHandler? GameOver
        {
            add => _economy.GameOver += value;
            remove => _economy.GameOver -= value;
        }

        public event EventHandler? RoadBought
        {
            add => _building.RoadBought += value;
            remove => _building.RoadBought -= value;
        }

        public event EventHandler? VehicleBought
        {
            add => _vehicles.VehicleBought += value;
            remove => _vehicles.VehicleBought -= value;
        }

        public event EventHandler? VehicleSold
        {
            add => _vehicles.VehicleSold += value;
            remove => _vehicles.VehicleSold -= value;
        }

        public event EventHandler? GameAdvanced
        {
            add => _timer.GameAdvanced += value;
            remove => _timer.GameAdvanced -= value;
        }

        public event EventHandler? GamePaused
        {
            add => _timer.GamePaused += value;
            remove => _timer.GamePaused -= value;
        }

        public event EventHandler? GameUnpaused
        {
            add => _timer.GameUnpaused += value;
            remove => _timer.GameUnpaused -= value;
        }

        public event EventHandler? TimescaleChanged
        {
            add => _timer.TimescaleChanged += value;
            remove => _timer.TimescaleChanged -= value;
        }

        public event EventHandler? OnPlacementFailed
        {
            add => _building.OnPlacementFailed += value;
            remove => _building.OnPlacementFailed -= value;
        }

        #endregion

        #region Singleton

        private static GameModel? _instance;

        private GameModel()
        {
            _instance = this;
            _economy = new EconomyService(GameSettings.StartingMoney);
            _building = new BuildingService(() => WorldInstance, _economy);
            _timer = new TimerService(() => WorldInstance, _economy);
            _vehicles = new VehicleService(() => WorldInstance, _economy);
            _saveLoad = new SaveLoadService(
                new SaveFileManager(new Zip2FileSaveFormat()),
                _vehicles.HandleVehicleArrived);
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
            _timer.StartTimer();
        }

        private GameModel(string fileName) : this()
        {
            LoadGame(fileName);
            _timer.StartTimer();
        }

        ~GameModel() { Dispose(); }

        public static GameModel Instance => _instance ?? throw new GameModelNotInitialisedException();

        public static void InitialiseNewGame(int worldSize, int seed)
        {
            _instance?.Dispose();
            _instance = new GameModel(worldSize, seed);
        }

        public static void InitialiseLoadedGame(string fileName)
        {
            _instance?.Dispose();
            _instance = new GameModel(fileName);
        }

        #endregion

        #region Economy

        public bool TryPurchase(double amount) => _economy.TryPurchase(amount);

        #endregion

        #region Building

        public bool IsBuildable(Coordinate coordinate) => _building.IsBuildable(coordinate);
        public bool IsHeightenable(Coordinate coordinate) => _building.IsHeightenable(coordinate);
        public bool IsLowerable(Coordinate coordinate) => _building.IsLowerable(coordinate);
        public void PlaceRoad(Coordinate coord) => _building.PlaceRoad(coord);
        public bool PlaceBridge(Coordinate start, Coordinate end, GameSettings.BridgeData bridgeType)
            => _building.PlaceBridge(start, end, bridgeType);
        public bool PlaceStation(Coordinate coord) => _building.PlaceStation(coord);
        public void HeightenField(Coordinate coord) => _building.HeightenField(coord);
        public void LowerField(Coordinate coord) => _building.LowerField(coord);

        #endregion

        #region Timer

        public void Pause() => _timer.Pause();

        public void UnPause() => _timer.UnPause();

        public void ChangeTimeSpeed1X() => _timer.ChangeTimeSpeed1X();
        public void ChangeTimeSpeed2X() => _timer.ChangeTimeSpeed2X();
        public void ChangeTimeSpeed4X() => _timer.ChangeTimeSpeed4X();
        public void Update(double deltaTime) => _timer.Update(deltaTime);

        #endregion

        #region Vehicles

        public bool BuyVehicle(Vehicle v) => _vehicles.BuyVehicle(v);

        public void SellVehicle(Vehicle v) => _vehicles.SellVehicle(v);

        public bool CreateAndStartVehicle(string vehicleType, string vehicleName, Route chosenRoute)
            => _vehicles.CreateAndStartVehicle(vehicleType, vehicleName, chosenRoute);

        public static void AddStopToVehicle(Vehicle v, Station s)
            => VehicleService.AddStopToVehicle(v, s);

        public void HandleVehicleArrived(object? sender, VehicleArrivedEventArgs e)
            => _vehicles.HandleVehicleArrived(sender, e);

        #endregion

        #region SaveLoad

        private void LoadGame(string filename)
        {
            var data = _saveLoad.LoadGame(filename);
            _timer.SetIsPaused(data.IsPaused);
            _timer.SetTime(data.Time);
            _economy.SetPlayerMoney(data.PlayerMoney);
        }

        public void SaveGame(string filename) => _saveLoad.SaveGame(new GameData(this), filename);

        #endregion

        public void Dispose()
        {
            _timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
