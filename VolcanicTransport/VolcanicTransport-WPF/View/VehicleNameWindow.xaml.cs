using System.Windows;
using System.Windows.Controls;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport_WPF.View
{
    /// <summary>
    /// Interaction logic for VehicleNameWindow.xaml
    /// </summary>
    public partial class VehicleNameWindow : Window
    {
        public string VehicleName { get; private set; } = string.Empty;
        public string? SelectedType => (TypeComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

        public Route? SelectedRoute => RouteComboBox.SelectedItem as Route;

        public VehicleNameWindow(IEnumerable<Route> savedRoutes)
        {
            InitializeComponent();
            RouteComboBox.ItemsSource = savedRoutes;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Válassz típust és adj meg egy nevet!");
                return;
            }
            VehicleName = NameTextBox.Text;
            this.DialogResult = true;
        }

        public VehicleNameWindow()
        {
            InitializeComponent();
            NameTextBox.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
