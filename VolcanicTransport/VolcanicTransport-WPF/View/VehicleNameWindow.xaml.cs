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

        public VehicleNameWindow()
        {
            InitializeComponent();
            NameTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            VehicleName = NameTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
