using System.Windows;
using System.Windows.Input;
using VolcanicTransport.Model.Utils;
using VolcanicTransport_WPF.ViewModel;

namespace VolcanicTransport_WPF.View
{
    /// <summary>
    /// Interaction logic for MainGameWindow.xaml
    /// </summary>
    public partial class MainGameWindow : Window
    {

        public MainGameWindow()
        {
            InitializeComponent();

            MouseWheel += MainGameWindow_MouseWheel;
            MouseLeftButtonDown += MainGameWindow_MouseLeftButtonDown;
            MouseMove += MainGameWindow_MouseMove;
            KeyDown += MainGameWindow_KeyDown;

            SizeChanged += (s, e) =>
            {
                if (DataContext is GameViewModel vm)
                    vm.SetViewDimensions(ViewPort.ActualWidth, ViewPort.ActualHeight);
            };

        }

        private void MainGameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                (DataContext as GameViewModel)?.TogglePauseCommand.Execute(null);
            }
        }

        private void MainGameWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is GameViewModel vm)
                vm.Camera.Zoom(e.Delta);
        }

        private void MainGameWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is GameViewModel vm)
            {
                Coordinate fieldCoord = vm.Camera.ScreenToField((Vector)e.GetPosition(ViewPort));

                if (vm.FieldClickedCommand.CanExecute(fieldCoord))
                    vm.FieldClickedCommand.Execute(fieldCoord);
            }
        }

        private void MainGameWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is GameViewModel vm)
                vm.UpdateHoveredCoordinateAndTooltips(e.GetPosition(ViewPort));
        }
    }
}
