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

        private static readonly List<Key> allowedFaceKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.Tab, Key.Back };
        private static readonly List<Key> allowedRowColKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.Tab, Key.Back };

        private int[] result = new int[3];
        public int[] Result {
            get => result;
            set {
                if (value.Length != 3) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                result = value;
                inputFaceIndex.Text = result[0].ToString();
                inputRowIndex.Text = result[1].ToString();
                inputColIndex.Text = result[2].ToString();
            }
        }
        public ReadPositionDialog() {
            InitializeComponent();

            //Make this window appear on top
            Topmost = true;
            inputFaceIndex.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

            if (inputFaceIndex.Text == "" || inputRowIndex.Text == "" || inputColIndex.Text == "") {

                return;
            }

            Result[0] = Convert.ToInt32(inputFaceIndex.Text);
            Result[1] = Convert.ToInt32(inputRowIndex.Text);
            Result[2] = Convert.ToInt32(inputColIndex.Text);

            DialogResult = true; // Marks that this dialog as finshed so it will be closed
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e) {

            TextBox senderTextBox = ((TextBox)sender);

            if (senderTextBox == inputFaceIndex) {

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
        }
    }
}
