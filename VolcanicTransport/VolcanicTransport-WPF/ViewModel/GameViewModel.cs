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
        public DelegateCommand SetBuildModeRoadCommand { get; private set; }
        public DelegateCommand SetBuildModeStationCommand { get; private set; }
        public DelegateCommand SetBuildModeBridgeCommand { get; private set; }
        public DelegateCommand SetBuildModeBuldozeCommand { get; private set; }
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
        public bool IsBuildModeBuldoze => CurrentBuildMode == BuildMode.BULDOZE;
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
