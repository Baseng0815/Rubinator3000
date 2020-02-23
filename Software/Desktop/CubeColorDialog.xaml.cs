using RubinatorCore.CubeRepresentation;
using System;
using System.Windows;

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
