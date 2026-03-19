using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport_WPF.View;

namespace VolcanicTransport_WPF.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        public GameModel GameModelInstance { get => GameModel.Instance; }

        public ObservableCollection<Chunk> LoadedChunks { get; } = new ObservableCollection<Chunk>();

        public void On_RequestChunkData(object? sender, RequestChunkDataEventArgs e)
        {
            var chunk = GameModelInstance.WorldInstance.GetChunk(e.Coordinate);

            if (chunk != null && !LoadedChunks.Contains(chunk))
            {
                LoadedChunks.Add(chunk);
            }
        }

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

            FieldClickedCommand = new DelegateCommand(param =>
            {
                if (param is Coordinate coord)
                    OnFieldClicked(coord);
            });
            SetBuildModeRoadCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.ROAD));
            SetBuildModeStationCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.STATION));
            SetBuildModeBridgeCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.BRIDGE));
            SetBuildModeBuldozeCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.BULDOZE));
            SetBuildModeLowerCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.LOWER));
            SetBuildModeHeightenCommand = new DelegateCommand(_ => OnSetBuildMode(BuildMode.HEIGHTEN));
        }


        public void Initialise()
        {
            GameModel.Initialise();
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
                    OnPropertyChanged(nameof(IsBuildModeBuldoze));
                    OnPropertyChanged(nameof(IsBuildModeLower));
                    OnPropertyChanged(nameof(IsBuildModeHeighten));
                }
            }
        }
        public bool IsBuildModeRoad => CurrentBuildMode == BuildMode.ROAD;
        public bool IsBuildModeStation => CurrentBuildMode == BuildMode.STATION;
        public bool IsBuildModeBridge => CurrentBuildMode == BuildMode.BRIDGE;
        public bool IsBuildModeBuldoze => CurrentBuildMode == BuildMode.BULDOZE;
        public bool IsBuildModeLower => CurrentBuildMode == BuildMode.LOWER;
        public bool IsBuildModeHeighten => CurrentBuildMode == BuildMode.HEIGHTEN;

        public DelegateCommand SetBuildModeRoadCommand { get; private set; }
        public DelegateCommand SetBuildModeStationCommand { get; private set; }
        public DelegateCommand SetBuildModeBridgeCommand { get; private set; }
        public DelegateCommand SetBuildModeBuldozeCommand { get; private set; }
        public DelegateCommand SetBuildModeLowerCommand { get; private set; }
        public DelegateCommand SetBuildModeHeightenCommand { get; private set; }
        public DelegateCommand DecreaseTimescaleCommand { get; private set; }
        public DelegateCommand IncreaseTimescaleCommand { get; private set; }
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

        private void OnIncreaseTimescale()
        {
            
        }



    }
}
