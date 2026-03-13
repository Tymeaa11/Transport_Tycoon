using System.Configuration;
using System.Data;
using System.Windows;
using VolcanicTransport.Model;
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
        private MainGameWindow? _mainGameWindow;

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _gameViewModel = new GameViewModel();
            _gameViewModel.Initialise();

            _mainGameWindow = new MainGameWindow();
            _mainGameWindow.DataContext = _gameViewModel;

            _mainGameWindow.RequestChunkData += _gameViewModel.On_RequestChunkData;

            _mainGameWindow.Show();


            _mainGameWindow.InitializeFirstChunk();
        }
    }

}