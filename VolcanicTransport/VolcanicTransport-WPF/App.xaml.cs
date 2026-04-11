using System.Windows;
using VolcanicTransport_WPF.View;
using VolcanicTransport_WPF.ViewModel;

namespace VolcanicTransport_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private GameViewModel? _gameViewModel;
        private MainMenuViewModel? _mainMenuViewModel;
        private MainGameWindow? _mainGameWindow;
        private MainMenuWindow? _mainMenuWindow;

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            TextureAtlas.Initialize("Assets/Atlas.png");
            _mainMenuViewModel = new MainMenuViewModel();
            _mainMenuViewModel.NewGameRequested += MainMenuViewModel_StartNewGameRequested;
            _mainMenuViewModel.ExitRequested += MainMenuViewModel_ExitRequested;

            _mainMenuWindow = new MainMenuWindow
            {
                DataContext = _mainMenuViewModel
            };

            _mainMenuWindow.Show();
        }

        private void MainMenuViewModel_StartNewGameRequested(object? sender, EventArgs e)
        {
            _gameViewModel = new GameViewModel();
            _gameViewModel.Initialise();
            //_gameViewModel.ExitToMenuRequested += GameViewModel_ExitToMenuRequested;

            _mainGameWindow = new MainGameWindow
            {
                DataContext = _gameViewModel
            };

            _mainGameWindow.Show();
            _mainMenuWindow?.Close();

            _mainMenuWindow = null;
            _mainMenuViewModel = null;
        }

        private void GameViewModel_ExitToMenuRequested(object? sender, EventArgs e)
        {
            if (_gameViewModel != null)
                _gameViewModel.ExitToMenuRequested -= GameViewModel_ExitToMenuRequested;

            _mainMenuViewModel = new MainMenuViewModel();
            _mainMenuViewModel.NewGameRequested += MainMenuViewModel_StartNewGameRequested;
            _mainMenuWindow = new MainMenuWindow
            {
                DataContext = _mainMenuViewModel
            };

            _mainMenuWindow.Show();

            _mainGameWindow?.Close();
        }

        private void MainMenuViewModel_ExitRequested(object? sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

}