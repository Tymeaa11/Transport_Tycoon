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

            this.MouseWheel += MainGameWindow_MouseWheel;
            this.MouseLeftButtonDown += MainGameWindow_MouseLeftButtonDown;
            this.MouseMove += MainGameWindow_MouseMove;

            this.SizeChanged += (s, e) =>
            {
                if (DataContext is GameViewModel vm)
                    vm.SetViewDimensions(ViewPort.ActualWidth, ViewPort.ActualHeight);
            };

        }

        private void MainGameWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is GameViewModel vm)
            {
                // Zoom around the current mouse position
                vm.Camera.Zoom(e.Delta, e.GetPosition(ViewPort));
            }
        }

        private void MainGameWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is GameViewModel vm)
            {
                Coordinate fieldCoord = vm.Camera.ScreenToField(e.GetPosition(ViewPort));

                if (vm.FieldClickedCommand.CanExecute(fieldCoord))
                    vm.FieldClickedCommand.Execute(fieldCoord);
            }
        }

        private void MainGameWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is GameViewModel vm)
                vm.HoveredCoordinate = vm.Camera.ScreenToField(e.GetPosition(ViewPort));
        }
    }
}
