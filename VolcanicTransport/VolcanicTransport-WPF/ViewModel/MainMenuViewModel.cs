namespace VolcanicTransport_WPF.ViewModel
{
    public class MainMenuViewModel : ViewModelBase
    {
        #region Commands
        public DelegateCommand NewGameCommand { get; }
        public DelegateCommand LoadGameCommand { get; }
        public DelegateCommand ExitCommand { get; }
        #endregion

        #region Events
        public event EventHandler? NewGameRequested;
        public event EventHandler? ExitRequested;
        #endregion

        public MainMenuViewModel()
        {
            NewGameCommand = new DelegateCommand(_ => OnNewGame());
            LoadGameCommand = new DelegateCommand(_ => OnLoadGame());
            ExitCommand = new DelegateCommand(_ => OnExit());
        }

        private void OnNewGame()
        {
            NewGameRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoadGame()
        {
            // Majd ide jön a betöltés logikája
        }

        private void OnExit()
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
