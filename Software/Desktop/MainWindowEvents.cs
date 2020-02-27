using Rubinator3000.Communication;
using Rubinator3000.CubeScan;
using Rubinator3000.CubeScan.CameraControl;
using Rubinator3000.CubeScan.ColorIdentification;
using Rubinator3000.CubeScan.RelativeElements;
using Rubinator3000.CubeView;
using Rubinator3000.XmlHandling;
using RubinatorCore;
using RubinatorCore.CubeRepresentation;
using RubinatorCore.Solving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Rubinator3000 {
    public partial class MainWindow {

        private CameraPreview MouseDownCp;

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

        private void Button_LEDControl_Clicked(object sender, RoutedEventArgs e) {
            moveSynchronizer.SetArduinoLEDs(ArduinoLEDs.ALL, Convert.ToByte(Math.Round(Slider_LEDBrightness.Value * 255)));
        }

        private void Button_ManualReadout_Click(object sender, RoutedEventArgs e) {

            cubeScanner.ReadCube();
            /*
            // If Readout should be asynchronous
            if (Settings.ReadoutRequested != ReadUtility.ReadoutRequested.AUTO_READOUT) {
                Settings.ReadoutRequested = ReadUtility.ReadoutRequested.SINGLE_READOUT;
            }
            */
        }

        private void CheckBox_AutoReadout_Click(object sender, RoutedEventArgs e) {

            if (CheckBox_AutoReadout.IsChecked.Value) {

                Settings.ReadoutRequested = ReadUtility.ReadoutRequested.AUTO_READOUT;
            }
            else {

                Settings.ReadoutRequested = ReadUtility.ReadoutRequested.DISABLED;
            }
        }

        private void CheckBox_ClearForcedColors_Click(object sender, RoutedEventArgs e) {

            ColorID.ClearForcedColors();
        }

        private void CheckBox_MultiTurn_Click(object sender, RoutedEventArgs e) {

            Settings.UseMultiTurn = CheckBox_MultiTurn.IsChecked.Value;
        }

        private void ComboBox_COMPort_DropDownOpened(object sender, EventArgs e) {
            ComboBox_COMPort.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
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

        private void CubeScanner_OnTileFound(object sender, TileFoundEventArgs e) {

            if (cubeScanner.AllPositionsAssigned()) { // If all tiles are being scanned

                return;
            }

            Application.Current.Dispatcher.Invoke(() => {

                cubeScanner.PreviewHandler.HighlightContour(e.Contour);

                Canvas canvas = cubeScanner.PreviewHandler.CameraPreviews[e.CameraIndex].Canvas;
                Point point = canvas.TransformToAncestor(this).Transform(new Point(0, 0));

                Point dest = new Point(Left + point.X + (e.Contour.RelativeCenterX * canvas.ActualWidth), Top + point.Y + (e.Contour.RelativeCenterY * canvas.ActualHeight));

                readPositionDialog = new ReadPositionDialog {
                    Top = dest.Y - Settings.OffsetToTile - 180,
                    Left = dest.X - Settings.OffsetToTile - 250
                };

                readPositionDialog.Owner = this;

                if (readPositionDialog.ShowDialog() == true) { // If Position Adding was confirmed

                    int[] r = readPositionDialog.Result;
                    cubeScanner.AddReadPosition(new ReadPosition(
                        faceIndex: r[0],
                        rowIndex: r[1],
                        colIndex: r[2],
                        cameraIndex: e.CameraIndex,
                        contour: e.Contour,
                        circle: new RelativeCircle(
                            new RelativePosition(
                                relativeX: e.Contour.RelativeCenterX,
                                relativeY: e.Contour.RelativeCenterY),
                                radius: Settings.PositionRadius,
                                color: System.Windows.Media.Colors.Black
                             )
                        )
                    );
                }
                else { // If Position Adding was Discarded

                    // Do nothing
                }

                cubeScanner.PreviewHandler.ClearHighlightedTiles();
            });
        }

        private void CubeScanner_OnCubeScanned(object sender, CubeScanEventArgs e) {

            try {
                cube = new Cube(e.ScanData);
                DrawCube.AddState(cube);
            }
            catch {
                Log.LogMessage("Cube not created");
            }
        }

        private void Image_CameraPreview_SizeChanged(object sender, SizeChangedEventArgs e) {

            cubeScanner.PreviewHandler.RedrawAllCanvasElements();
        }

        private void InitializeCubeScanner() {

            List<(Image Image, Canvas Canvas)> outputs = new List<(Image Image, Canvas Canvas)> {

                (Image_CameraPreview0, Canvas_CameraPreview0),
                (Image_CameraPreview1, Canvas_CameraPreview1),
                (Image_CameraPreview2, Canvas_CameraPreview2),
                (Image_CameraPreview3, Canvas_CameraPreview3),
            };

            cubeScanner = new CubeScanner(outputs);
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

        public async void ShuffleCube() {
            Random rnd = new Random();

            TextBox_MoveHistoryOutput.Clear();
            MoveCollection shuffleMoves = cube.GetShuffleMoves(rnd.Next(10, 20));

            await moveSynchronizer.RunAsync(shuffleMoves);

        }

        private void Slider_LEDBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (Button_LEDControl != null)
                Button_LEDControl.Content = $"{(int)Math.Floor(e.NewValue * 100)}%";
        }

        public async void SolveCube() {
            CubeSolver solver = new CubeSolverFridrich(cube);

            solver.SolveCube();

            MoveCollection solvingMoves = solver.SolvingMoves;
            TextBox_MoveHistoryOutput.Clear();

            await moveSynchronizer.RunAsync(solvingMoves);

            //moveSynchronizer.SetSolvedState(true);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Log.LogMessage("Shutting down..");

            moveSynchronizer.DisconnectArduino();
            ctSource.Cancel();

            XmlHandler.SaveReadPositions(cubeScanner.ReadPositions);

            // Without this line, the program would throw an exception on close
            Environment.Exit(0);

            Log.StopLogging();

            Application.Current.Shutdown();
        }

        private void Canvas_CameraPreview_MouseDown(object sender, MouseButtonEventArgs e) {

            Canvas clickedCanvas = (Canvas)sender;
            MouseDownCp = cubeScanner.PreviewHandler.CameraPreviews.Where(o => o.Canvas == clickedCanvas).FirstOrDefault();
        }

        private void Canvas_CameraPreview_MouseUp(object sender, MouseButtonEventArgs e) {

            if (MouseDownCp != null) {

                Canvas clickedCanvas = (Canvas)sender;
                CameraPreview MouseUpCp = cubeScanner.PreviewHandler.CameraPreviews.Where(o => o.Canvas == clickedCanvas).FirstOrDefault();
                cubeScanner.PreviewHandler.SwitchPreviews(MouseDownCp, MouseUpCp);
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e) {

            MouseDownCp = null;
        }

        private void WinFormsHost_Initialized(object sender, EventArgs e) {
            WindowsFormsHost_CubePreview.Child = CubeViewer.Window;
        }
    }
}
