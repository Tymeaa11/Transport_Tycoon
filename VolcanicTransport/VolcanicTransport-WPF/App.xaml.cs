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
        private MainGameWindow? _mainGameWindow;

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            TextureAtlas.Initialize("Assets/Atlas.png");
            _gameViewModel = new();
            _gameViewModel.Initialise();

            _mainGameWindow = new()
            {
                DataContext = _gameViewModel
            };

            _mainGameWindow.Show();
        }
    }

}