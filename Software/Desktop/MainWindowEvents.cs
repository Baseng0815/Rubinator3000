
using Rubinator3000.CubeScan;
using RubinatorCore;
using RubinatorCore.Solving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rubinator3000 {
    public partial class MainWindow {
        private void WinFormsHost_Initialized(object sender, EventArgs e) {
            WindowsFormsHost_CubePreview.Child = CubeViewer.Window;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Log.LogMessage("Shutting down..");

            moveSynchronizer.DisconnectArduino();
            ctSource.Cancel();

            // Without this line, the program would throw an exception on close
            Environment.Exit(0);

            Log.StopLogging();

            Application.Current.Shutdown();
        }

        private void MenuItem_CameraPreview_Click(object sender, RoutedEventArgs e) {

            // TODO Swap cameraPreviews per drag and drop
        }

        public async void SolveCube() {
            CubeSolver solver = new CubeSolverFridrich(cube);

            solver.SolveCube();

            MoveCollection solvingMoves = solver.SolvingMoves;
            TextBox_MoveHistoryOutput.Clear();

            await moveSynchronizer.RunAsync(solvingMoves);

            //moveSynchronizer.SetSolvedState(true);
        }

        public async void ShuffleCube() {
            Random rnd = new Random();

            TextBox_MoveHistoryOutput.Clear();
            MoveCollection shuffleMoves = cube.GetShuffleMoves(rnd.Next(10, 20));

            await moveSynchronizer.RunAsync(shuffleMoves);

        }

        public void ConnectArduino() {
            string serialPort = ComboBox_COMPort.Text;
            if (!System.IO.Ports.SerialPort.GetPortNames().Contains(serialPort)) {
                MessageBox.Show("Bitte einen gültigen Port auswählen!");
            }
            else {
                moveSynchronizer.ConnectArduino(serialPort);

                moveSynchronizer.SetArduinoLEDs(ArduinoLEDs.ALL, Convert.ToByte(Math.Round(Slider_LEDBrightness.Value * 255)));

                Button_Connect.Content = "Disconnect";
            }
        }

        private void WebCamControl_OnCubeScanned(object sender, CubeScanEventArgs e) {

            try {
                cube = new Cube(e.ScanData);
                DrawCube.AddState(cube);
            }
            catch {
                Log.LogMessage("Cube not created");
            }
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            int direction = Keyboard.IsKeyDown(Key.LeftShift) ? -1 : 1;
            Move move = null;

            switch (e.Key) {
                case Key.L:
                    move = new Move(CubeFace.LEFT, direction);
                    break;
                case Key.U:
                    move = new Move(CubeFace.UP, direction);
                    break;
                case Key.F:
                    move = new Move(CubeFace.FRONT, direction);
                    break;
                case Key.D:
                    move = new Move(CubeFace.DOWN, direction);
                    break;
                case Key.R:
                    move = new Move(CubeFace.RIGHT, direction);
                    break;
                case Key.B:
                    move = new Move(CubeFace.BACK, direction);
                    break;
                case Key.S:
                    if (Keyboard.IsKeyDown(Key.LeftShift))
                        ShuffleCube();
                    else {
                        SolveCube();
                    }
                    break;
            }

            if (move != null) {
                moveSynchronizer.RunAsync(move);
            }
        }

        // cube position editing        
        private int[] lastIndices = new int[3] { 6, 3, 3 };

        private void InitalizeCameraPreviews() {

            const int width = 640;
            const int height = 480;

            List<Image> previewImages = new List<Image>() {
                Image_CameraPreview0,
                Image_CameraPreview1,
                Image_CameraPreview2,
                Image_CameraPreview3
            };
            List<Canvas> previewCanvases = new List<Canvas>() {
                Canvas_CameraPreview0,
                Canvas_CameraPreview1,
                Canvas_CameraPreview2,
                Canvas_CameraPreview3
            };

            cubeScanner = new CubeScanner(previewImages, previewCanvases);
        }

        private void Image_CameraPreview_MouseDown(object sender, MouseButtonEventArgs e) {

            Image clickedImage = (Image)sender;
            Point clickPosition = e.GetPosition(clickedImage);

        }

        private void CheckBox_MultiTurn_Click(object sender, RoutedEventArgs e) {

            Settings.UseMultiTurn = CheckBox_MultiTurn.IsChecked.Value;
        }

        private void CheckBox_ClearForcedColors_Click(object sender, RoutedEventArgs e) {

            ColorIdentification.ClearForcedColors();
        }

        private void CheckBox_AutoReadout_Click(object sender, RoutedEventArgs e) {

            if (CheckBox_AutoReadout.IsChecked.Value) {

                Settings.ReadoutRequested = ReadUtility.ReadoutRequested.AUTO_READOUT;
            }
            else {

                Settings.ReadoutRequested = ReadUtility.ReadoutRequested.DISABLED;
            }
        }

        private void Button_ManualReadout_Click(object sender, RoutedEventArgs e) {

            if (Settings.ReadoutRequested != ReadUtility.ReadoutRequested.AUTO_READOUT) {
                Settings.ReadoutRequested = ReadUtility.ReadoutRequested.SINGLE_READOUT;
            }
        }

        private void Image_CameraPreview_SizeChanged(object sender, SizeChangedEventArgs e) {

            cubeScanner.RedrawAllCanvasElements();
        }

        private void Button_LEDControl_Clicked(object sender, RoutedEventArgs e) {
            moveSynchronizer.SetArduinoLEDs(ArduinoLEDs.ALL, Convert.ToByte(Math.Round(Slider_LEDBrightness.Value * 255)));
        }

        private void Slider_LEDBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (Button_LEDControl != null)
                Button_LEDControl.Content = $"{(int)Math.Floor(e.NewValue * 100)}%";
        }

        private void ComboBox_COMPort_DropDownOpened(object sender, EventArgs e) {
            ComboBox_COMPort.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
        }

        private void Button_Connect_Clicked(object sender, RoutedEventArgs e) {
            if (!moveSynchronizer.ArduinoConnected) {
                ConnectArduino();
                Button_Connect.Content = "Disconnect";
            }
            else {
                moveSynchronizer.DisconnectArduino();
                Button_Connect.Content = "Connect";
            }

        }
    }
}
