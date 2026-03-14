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
            SetRoadModeCommand = new DelegateCommand(_ => OnSetRoadMode());
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
