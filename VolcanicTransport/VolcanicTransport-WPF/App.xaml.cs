using Microsoft.Win32;
using System.Windows;
using VolcanicTransport.Model;
using VolcanicTransport.Model.Exceptions;
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

        private readonly OpenFileDialog _openFileDialog;
        private readonly SaveFileDialog _saveFileDialog;

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);

            _openFileDialog = new OpenFileDialog
            {
                Title = "Load Vulcanic Transport save",
                Filter = "Vulcanic Transport Save files (.vts)|*.vts",
                RestoreDirectory = true
            };

            _saveFileDialog = new SaveFileDialog
            {
                Title = "Save Vulcanic Transport game",
                Filter = "Vulcanic Transport Save files (.vts)|*.vts",
                RestoreDirectory = true
            };

        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            TextureAtlas.Initialize("Assets/Atlas.png");
            _mainMenuViewModel = new MainMenuViewModel();
            _mainMenuViewModel.NewGameRequested += MainMenuViewModel_StartNewGameRequested;
            _mainMenuViewModel.ExitRequested += MainMenuViewModel_ExitRequested;
            _mainMenuViewModel.LoadGameRequested += MainMenuViewModel_LoadGameRequested;

            _mainMenuWindow = new MainMenuWindow
            {
                DataContext = _mainMenuViewModel
            };

            _mainMenuWindow.Show();
        }

        private void MainMenuViewModel_StartNewGameRequested(object? sender, EventArgs e)
        {
            _gameViewModel = new GameViewModel();
            _gameViewModel.ExitToMenuRequested += GameViewModel_ExitToMenuRequested;
            _gameViewModel.SaveGameRequested += GameViewModel_SaveGame;
            _gameViewModel.LoadGameRequested += GameViewModel_LoadGame;

            _gameViewModel.InitialiseNewGame();

            _mainGameWindow = new MainGameWindow
            {
                DataContext = _gameViewModel
            };

            _mainGameWindow.Show();
            _mainMenuWindow?.Close();

            _mainMenuWindow = null;
            _mainMenuViewModel = null;
        }

        private void MainMenuViewModel_LoadGameRequested(object? sender, EventArgs e)
        {
            _gameViewModel = new GameViewModel();
            _gameViewModel.ExitToMenuRequested += GameViewModel_ExitToMenuRequested;
            _gameViewModel.SaveGameRequested += GameViewModel_SaveGame;
            _gameViewModel.LoadGameRequested += GameViewModel_LoadGame;

            GameViewModel_LoadGame(null, EventArgs.Empty);

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

        private void GameViewModel_LoadGame(object? sender, EventArgs e)
        {
            if (_gameViewModel == null) return;

            if (_openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _gameViewModel.InitialiseLodedGame(_openFileDialog.FileName);
                }
                catch (LoadingException ex)
                {
                    MessageBox.Show($"An error occured while trying to load {_openFileDialog.FileName}: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
        }


        private void GameViewModel_SaveGame(object? sender, EventArgs e)
        {
            if (_gameViewModel == null) return;

            if (_saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _gameViewModel.SaveGame(_saveFileDialog.FileName);
                }
                catch (SavingException ex)
                {
                    MessageBox.Show("An error occured while trying to save the game: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void MainMenuViewModel_ExitRequested(object? sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

}