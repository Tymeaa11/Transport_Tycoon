using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VolcanicTransport_WPF.View
{
    /// <summary>
    /// Interaction logic for VehicleNameWindow.xaml
    /// </summary>
    public partial class VehicleNameWindow : Window
    {
        public string VehicleName { get; private set; } = string.Empty;
        public string? SelectedType => (TypeComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

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
