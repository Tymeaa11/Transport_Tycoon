using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_WPF.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        public static GameModel GameModelInstance { get => GameModel.Instance; }
        private bool _isPausedView;

        #region Events
        public event EventHandler? ExitToMenuRequested;
        #endregion

        public bool IsPausedView
        {
            get => _isPausedView;
            set
            {
                _isPausedView = value;
                OnPropertyChanged();
                if (_isPausedView)
                {
                    GameModelInstance.Pause();
                }
                else
                {
                    GameModelInstance.UnPause();
                }
                RefreshTimescaleProperties();
            }
        }
        private void RefreshTimescaleProperties()
        {
            OnPropertyChanged(nameof(IsTimescale0));
            OnPropertyChanged(nameof(IsTimescale1));
            OnPropertyChanged(nameof(IsTimescale2));
            OnPropertyChanged(nameof(IsTimescale4));
        }

        #region Chunks
        public ObservableCollection<Chunk> LoadedChunks { get; } = [];

        public void SetViewDimensions(double width, double height)
        {
            _lastWidth = width;
            _lastHeight = height;
            UpdateVisibleChunks(width, height);
        }

        public void UpdateVisibleChunks(double width, double height)
        {
            Rect bounds = Camera.GetVisibleWorldBounds(width, height);

            // Get visible chunk coordinates (+1 buffer)
            int chunkPX = GameSettings.ChunkSize * GameSettings.FieldSize;

            int startX = (int)Math.Floor(bounds.Left / chunkPX) - 1;
            int endX = (int)Math.Ceiling(bounds.Right / chunkPX) + 1;
            int startY = (int)Math.Floor(bounds.Top / chunkPX) - 1;
            int endY = (int)Math.Ceiling(bounds.Bottom / chunkPX) + 1;

            HashSet<Coordinate> visibleCoords = [];

            for (int x = startX; x <= endX; x++)
                for (int y = startY; y <= endY; y++)
                    if (x >= 0 && x < WorldSizeInChunks.X && y >= 0 && y < WorldSizeInChunks.Y)
                        visibleCoords.Add(new Coordinate(x, y));


            // 1. Remove if outside
            var toRemove = LoadedChunks.Where(c => !visibleCoords.Contains(c.Coordinate)).ToList();
            foreach (var chunk in toRemove) LoadedChunks.Remove(chunk);

            // 2. Add if became visible
            foreach (var coord in visibleCoords)
                if (!LoadedChunks.Any(c => c.Coordinate.Equals(coord)))
                {
                    var chunk = GameModel.WorldInstance.GetChunk(coord);
                    if (chunk != null) LoadedChunks.Add(chunk);
                }
        }

        #endregion

        public Coordinate WorldSizeInChunks => GameModel.WorldInstance.SizeInChunks;
        public Camera Camera { get; }

        #region Commands
        public DelegateCommand SetBuildModeRoadCommand { get; private set; }
        public DelegateCommand SetBuildModeStationCommand { get; private set; }
        public DelegateCommand SetBuildModeBridgeCommand { get; private set; }
        public DelegateCommand SetBuildModeLowerCommand { get; private set; }
        public DelegateCommand SetBuildModeHeightenCommand { get; private set; }
        public DelegateCommand SetTimescale0Command { get; private set; }
        public DelegateCommand SetTimescale1Command { get; private set; }
        public DelegateCommand SetTimescale2Command { get; private set; }
        public DelegateCommand SetTimescale4Command { get; private set; }
        public DelegateCommand TogglePauseCommand { get; private set; }
        public DelegateCommand ResumeCommand { get; private set; }
        public DelegateCommand QuitToMainMenuCommand { get; private set; }
        public DelegateCommand ReGenerateWithRandomSeed { get; private set; }
        public DelegateCommand BuyVehicleCommand { get; private set; }
        public DelegateCommand AddStopCommand { get; }
        #endregion

        #region FieldClicked
        public DelegateCommand FieldClickedCommand { get; private set; }

        public DelegateCommand ClearRouteCommand { get; private set; }

        private void OnFieldClicked(Coordinate coord)
        {
            System.Diagnostics.Debug.WriteLine($"Field clicked at: {coord.X}, {coord.Y}");
            Field? f = GameModel.WorldInstance.GetField(coord);
            if (f == null) return;

            if (CurrentBuildMode == BuildMode.SELECT_STATION)
            {
                System.Diagnostics.Debug.WriteLine($"SELECT_STATION mód aktív. Mező felülete: {f.Surface?.GetType().Name}");
                if (f.Surface is Station clickedStation)
                {
                    if (SelectedVehicle != null)
                    {
                        var v = SelectedVehicle.GetVehicle;
                        if (v != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Station megvan, küldöm a modellnek!");
                            GameModel.AddStopToVehicle(v, clickedStation);
                        }
                    }
                    CurrentBuildMode = BuildMode.NONE;
                }
                return;
            }

            if (CurrentBuildMode == BuildMode.BUY_VEHICLE)
            {
                if (f.Surface is Station clickedStation)
                {
                    if (_firstSelectedStation == null)
                    {
                        // 1. KATTINTÁS: Eltároljuk a start állomást
                        _firstSelectedStation = clickedStation;
                        System.Diagnostics.Debug.WriteLine($"1. állomás rögzítve: {clickedStation.Coordinate}. Kattints a célra!");
                    }
                    else
                    {
                        // 2. KATTINTÁS: Megvan a cél állomás
                        Station secondSelectedStation = clickedStation;

                        if (_firstSelectedStation == secondSelectedStation)
                        {
                            System.Diagnostics.Debug.WriteLine("A cél nem lehet ugyanaz, mint a start!");
                            return;
                        }

                        var nodes = GameModel.WorldInstance.Roadnetwork.NodeMap;
                        RoadNode? startNode = nodes.Values.FirstOrDefault(n => n.Coordinate == _firstSelectedStation.Coordinate);
                        RoadNode? endNode = nodes.Values.FirstOrDefault(n => n.Coordinate == secondSelectedStation.Coordinate);

                        if (startNode != null && endNode != null)
                        {

                            if (startNode != null && endNode != null)
                            {
                                if (SelectedVehicle != null)
                                {
                                    Vehicle v = SelectedVehicle.GetVehicle;

                                    Route newRoute = new Route();
                                    newRoute.AddStop(_firstSelectedStation);
                                    newRoute.AddStop(secondSelectedStation);

                                    v.AssignNewRoute(newRoute);

                                    System.Diagnostics.Debug.WriteLine($"[{v.Name}] Új menetrend fiókba téve! Amint beér a megállóba, irányt vált.");
                                }
                                else
                                {
                                    List<Road>? path = Pathfinder.FindPath(startNode, endNode);

                                    if (path != null && path.Count > 0)
                                    {
                                        var nameDialog = new VolcanicTransport_WPF.View.VehicleNameWindow();
                                        nameDialog.Owner = System.Windows.Application.Current.MainWindow;

                                        if (nameDialog.ShowDialog() == true)
                                        {
                                            string chosenName = string.IsNullOrWhiteSpace(nameDialog.VehicleName) ? "Névtelen Busz" : nameDialog.VehicleName;

                                            var newBus = new Bus(chosenName, VolcanicTransport.Model.World.Economy.ProductType.HUMAN);
                                            Route initialRoute = new Route();
                                            initialRoute.AddStop(_firstSelectedStation);
                                            initialRoute.AddStop(secondSelectedStation);
                                            newBus.Route = initialRoute;
                                            newBus.CurrentStopIndex = 1;

                                            newBus.StartJourney(path, false, _firstSelectedStation);
                                            GameModelInstance.BuyVehicle(newBus);
                                            System.Diagnostics.Debug.WriteLine($"Új busz sikeresen megvéve: {chosenName}");
                                        } else
                                        {
                                            System.Diagnostics.Debug.WriteLine("Vásárlás megszakítva.");
                                        }
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine("Nincs összefüggő aszfalt a két állomás között!");
                                    }
                                }
                            }

                            _firstSelectedStation = null;
                            CurrentBuildMode = BuildMode.NONE;
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Kérlek egy állomásra kattints!");
                }
            }


            if (f.IsBuildable())
            {
                switch (CurrentBuildMode)
                {
                    case BuildMode.ROAD:
                        GameModelInstance.PlaceRoad(coord);
                        break;

                    case BuildMode.STATION:
                        GameModelInstance.PlaceStation(coord);
                        break;

                    case BuildMode.BUY_VEHICLE:
                        break;
                }
            }

            if (CurrentBuildMode == BuildMode.HEIGHTEN)
            {
                GameModelInstance.HeightenField(coord);
            }

            if (CurrentBuildMode == BuildMode.LOWER)
            {
                GameModelInstance.LowerField(coord);
            }
        }

        #endregion

        #region Hovered field & Tooltips

        public void UpdateHoveredCoordinateAndTooltips(Point mouseXY)
        {
            HoveredCoordinate = Camera.ScreenToField(mouseXY);

            UpdateBuildability();

            _hoveredField = GameModel.WorldInstance.GetField(HoveredCoordinate);

            var text = $"X:{HoveredCoordinate.X} Y:{HoveredCoordinate.Y}  ";

            if (_hoveredField != null)
            {
                text += $"{_hoveredField.Type} ({(int)_hoveredField.Type})";

                if (_hoveredField.Surface != null)
                {
                    text += " - ";

                    text += _hoveredField.Surface switch
                    {
                        Mushroom m => $"M({m.GrowthStage})",
                        Road r => $"R({r.RoadType})",
                        Station _ => $"S",
                        CityBuilding _ => $"C",
                        FactoryBuilding _ => $"F",
                        _ => "Not listed"
                    };
                }
            }
            else text += "-";

            ToolTipText = text;
        }

        public int TileSize => GameSettings.FieldSize; //used to size the hovered field highlight

        private Field? _hoveredField;

        private Coordinate _hoveredCoordinate;
        public Coordinate HoveredCoordinate
        {
            get => _hoveredCoordinate;
            set
            {
                _hoveredCoordinate = value;
                OnPropertyChanged();
            }
        }

        private string _toolTipText = "";
        public string ToolTipText
        {
            get => _toolTipText;
            set
            {
                _toolTipText = value;
                OnPropertyChanged();
            }
        }

        private bool _isHoveredFieldBuildable;
        public bool IsHoveredFieldBuildable
        {
            get => _isHoveredFieldBuildable;
            set { _isHoveredFieldBuildable = value; OnPropertyChanged(); }
        }

        private void UpdateBuildability()
        {
            IsHoveredFieldBuildable = CurrentBuildMode switch
            {
                BuildMode.HEIGHTEN => GameModelInstance.IsHeightenable(HoveredCoordinate),
                BuildMode.LOWER => GameModelInstance.IsLowerable(HoveredCoordinate),
                _ => GameModelInstance.IsBuildable(HoveredCoordinate),
            };
        }
        #endregion


        #region Vehicles
        public ObservableCollection<Vehicle> Vehicles => World.Instance.Vehicles;

        public ObservableCollection<VehicleViewModel> VehicleViewModels { get; } = [];

        private Station? _firstSelectedStation = null;

        private VehicleViewModel? _selectedVehicle;
        public VehicleViewModel? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                _selectedVehicle = value;
                OnPropertyChanged(nameof(SelectedVehicle));
                OnPropertyChanged(nameof(IsVehiclePanelVisible));
            }
        }

        private double _accumulator = 0;
        private const double FIXED_DELTA_TIME = 1.0 / 60.0; // Fix 60 FPS-es fizikai lépés (0.0166s)

        public bool IsVehiclePanelVisible => SelectedVehicle != null;

        #endregion

        private double _lastWidth;
        private double _lastHeight;

        public GameViewModel()
        {
            Camera = new Camera(Matrix.Identity);

            Camera.CameraChanged += (s, e) => UpdateVisibleChunks(_lastWidth, _lastHeight);

            CurrentTimescale = 1;

            FieldClickedCommand = new DelegateCommand(param =>
            {
                if (param is Coordinate coord)
                    OnFieldClicked(coord);
            });
            SetBuildModeRoadCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.ROAD));
            SetBuildModeStationCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.STATION));
            SetBuildModeBridgeCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.BRIDGE));
            SetBuildModeLowerCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.LOWER));
            SetBuildModeHeightenCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.HEIGHTEN));
            SetTimescale0Command = new DelegateCommand(_ => OnSetTimescale0X());
            SetTimescale1Command = new DelegateCommand(_ => OnSetTimescale1X());
            SetTimescale2Command = new DelegateCommand(_ => OnSetTimescale2X());
            SetTimescale4Command = new DelegateCommand(_ => OnSetTimescale4X());
            BuyVehicleCommand = new DelegateCommand(_ =>
            {
                SelectedVehicle = null;
                _firstSelectedStation = null;
                OnSetBuildMode(BuildMode.BUY_VEHICLE);
            });
            TogglePauseCommand = new DelegateCommand(_ => IsPausedView = !IsPausedView);
            ResumeCommand = new DelegateCommand(_ => IsPausedView = false);
            /*
            QuitToMainMenuCommand = new DelegateCommand(_ =>
            {
                ExitToMenuRequested?.Invoke(this, EventArgs.Empty);
            });
            */
            AddStopCommand = new DelegateCommand(_ =>
            {
                if (SelectedVehicle != null)
                {
                    CurrentBuildMode = BuildMode.SELECT_STATION;
                    System.Diagnostics.Debug.WriteLine("Válassz megállót a térképen!");
                    System.Diagnostics.Debug.WriteLine($"Siker: Mód átváltva: {CurrentBuildMode}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("HIBA: Nincs kijelölt jármű, nem tudok módot váltani!");
                }
            });

            ReGenerateWithRandomSeed = new DelegateCommand(_ =>
            {
                GameModel.WorldInstance.Generate(new Random().Next());
                LoadedChunks.Clear();
                GameModel.WorldInstance.ChunkMatrix.ReadEach((x, y, c) => c.RemoveAllUpdateTriggers());
                UpdateVisibleChunks(_lastWidth, _lastHeight);
            }
            );

            ClearRouteCommand = new DelegateCommand(_ =>
            {
                if (SelectedVehicle != null)
                {
                    SelectedVehicle.GetVehicle.ClearRoute();

                    _firstSelectedStation = null;
                    CurrentBuildMode = BuildMode.BUY_VEHICLE;
                    System.Diagnostics.Debug.WriteLine($"Menetrend törölve a {SelectedVehicle.GetName} járművön. Válassz új start állomást!");
                }
            });

            //GameModelInstance.moneyChanged += GameModelInstance_moneyChanged;
        }

        private void GameModelInstance_moneyChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentMoney));
        }

        public void Initialise()
        {
            GameModel.Initialise(8, 0);

            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(Vehicles, _vehiclesLock);

            GameModelInstance.MoneyChanged += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(CurrentMoney)));
            };

            GameModelInstance.VehicleBought += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var newModelVehicle = GameModel.WorldInstance.GetLatestVehicle();
                    if (newModelVehicle != null)
                    {
                        var vvm = new VehicleViewModel(newModelVehicle);
                        VehicleViewModels.Add(vvm);
                        SelectedVehicle = vvm;
                        System.Diagnostics.Debug.WriteLine($"Sikeres vétel! SelectedVehicle neve: {vvm.GetName}");
                    }
                });
            };
            StartGameLoop();
        }

        private BuildMode currentBuildMode = BuildMode.NONE;
        public BuildMode CurrentBuildMode
        {
            get => currentBuildMode;
            set
            {
                if (currentBuildMode != value)
                {
                    currentBuildMode = value;
                    OnPropertyChanged(nameof(IsBuildModeRoad));
                    OnPropertyChanged(nameof(IsBuildModeStation));
                    OnPropertyChanged(nameof(IsBuildModeBuyVehicle));
                    OnPropertyChanged(nameof(IsBuildModeSelectStation));
                    OnPropertyChanged(nameof(IsBuildModeBridge));
                    OnPropertyChanged(nameof(IsBuildModeLower));
                    OnPropertyChanged(nameof(IsBuildModeHeighten));
                }
            }
        }

        private int currentTimescale;
        public int CurrentTimescale
        {
            get => currentTimescale;
            set
            {
                if (currentTimescale == value) return;

                currentTimescale = value;
                OnPropertyChanged(nameof(IsTimescale0));
                OnPropertyChanged(nameof(IsTimescale1));
                OnPropertyChanged(nameof(IsTimescale2));
                OnPropertyChanged(nameof(IsTimescale4));

            }
        }
        public bool IsBuildModeRoad => CurrentBuildMode == BuildMode.ROAD;
        public bool IsBuildModeStation => CurrentBuildMode == BuildMode.STATION;

        public bool IsBuildModeSelectStation => CurrentBuildMode == BuildMode.SELECT_STATION;

        public bool IsBuildModeBuyVehicle => CurrentBuildMode == BuildMode.BUY_VEHICLE;
        public bool IsBuildModeBridge => CurrentBuildMode == BuildMode.BRIDGE;
        public bool IsBuildModeLower => CurrentBuildMode == BuildMode.LOWER;
        public bool IsBuildModeHeighten => CurrentBuildMode == BuildMode.HEIGHTEN;

        public bool IsTimescale0 => CurrentTimescale == 0 || IsPausedView || GameModelInstance.IsPaused;
        public bool IsTimescale1 => !IsPausedView && !GameModelInstance.IsPaused && CurrentTimescale == 1;
        public bool IsTimescale2 => !IsPausedView && !GameModelInstance.IsPaused && CurrentTimescale == 2;
        public bool IsTimescale4 => !IsPausedView && !GameModelInstance.IsPaused && CurrentTimescale == 4;

        public string CurrentMoney
        {
            get => GameModelInstance.PlayerMoney.ToString("F0") + " $";
        }

        private readonly object _vehiclesLock = new();

        //private readonly DispatcherTimer _gameLoop;
        private System.Diagnostics.Stopwatch _stopwatch;
        private TimeSpan _lastRenderTime = TimeSpan.Zero;

        public void StartGameLoop()
        {
            _stopwatch = new System.Diagnostics.Stopwatch();
            _stopwatch.Start();

            CompositionTarget.Rendering += OnCompositionTargetRendering;
        }
        private const double BASE_SPEED_MULTIPLIER = 10.0;
        private void OnCompositionTargetRendering(object? sender, EventArgs e)
        {
            TimeSpan currentRenderTime = _stopwatch.Elapsed;
            double deltaTime = (currentRenderTime - _lastRenderTime).TotalSeconds;
            _lastRenderTime = currentRenderTime;

            if (CurrentTimescale == 0)
            {
                _accumulator = 0;
            }

            if (deltaTime > 0.1) deltaTime = 0.1;

            _accumulator += deltaTime * CurrentTimescale * BASE_SPEED_MULTIPLIER;

            while (_accumulator >= FIXED_DELTA_TIME)
            {
                GameModelInstance.Update(FIXED_DELTA_TIME);
                _accumulator -= FIXED_DELTA_TIME;
            }
        }
        private void OnSetBuildMode(BuildMode mode)
        {
            BuildMode previousMode = CurrentBuildMode;

            if (CurrentBuildMode == mode)
            {
                CurrentBuildMode = BuildMode.NONE;
            }
            else
            {
                CurrentBuildMode = mode;
            }

            if (previousMode == BuildMode.ROAD && CurrentBuildMode != BuildMode.ROAD)
            {
                System.Diagnostics.Debug.WriteLine("Útépítés befejezve! Élek (Edges) újraépítése...");

                GameModel.WorldInstance.Roadnetwork.RebuildEdges();
            }

        }

        private void OnSetTimescale0X()
        {
            CurrentTimescale = 0;
            GameModelInstance.Pause();

        }

        private void OnSetTimescale1X()
        {
            CurrentTimescale = 1;
            GameModelInstance.UnPause();
            GameModelInstance.ChangeTimeSpeed1X();
        }

        private void OnSetTimescale2X()
        {
            CurrentTimescale = 2;
            GameModelInstance.UnPause();
            GameModelInstance.ChangeTimeSpeed2X();
        }

        private void OnSetTimescale4X()
        {
            CurrentTimescale = 4;
            GameModelInstance.UnPause();
            GameModelInstance.ChangeTimeSpeed4X();
        }



    }
}
