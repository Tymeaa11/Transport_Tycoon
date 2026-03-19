using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        public static GameModel GameModelInstance { get => GameModel.Instance; }

        public ObservableCollection<Chunk> LoadedChunks { get; } = [];

        private double _lastWidth;
        private double _lastHeight;

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
            int chunkPX = Chunk.ChunkSize * Field.FieldSize;

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
                    var chunk = GameModelInstance.WorldInstance.GetChunk(coord);
                    if (chunk != null) LoadedChunks.Add(chunk);
                }
        }

        public Coordinate WorldSizeInChunks => GameModelInstance.WorldInstance.SizeInChunks;
        public int TileSize => Field.FieldSize;

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
        #endregion

        #region FieldClicked & FieldHovered
        public DelegateCommand FieldClickedCommand { get; private set; }

        private void OnFieldClicked(Coordinate coord)
        {
            System.Diagnostics.Debug.WriteLine($"Field clicked at: {coord.X}, {coord.Y}");
            System.Diagnostics.Debug.WriteLine($"Chunks: {LoadedChunks.Count}");

            Field? f = GameModelInstance.WorldInstance.GetField(coord);

            if (f != null && f.IsBuildable()) 
            {
                f.Surface = new Mushroom(coord, MushroomGrowthStage.FULLY_GROWN);
                var chunkCoord = GameModelInstance.WorldInstance.GetChunkCoordinate(coord);
                GameModelInstance.WorldInstance.GetChunk(chunkCoord)?.TriggerRerender();

            }
        }

        private Coordinate _hoveredCoordinate;
        public Coordinate HoveredCoordinate
        {
            get => _hoveredCoordinate;
            set
            {
                _hoveredCoordinate = value;
                OnPropertyChanged();
                UpdateBuildability();
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
            IsHoveredFieldBuildable = GameModelInstance.IsBuildable(HoveredCoordinate);
        }
        #endregion


        public GameViewModel()
        {
            Camera = new Camera(Matrix.Identity);

            Camera.CameraChanged += (s, e) => UpdateVisibleChunks(_lastWidth, _lastHeight);

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

            //GameModelInstance.moneyChanged += GameModelInstance_moneyChanged;
        }

        private void GameModelInstance_moneyChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentMoney));
        }

        public void Initialise()
        {
            GameModel.Initialise(8);
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
                if (currentTimescale != value)
                {
                    currentTimescale = value;
                    OnPropertyChanged(nameof(IsTimescale0));
                    OnPropertyChanged(nameof(IsTimescale1));
                    OnPropertyChanged(nameof(IsTimescale2));
                    OnPropertyChanged(nameof(IsTimescale4));
                }

            }
        }
        public bool IsBuildModeRoad => CurrentBuildMode == BuildMode.ROAD;
        public bool IsBuildModeStation => CurrentBuildMode == BuildMode.STATION;
        public bool IsBuildModeBridge => CurrentBuildMode == BuildMode.BRIDGE;
        public bool IsBuildModeLower => CurrentBuildMode == BuildMode.LOWER;
        public bool IsBuildModeHeighten => CurrentBuildMode == BuildMode.HEIGHTEN;

        public bool IsTimescale0 => CurrentTimescale == 0;
        public bool IsTimescale1 => CurrentTimescale == 1;
        public bool IsTimescale2 => CurrentTimescale == 2;
        public bool IsTimescale4 => CurrentTimescale == 4;

        public string CurrentMoney
        {
            get => 1000.ToString() + "€$"; // TODO: get from model
        }
        private void OnSetBuildMode(BuildMode mode)
        {
            if (CurrentBuildMode == mode)
            {
                CurrentBuildMode = BuildMode.NONE;
            }
            else
            {
                CurrentBuildMode = mode;
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
            GameModelInstance.ChangeTimeSpeed1X();
        }

        private void OnSetTimescale2X()
        {
            CurrentTimescale = 2;
            GameModelInstance.ChangeTimeSpeed2X();
        }

        private void OnSetTimescale4X()
        {
            CurrentTimescale = 4;
            GameModelInstance.ChangeTimeSpeed4X();
        }



    }
}
