using RubinatorCore;
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

namespace Rubinator3000 {
    /// <summary>
    /// Interaction logic for CubeColorDialog.xaml
    /// </summary>
    public partial class CubeColorDialog : Window {

        public CubeColor Result { get; set; } = CubeColor.NONE;

        public CubeColorDialog() {

            InitializeComponent();
            // Make this window appear on top
            Topmost = true;

            ComboBox_CubeColorSelection.ItemsSource = Enum.GetNames(typeof(CubeColor));

            ComboBox_CubeColorSelection.SelectedItem = null;
            ComboBox_CubeColorSelection.Focus();
        }

        private void Button_FinishDialog_Click(object sender, RoutedEventArgs e) {

            if (ComboBox_CubeColorSelection != null) {

                Result = (CubeColor)Enum.Parse(typeof(CubeColor), ComboBox_CubeColorSelection.SelectedItem.ToString());
                DialogResult = true; // Marks that this dialog as finshed so it will be closed
            }
        }
    }
}
