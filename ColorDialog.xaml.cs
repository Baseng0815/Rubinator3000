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
    /// Interaction logic for ColorDialog.xaml
    /// </summary>
    public partial class ColorDialog : Window {

        public int[] Result { get; set; } = new int[3];
        public ColorDialog() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

            Result[0] = Convert.ToInt32(inputFaceIndex.Text);
            Result[1] = Convert.ToInt32(inputRowIndex.Text);
            Result[2] = Convert.ToInt32(inputColIndex.Text);
            DialogResult = true;
        }
    }
}
