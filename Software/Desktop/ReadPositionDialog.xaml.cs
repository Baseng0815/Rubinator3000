using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rubinator3000 {
    /// <summary>
    /// Interaction logic for ReadPositionDialog.xaml
    /// </summary>
    public partial class ReadPositionDialog : Window {

        private static readonly List<Key> allowedFaceKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.Tab, Key.Back, Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5 };
        private static readonly List<Key> allowedRowColKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.Tab, Key.Back, Key.NumPad0, Key.NumPad1, Key.NumPad2 };

        public int[] Result { get; private set; } = new int[3];

        public ReadPositionDialog() {

            InitializeComponent();

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;

            for (int i = 0; i < Result.Length; i++) {

                Result[i] = -1;
            }

            //Make this window appear on top
            Topmost = true;
            TextBox_InputFaceIndex.Focus();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e) {

            TextBox senderTextBox = ((TextBox)sender);

            if (senderTextBox == TextBox_InputFaceIndex) {

                if (!allowedFaceKeys.Contains(e.Key)) {

                    e.Handled = true;
                    return;
                }
            }
            else {

                if (!allowedRowColKeys.Contains(e.Key)) {

                    e.Handled = true;
                    return;
                }
            }

            if (e.Key != Key.Tab && e.Key != Key.Back) {

                senderTextBox.Text = "";
            }
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e) {

            if (TextBox_InputFaceIndex.Text == "" || TextBox_InputRowIndex.Text == "" || TextBox_InputColIndex.Text == "") {

                return;
            }

            Result[0] = Convert.ToInt32(TextBox_InputFaceIndex.Text);
            Result[1] = Convert.ToInt32(TextBox_InputRowIndex.Text);
            Result[2] = Convert.ToInt32(TextBox_InputColIndex.Text);

            if (((MainWindow)Application.Current.MainWindow).cubeScanner.ReadPositionByIndices(Result[0], Result[1], Result[2]) != null) {

                if (MessageBox.Show("Do you want to replace the current position", "Position exists already", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { // If yes, replace position

                    return;
                }
            }

            DialogResult = true; // Marks that this dialog has finshed successfully
            // Dialog will close
        }

        private void Button_Discard_Click(object sender, RoutedEventArgs e) {

            DialogResult = false; // Marks that this dialog has finished not successfully
            // Dialog will close
        }
    }
}
