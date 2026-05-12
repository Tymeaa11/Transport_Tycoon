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
            KeyUp += MainGameWindow_KeyUp;

            SizeChanged += (s, e) =>
            {
                if (DataContext is GameViewModel vm)
                    vm.SetViewDimensions(ViewPort.ActualWidth, ViewPort.ActualHeight);
            };

        }

        private void MainGameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is GameViewModel vm)
            {
                UpdateCameraInput(vm.Camera, e.Key, true);

                if (e.Key == Key.Escape)
                    vm.TogglePauseCommand.Execute(null); //
            }
        }

        private void MainGameWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is GameViewModel vm)
                vm.Camera.Zoom(e.Delta);
        }

        private void MainGameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (DataContext is GameViewModel vm)
            {
                UpdateCameraInput(vm.Camera, e.Key, false);
            }
        }
        private void UpdateCameraInput(Camera camera, Key key, bool isPressed)
        {
            switch (key)
            {
                case Key.W: camera.IsMovingUp = isPressed; break;
                case Key.S: camera.IsMovingDown = isPressed; break;
                case Key.A: camera.IsMovingLeft = isPressed; break;
                case Key.D: camera.IsMovingRight = isPressed; break;
            }
        }

        private void MainGameWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is GameViewModel vm)
            {
                if (!MinimapBounds.IsMouseOver)
                {
                    Coordinate fieldCoord = vm.Camera.ScreenToField((Vector)e.GetPosition(ViewPort));

                    if (vm.FieldClickedCommand.CanExecute(fieldCoord))
                        vm.FieldClickedCommand.Execute(fieldCoord);
                }
                else
                {
                    vm.MinimapTeleport((Vector)e.GetPosition(MinimapBounds));
                }


            }
        }

        private void MainGameWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is GameViewModel vm)
                vm.UpdateHoveredCoordinateAndTooltips((Vector)e.GetPosition(ViewPort));
        }
    }
}
