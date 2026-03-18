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
        /*
        public DelegateCommand DecreaseTimeScaleCommand {get; private set; }
        public DelegateCommand IncreaseTimeScaleCommand {get; private set; }
        public DelegateCommand BuildRoadCommand {get; private set; }
        public DelegateCommand BuildBridgeCommand {get; private set; }
        public DelegateCommand BuildStationCommand {get; private set; }
        public DelegateCommand BuyVehicleCommand {get; private set; }
        public DelegateCommand SellVehicleCommand {get; private set; }
        public DelegateCommand LowerTerrainCommand {get; private set; }
        public DelegateCommand HeightenTerrainCommand { get; private set; }
        */
        #endregion

        #region FieldClicked & FieldHovered
        public DelegateCommand FieldClickedCommand { get; private set; }

        private void OnFieldClicked(Coordinate coord)
        {
            System.Diagnostics.Debug.WriteLine($"Field clicked at: {coord.X}, {coord.Y}");
            System.Diagnostics.Debug.WriteLine($"Chunks: {LoadedChunks.Count}");
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
            SetRoadModeCommand = new DelegateCommand(_ => OnSetRoadMode());
        }


        public void Initialise()
        {
            // VisualChunk/ CacheMode = new BitmapCache(); sor engedélyezése csak saját felelősségre!
            // Kikapcsolva nem zabálja meg a memóriát, cserébe kizoomolva durván laggol
            // 
            //GameModel.Initialise(1); // 48 MB
            //GameModel.Initialise(10); // 474 MB
            //GameModel.Initialise(20); // 1781 MB
            //GameModel.Initialise(40); // ~ 8 GB // itt lenne szép a generálás :(
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
                    OnPropertyChanged(nameof(IsRoadModeActive));
                }
            }
        }

        public bool IsRoadModeActive => CurrentBuildMode == BuildMode.ROAD;

        public DelegateCommand SetRoadModeCommand { get; private set; }

        private void OnSetRoadMode()
        {
            if (CurrentBuildMode == BuildMode.ROAD)
            {
                CurrentBuildMode = BuildMode.ROAD;
            }
            else
            {
                CurrentBuildMode = BuildMode.ROAD;
            }
        }



    }
}
