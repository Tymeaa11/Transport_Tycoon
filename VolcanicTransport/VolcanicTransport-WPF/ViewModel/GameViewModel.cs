using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using VolcanicTransport_WPF.View;

namespace VolcanicTransport_WPF.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        public static GameModel GameModelInstance { get => GameModel.Instance; }
        public Coordinate WorldSizeInChunks => GameModel.WorldInstance.SizeInChunks;
        public int TileSize => GameSettings.FieldSize; //used to size the hovered field highlight
        public Camera Camera { get; }
        public string CurrentMoney => GameModelInstance.PlayerMoney.ToString("F0") + " $";


        #region Events
        public event EventHandler? ExitToMenuRequested;

        private void GameModelInstance_moneyChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentMoney));
        }

        private void On_UpdateChunk(object? sender, ChunkUpdatedEventArgs e)
        {
            if (ChunkMap.ContainsKey(e.ChunkCoordinate))
                ChunkMap[e.ChunkCoordinate].TriggerRerender();
        }

        #endregion

        #region Pause
        private bool _isPausedView;
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
            OnPropertyChanged(nameof(IsTimeScale0));
            OnPropertyChanged(nameof(IsTimeScale1));
            OnPropertyChanged(nameof(IsTimeScale2));
            OnPropertyChanged(nameof(IsTimeScale4));
        }
        #endregion

        #region Chunks
        public ObservableCollection<ChunkViewModel> LoadedChunks { get; }
        public Dictionary<Coordinate, ChunkViewModel> ChunkMap { get; }

        private double _lastWidth;
        private double _lastHeight;
        public void SetViewDimensions(double width, double height)
        {
            _lastWidth = width;
            _lastHeight = height;
            UpdateVisibleChunks();
            Camera.HalfScreenDimensions = new Vector(width * 0.5, height * 0.5);
        }



        public void UpdateVisibleChunks()
        {
            Rect bounds = Camera.GetVisibleWorldBounds();

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

            // Set visibility based on whether the coordinate is in the visible set
            foreach (var kvp in ChunkMap)
                kvp.Value.IsVisible = visibleCoords.Contains(kvp.Key);
        }

        #endregion

        #region Commands
        public DelegateCommand SetBuildModeRoadCommand { get; private set; }
        public DelegateCommand SetBuildModeStationCommand { get; private set; }
        public DelegateCommand SetBuildModeBridgeCommand { get; private set; }
        public DelegateCommand SetBuildModeLowerCommand { get; private set; }
        public DelegateCommand SetBuildModeHeightenCommand { get; private set; }
        public DelegateCommand SetTimeScale0Command { get; private set; }
        public DelegateCommand SetTimeScale1Command { get; private set; }
        public DelegateCommand SetTimeScale2Command { get; private set; }
        public DelegateCommand SetTimeScale4Command { get; private set; }
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
            if (IsPausedView) return;
            Debug.WriteLine($"Field clicked at: {coord.X}, {coord.Y}");

            //Camera.PrintDebug();

            //foreach (var cvm in LoadedChunks)
            //    System.Diagnostics.Debug.WriteLine($"CVM {cvm.Chunk.Coordinate}, {cvm.IsVisible}");
            //System.Diagnostics.Debug.WriteLine("\n");

            Field? field = GameModel.WorldInstance.GetField(coord);
            if (field == null) return;

            switch (CurrentBuildMode)
            {
                case BuildMode.SELECT_STATION:
                    OnBuildModeSelectStation(field);
                    break;

                case BuildMode.BUY_VEHICLE:
                    OnBuildModeBuyVehicle(field);
                    break;

                case BuildMode.ROAD:
                    if (field.IsBuildable()) 
                        GameModelInstance.PlaceRoad(coord);
                    break;

                case BuildMode.STATION:
                    if (field.IsBuildable()) 
                        GameModelInstance.PlaceStation(coord);
                    break;

                case BuildMode.HEIGHTEN:
                    GameModelInstance.HeightenField(coord);
                    break;

                case BuildMode.LOWER:
                    GameModelInstance.LowerField(coord);
                    break;

                default:
                    // Handle BuildMode.NONE or unhandled cases
                    break;
            }
        }

        private void OnBuildModeSelectStation(Field field)
        {
            if (IsPausedView) return;
            Debug.WriteLine($"SELECT_STATION mód aktív. Mező felülete: {field.Surface?.GetType().Name}");
            if (field.Surface is Station clickedStation)
            {
                if (SelectedVehicle != null)
                {
                    var v = SelectedVehicle.GetVehicle;
                    if (v != null)
                    {
                        Debug.WriteLine("Station megvan, küldöm a modellnek!");
                        GameModel.AddStopToVehicle(v, clickedStation);
                    }
                }
                CurrentBuildMode = BuildMode.NONE;
            }
        }

        private void OnBuildModeBuyVehicle(Field field)
        {
            if (IsPausedView) return;
            if (field.Surface is Station clickedStation)
            {
                if (_firstSelectedStation == null)
                {
                    // 1. KATTINTÁS: Eltároljuk a start állomást
                    _firstSelectedStation = clickedStation;
                    Debug.WriteLine($"1. állomás rögzítve: {clickedStation.Coordinate}. Kattints a célra!");
                }
                else
                {
                    // 2. KATTINTÁS: Megvan a cél állomás
                    Station secondSelectedStation = clickedStation;

                    if (_firstSelectedStation == secondSelectedStation)
                    {
                        Debug.WriteLine("A cél nem lehet ugyanaz, mint a start!");
                        return;
                    }

                    var nodes = GameModel.WorldInstance.Roadnetwork.NodeMap;
                    RoadNode? startNode = nodes.Values.FirstOrDefault(n => n.Coordinate == _firstSelectedStation.Coordinate);
                    RoadNode? endNode = nodes.Values.FirstOrDefault(n => n.Coordinate == secondSelectedStation.Coordinate);

                    if (startNode != null && endNode != null)
                    {
                        if (SelectedVehicle != null)
                        {
                            Vehicle v = SelectedVehicle.GetVehicle;

                            var newRoute = new Route();
                            newRoute.AddStop(_firstSelectedStation);
                            newRoute.AddStop(secondSelectedStation);

                            v.AssignNewRoute(newRoute);

                            Debug.WriteLine($"[{v.Name}] Új menetrend fiókba téve! Amint beér a megállóba, irányt vált.");
                        }
                        else
                        {
                            List<Road>? path = Pathfinder.FindPath(startNode, endNode);

                            if (path != null && path.Count > 0)
                            {
                                // TODO : Is this allowed in MVVM?
                                var nameDialog = new VehicleNameWindow { Owner = Application.Current.MainWindow };

                                if (nameDialog.ShowDialog() == true)
                                {
                                    string chosenName = nameDialog.VehicleName;
                                    string? chosenType = nameDialog.SelectedType;

                                    Vehicle newVehicle = chosenType switch
                                    {
                                        "CargoTruck" => new CargoTruck(chosenName),
                                        "TankerTruck" => new TankerTruck(chosenName),
                                        "MiniBus" => new MiniBus(chosenName),
                                        _ => new Bus(chosenName)
                                    };

                                    var initialRoute = new Route();
                                    initialRoute.AddStop(_firstSelectedStation);
                                    initialRoute.AddStop(secondSelectedStation);
                                    newVehicle.Route = initialRoute;
                                    newVehicle.CurrentStopIndex = 1;

                                    newVehicle.StartJourney(path, false, _firstSelectedStation);
                                    GameModelInstance.BuyVehicle(newVehicle);
                                    Debug.WriteLine($"Új busz sikeresen megvéve: {chosenName} ({chosenType})");
                                }
                                else
                                {
                                    Debug.WriteLine("Vásárlás megszakítva.");
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Nincs összefüggő aszfalt a két állomás között!");
                            }
                        }

                        _firstSelectedStation = null;
                        CurrentBuildMode = BuildMode.NONE;
                    }
                }
            }
            else
            {
                Debug.WriteLine("Kérlek egy állomásra kattints!");
            }
        }

        #endregion

        #region Hovered field & Inspector

        private bool _isInspectorVisible;
        public bool IsInspectorVisible
        {
            get => _isInspectorVisible;
            set { _isInspectorVisible = value; OnPropertyChanged(); }
        }

        private string _inspectorText = "";
        public string InspectorText
        {
            get => _inspectorText;
            set { _inspectorText = value; OnPropertyChanged(); }
        }
        private Vector _lastMousePosition;

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
            if (IsPausedView) return;
            IsHoveredFieldBuildable = CurrentBuildMode switch
            {
                BuildMode.HEIGHTEN => GameModelInstance.IsHeightenable(HoveredCoordinate),
                BuildMode.LOWER => GameModelInstance.IsLowerable(HoveredCoordinate),
                _ => GameModelInstance.IsBuildable(HoveredCoordinate),
            };
        }


        public void UpdateHoveredCoordinateAndTooltips(Vector mouseXY)
        {
            if (IsPausedView) return;
            _lastMousePosition = mouseXY;
            Camera.CurrentMousePosition = mouseXY;

            HoveredCoordinate = Camera.ScreenToField(mouseXY);
            _hoveredField = GameModel.WorldInstance.GetField(HoveredCoordinate);

            UpdateBuildability();


            var cornerSb = new StringBuilder();
            cornerSb.Append($"X:{HoveredCoordinate.X} Y:{HoveredCoordinate.Y} ");

            if (_hoveredField != null)
            {
                cornerSb.Append($"| {_hoveredField.Type} ({(int)_hoveredField.Type})");

                string surfaceDetail = _hoveredField.Surface switch
                {
                    Mushroom m => $" | M({m.GrowthStage})",
                    Road r => $" | R({r.RoadType})",
                    _ => ""
                };

                cornerSb.Append(surfaceDetail);
            }

            ToolTipText = cornerSb.ToString();

            InspectorText = (_hoveredField?.Surface is IInspectable inspectable)
                ? inspectable.Inspect()
                : "";

            IsInspectorVisible = !string.IsNullOrEmpty(InspectorText);
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



        public GameViewModel()
        {
            Camera = new Camera();
            Camera.CameraChanged += (s, e) => UpdateVisibleChunks();

            CurrentTimeScale = 1;

            LoadedChunks = [];
            ChunkMap = [];

            FieldClickedCommand = new DelegateCommand(param =>
            {
                if (param is Coordinate coord)
                    OnFieldClicked(coord);
            });
            SetBuildModeRoadCommand = new DelegateCommand(
                _ => OnSetBuildMode(BuildMode.ROAD));
            SetBuildModeStationCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.STATION));
            SetBuildModeBridgeCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.BRIDGE));
            SetBuildModeLowerCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.LOWER));
            SetBuildModeHeightenCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.HEIGHTEN));
            SetTimeScale0Command = new DelegateCommand(_ => OnSetTimescale0X());
            SetTimeScale1Command = new DelegateCommand(_ => OnSetTimescale1X());
            SetTimeScale2Command = new DelegateCommand(_ => OnSetTimescale2X());
            SetTimeScale4Command = new DelegateCommand(_ => OnSetTimescale4X());
            BuyVehicleCommand = new DelegateCommand(_ =>
            {
                SelectedVehicle = null;
                _firstSelectedStation = null;
                OnSetBuildMode(BuildMode.BUY_VEHICLE);
            });
            TogglePauseCommand = new DelegateCommand(_ => IsPausedView = !IsPausedView);
            ResumeCommand = new DelegateCommand(_ => IsPausedView = false);
            QuitToMainMenuCommand = new DelegateCommand(_ =>
            {
                ExitToMenuRequested?.Invoke(this, EventArgs.Empty);
            });
            AddStopCommand = new DelegateCommand(_ =>
            {
                if (IsPausedView) return;
                if (SelectedVehicle != null)
                {
                    CurrentBuildMode = BuildMode.SELECT_STATION;
                    Debug.WriteLine("Válassz megállót a térképen!");
                    Debug.WriteLine($"Siker: Mód átváltva: {CurrentBuildMode}");
                }
                else
                {
                    Debug.WriteLine("HIBA: Nincs kijelölt jármű, nem tudok módot váltani!");
                }
            });
            ReGenerateWithRandomSeed = new DelegateCommand(_ =>
            {
                LoadedChunks.Clear();
                ChunkMap.Clear();
                
                Initialise();

                GameModel.WorldInstance.ChunkMatrix.ReadEach((x, y, c) =>
                {
                    ChunkViewModel chunkViewModel = new(c);
                    LoadedChunks.Add(chunkViewModel);
                    ChunkMap[new(x, y)] = chunkViewModel;
                });

                UpdateVisibleChunks();

                Camera.Reset();

            }
            );
            ClearRouteCommand = new DelegateCommand(_ =>
            {
                if (SelectedVehicle != null)
                {
                    SelectedVehicle.GetVehicle.ClearRoute();

                    _firstSelectedStation = null;
                    CurrentBuildMode = BuildMode.BUY_VEHICLE;
                    Debug.WriteLine($"Menetrend törölve a {SelectedVehicle.GetName} járművön. Válassz új start állomást!");
                }
            });

        }

        public void Initialise()
        {
            GameModel.Initialise(GameSettings.DefaultWorldSize, new Random().Next());

            GameModel.WorldInstance.ChunkMatrix.ReadEach((x, y, c) =>
            {
                ChunkViewModel chunkViewModel = new(c);
                LoadedChunks.Add(chunkViewModel);
                ChunkMap[new(x, y)] = chunkViewModel;
            });

            GameModel.WorldInstance.ChunkChanged += On_UpdateChunk;

            Camera.Reset();

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
                        Debug.WriteLine($"Sikeres vétel! SelectedVehicle neve: {vvm.GetName}");
                    }
                });
            };

            GameModelInstance.GameOver += (s, e) =>
            {
                CompositionTarget.Rendering -= OnCompositionTargetRendering;

                MessageBox.Show("Csődbe mentél! A játéknak vége.");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ExitToMenuRequested?.Invoke(this, EventArgs.Empty);
                });
            };

            StartGameLoop();
        }

        #region BuildMode
        private BuildMode currentBuildMode = BuildMode.NONE;
        public BuildMode CurrentBuildMode
        {
            get => currentBuildMode;
            set
            {
                if (currentBuildMode == value) return;

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
       
        public bool IsBuildModeRoad => CurrentBuildMode == BuildMode.ROAD;
        public bool IsBuildModeStation => CurrentBuildMode == BuildMode.STATION;
        public bool IsBuildModeSelectStation => CurrentBuildMode == BuildMode.SELECT_STATION;
        public bool IsBuildModeBuyVehicle => CurrentBuildMode == BuildMode.BUY_VEHICLE;
        public bool IsBuildModeBridge => CurrentBuildMode == BuildMode.BRIDGE;
        public bool IsBuildModeLower => CurrentBuildMode == BuildMode.LOWER;
        public bool IsBuildModeHeighten => CurrentBuildMode == BuildMode.HEIGHTEN;

        private void OnSetBuildMode(BuildMode mode)
        {
            BuildMode previousMode = CurrentBuildMode;

            CurrentBuildMode = (CurrentBuildMode == mode)
                ? BuildMode.NONE
                : mode;

            if (previousMode == BuildMode.ROAD && CurrentBuildMode != BuildMode.ROAD)
            {
                Debug.WriteLine("Útépítés befejezve! Élek (Edges) újraépítése...");
                GameModel.WorldInstance.Roadnetwork.RebuildEdges();
            }

        }
        #endregion

        #region TimeScale
        private int currentTimeScale;
        public int CurrentTimeScale
        {
            get => currentTimeScale;
            set
            {
                if (currentTimeScale == value) 
                    return;

                currentTimeScale = value;

                OnPropertyChanged(nameof(IsTimeScale0));
                OnPropertyChanged(nameof(IsTimeScale1));
                OnPropertyChanged(nameof(IsTimeScale2));
                OnPropertyChanged(nameof(IsTimeScale4));
            }
        }

        public bool IsTimeScale0 => CurrentTimeScale == 0 || IsPausedView || GameModelInstance.IsPaused;
        public bool IsTimeScale1 => !IsPausedView && !GameModelInstance.IsPaused && CurrentTimeScale == 1;
        public bool IsTimeScale2 => !IsPausedView && !GameModelInstance.IsPaused && CurrentTimeScale == 2;
        public bool IsTimeScale4 => !IsPausedView && !GameModelInstance.IsPaused && CurrentTimeScale == 4;

        private void OnSetTimescale0X()
        {
            CurrentTimeScale = 0;
            GameModelInstance.Pause();
        }
        private void OnSetTimescale1X()
        {
            CurrentTimeScale = 1;
            GameModelInstance.UnPause();
            GameModelInstance.ChangeTimeSpeed1X();
        }
        private void OnSetTimescale2X()
        {
            CurrentTimeScale = 2;
            GameModelInstance.UnPause();
            GameModelInstance.ChangeTimeSpeed2X();
        }
        private void OnSetTimescale4X()
        {
            CurrentTimeScale = 4;
            GameModelInstance.UnPause();
            GameModelInstance.ChangeTimeSpeed4X();
        }
        #endregion

        #region GameLoop
        private readonly object _vehiclesLock = new();

        private Stopwatch? _stopwatch;
        private TimeSpan _lastRenderTime = TimeSpan.Zero;

        public void StartGameLoop()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            CompositionTarget.Rendering += OnCompositionTargetRendering;
        }
        private const double BASE_SPEED_MULTIPLIER = 10.0;
        private void OnCompositionTargetRendering(object? sender, EventArgs e)
        {
            TimeSpan currentRenderTime = _stopwatch!.Elapsed;
            double deltaTime = (currentRenderTime - _lastRenderTime).TotalSeconds;
            _lastRenderTime = currentRenderTime;

            if (CurrentTimeScale == 0)
            {
                _accumulator = 0;
            }

            if (deltaTime > 0.1) deltaTime = 0.1;

            _accumulator += deltaTime * CurrentTimeScale * BASE_SPEED_MULTIPLIER;

            while (_accumulator >= FIXED_DELTA_TIME)
            {
                GameModelInstance.Update(FIXED_DELTA_TIME);

                _accumulator -= FIXED_DELTA_TIME;
            }

            if (IsInspectorVisible && _hoveredField != null)
            {
                UpdateHoveredCoordinateAndTooltips(_lastMousePosition);
            }

            Camera.Update(deltaTime);

        }

        #endregion
    }
}
