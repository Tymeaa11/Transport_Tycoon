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
        public int MinimapBorderSize => GameSettings.WorldSizeInFields + 20; //used to size minimap border
        public int MinimapSize => GameSettings.WorldSizeInFields; //used to size minimap
        public Camera Camera { get; }
        public CameraToMinimap MinimapSelector { get; }
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

            if (CurrentBuildMode == BuildMode.BRIDGE)
            {
                if (_bridgeStartCoord == null)
                {
                    _bridgeStartCoord = coord;
                    System.Diagnostics.Debug.WriteLine($"Híd 1. pontja lerakva: {coord}. Kattints legfeljebb {SelectedBridgeType.Length} mezővel arrébb a túlpartra!");
                }
                else
                {
                    bool success = GameModelInstance.PlaceBridge(_bridgeStartCoord.Value, coord, SelectedBridgeType);
                    System.Diagnostics.Debug.WriteLine(success ? "Híd felépítve!" : "Hibás hídelhelyezés! Ellenőrizd a partot, a magasságot és a hosszt.");

                    _bridgeStartCoord = null;
                }
                return;
            }

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
                case BuildMode.EDIT_GLOBAL_ROUTE:
                    if (field.Surface is Station clickedGlobalStation && SelectedSavedRoute != null)
                    {
                        SelectedSavedRoute.AddStop(clickedGlobalStation);
                        System.Diagnostics.Debug.WriteLine($"Állomás hozzáadva a {SelectedSavedRoute.Name} járathoz!");
                    }
                    break;

                default:
                    // Handle BuildMode.NONE or unhandled cases
                    break;
            }
        }

        public ObservableCollection<Route> SavedRoutes => GameModelInstance.SavedRoutes;

        private Route? _selectedSavedRoute;
        public Route? SelectedSavedRoute
        {
            get => _selectedSavedRoute;
            set { _selectedSavedRoute = value; OnPropertyChanged(); }
        }

        private string _newRouteName = "Új Járat 1";
        public string NewRouteName
        {
            get => _newRouteName;
            set { _newRouteName = value; OnPropertyChanged(); }
        }

        public DelegateCommand SaveCurrentRouteCommand { get; private set; }
        public DelegateCommand AssignSavedRouteCommand { get; private set; }

        private bool _isTimetablePanelVisible = false;
        public bool IsTimetablePanelVisible
        {
            get => _isTimetablePanelVisible;
            set { _isTimetablePanelVisible = value; OnPropertyChanged(); }
        }

        public DelegateCommand ToggleTimetablePanelCommand { get; private set; }
        public DelegateCommand CreateGlobalRouteCommand { get; private set; }
        public DelegateCommand EditGlobalRouteCommand { get; private set; }

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
        #endregion

        #region Minimap
        public void MinimapTeleport(Vector vector)
        {
            Debug.WriteLine($"Minimap clicked at: {vector.X}, {vector.Y}");
            Camera.Position = -vector * GameSettings.FieldSize * Camera.Scale;

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

        private bool _isBridgeMenuOpen;
        public bool IsBridgeMenuOpen
        {
            get => _isBridgeMenuOpen;
            set { _isBridgeMenuOpen = value; OnPropertyChanged(); }
        }

        public GameSettings.BridgeData[] AvailableBridges => GameSettings.BridgeTypes;

        private GameSettings.BridgeData _selectedBridgeType;
        public GameSettings.BridgeData SelectedBridgeType
        {
            get => _selectedBridgeType;
            set { _selectedBridgeType = value; OnPropertyChanged(); }
        }

        private Coordinate? _bridgeStartCoord = null;

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

        private bool _isVehiclePanelVisible = false;
        public bool IsVehiclePanelVisible
        {
            get => _isVehiclePanelVisible;
            set { _isVehiclePanelVisible = value; OnPropertyChanged(); }
        }

        public DelegateCommand ToggleVehiclePanelCommand { get; private set; }

        #endregion



        public GameViewModel()
        {
            Camera = new Camera();
            MinimapSelector = new CameraToMinimap(Camera);
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
            SetBuildModeBridgeCommand = new DelegateCommand(_ =>
            {
                OnSetBuildMode(BuildMode.BRIDGE);
                IsBridgeMenuOpen = IsBuildModeBridge;
                _bridgeStartCoord = null;
            });
            SetBuildModeLowerCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.LOWER));
            SetBuildModeHeightenCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.HEIGHTEN));
            SetTimeScale0Command = new DelegateCommand(_ => OnSetTimescale0X());
            SetTimeScale1Command = new DelegateCommand(_ => OnSetTimescale1X());
            SetTimeScale2Command = new DelegateCommand(_ => OnSetTimescale2X());
            SetTimeScale4Command = new DelegateCommand(_ => OnSetTimescale4X());
            BuyVehicleCommand = new DelegateCommand(_ =>
            {
                var nameDialog = new VehicleNameWindow(SavedRoutes) { Owner = Application.Current.MainWindow };

                if (nameDialog.ShowDialog() == true)
                {
                    string chosenName = nameDialog.VehicleName;
                    string? chosenType = nameDialog.SelectedType;
                    Route? chosenRoute = nameDialog.SelectedRoute;

                    if (chosenRoute == null || chosenRoute.Stops.Count < 2)
                    {
                        MessageBox.Show("Válaszd ki a kezdő járatot, amiben van legalább két megálló, hogy a jármű le tudjon spawnolni!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Station firstStation = chosenRoute.Stops[0];
                    Station? secondStation = chosenRoute.Stops.Count > 1 ? chosenRoute.Stops[1] : null;

                    Vehicle newVehicle = chosenType switch
                    {
                        "CargoTruck" => new CargoTruck(chosenName),
                        "TankerTruck" => new TankerTruck(chosenName),
                        "MiniBus" => new MiniBus(chosenName),
                        _ => new Bus(chosenName)
                    };

                    newVehicle.AssignNewRoute(chosenRoute);
                    newVehicle.CurrentStopIndex = secondStation != null ? 1 : 0;

                    List<Road> path = new List<Road>();
                    if (secondStation != null)
                    {
                        var nodes = GameModel.WorldInstance.Roadnetwork.NodeMap;
                        RoadNode? startNode = nodes.Values.FirstOrDefault(n => n.Coordinate == firstStation.Coordinate);
                        RoadNode? endNode = nodes.Values.FirstOrDefault(n => n.Coordinate == secondStation.Coordinate);

                        if (startNode != null && endNode != null)
                        {
                            path = Pathfinder.FindPath(startNode, endNode) ?? new List<Road>();
                        }
                    }

                    if (path.Count == 0 || path.First().Coordinate != firstStation.Coordinate)
                    {
                        path.Insert(0, firstStation);
                    }
                    if (secondStation != null && path.Last().Coordinate != secondStation.Coordinate)
                    {
                        path.Add(secondStation);
                    }

                    newVehicle.StartJourney(path, false, firstStation);
                    GameModelInstance.BuyVehicle(newVehicle);
                    System.Diagnostics.Debug.WriteLine($"Új busz sikeresen megvéve: {chosenName} ({chosenType}), Járat: {chosenRoute.Name}");
                }
            });
            TogglePauseCommand = new DelegateCommand(_ => IsPausedView = !IsPausedView);
            ToggleVehiclePanelCommand = new DelegateCommand(_ =>
            {
                IsVehiclePanelVisible = !IsVehiclePanelVisible;
            });
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
            SaveCurrentRouteCommand = new DelegateCommand(_ =>
            {
                if (SelectedVehicle != null && SelectedVehicle.GetVehicle.Route != null && SelectedVehicle.GetVehicle.Route.Stops.Count > 0)
                {
                    var currentRoute = SelectedVehicle.GetVehicle.Route;
                    currentRoute.Name = NewRouteName;

                    if (!SavedRoutes.Contains(currentRoute))
                    {
                        SavedRoutes.Add(currentRoute);
                        Debug.WriteLine($"[Járat] '{currentRoute.Name}' elmentve a globális listába!");
                    }

                    NewRouteName = $"Új Járat {SavedRoutes.Count + 1}";
                }
            });

            AssignSavedRouteCommand = new DelegateCommand(_ =>
            {
                if (SelectedVehicle != null && SelectedSavedRoute != null)
                {
                    if (SelectedSavedRoute.Stops.Count < 2)
                    {
                        MessageBox.Show("Egy menetrendnek legalább 2 megállót kell tartalmaznia!", "Érvénytelen menetrend", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    SelectedVehicle.GetVehicle.AssignNewRoute(SelectedSavedRoute);
                    Debug.WriteLine($"[Járat] A(z) '{SelectedVehicle.GetName}' megkapta a '{SelectedSavedRoute.Name}' menetrendet!");
                }
            });

            ToggleTimetablePanelCommand = new DelegateCommand(_ =>
            {
                IsTimetablePanelVisible = !IsTimetablePanelVisible;
                if (!IsTimetablePanelVisible && CurrentBuildMode == BuildMode.EDIT_GLOBAL_ROUTE)
                    CurrentBuildMode = BuildMode.NONE;
            });

            CreateGlobalRouteCommand = new DelegateCommand(_ =>
            {
                var newRoute = new Route { Name = NewRouteName };
                SavedRoutes.Add(newRoute);
                SelectedSavedRoute = newRoute;
                NewRouteName = $"Járat {SavedRoutes.Count + 1}";
            });

            EditGlobalRouteCommand = new DelegateCommand(_ =>
            {
                if (SelectedSavedRoute != null)
                {
                    CurrentBuildMode = BuildMode.EDIT_GLOBAL_ROUTE;
                    System.Diagnostics.Debug.WriteLine($"Szerkesztés indul: {SelectedSavedRoute.Name}. Kattints a megállókra!");
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

            SelectedBridgeType = GameSettings.BridgeTypes[0];

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
                        IsVehiclePanelVisible = true;
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
