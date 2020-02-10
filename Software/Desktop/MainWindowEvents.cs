
using Rubinator3000.CubeScan;
using RubinatorCore;
using RubinatorCore.Solving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            ctSource.Cancel();

            for (int i = 0; i < webCamControls.Length; i++) {

                webCamControls[i].StopThread();
            }
            WebCamControl.SaveAllPositionsToXml();

            // Without this line, the program would throw an exception on close
            Environment.Exit(0);

            Application.Current.Shutdown();


            Log.StopLogging();

            Application.Current.Shutdown();
        }

        private void MenuItem_CameraPreview_Click(object sender, RoutedEventArgs e) {

            Image cameraPreivew = (Image)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget;

            int cameraIndex = Array.IndexOf(cameraPreviews, cameraPreivew);
            webCamControls[cameraIndex].TryInitializeAndStart();
        }

        private void CheckBox_AllowPosEdit_Click(object sender, RoutedEventArgs e) {

            Settings.PositionEditingAllowed = CheckBox_AllowPosEdit.IsChecked.Value;
        }

        public async void SolveCube() {
            CubeSolver solver = new CubeSolverFridrich(cube);

            solver.SolveCube();

            MoveCollection solvingMoves = solver.SolvingMoves;
            TextBox_MoveHistoryOutput.Clear();

            await moveSynchronizer.RunAsync(solvingMoves);
        }

        public async void ShuffleCube() {
            Random rnd = new Random();

            TextBox_MoveHistoryOutput.Clear();
            MoveCollection shuffleMoves = cube.GetShuffleMoves(rnd.Next(5, 10));

            await moveSynchronizer.RunAsync(shuffleMoves);
        }

        private void WebCamControl_OnCubeScanned(object sender, CubeScanEventArgs e) {
            //TODO: add code
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

        private void InitalizeCameraPreviews() {

            const int width = 640;
            const int height = 480;

            // Link the image-controls to cameraPreviews-array
            cameraPreviews[0] = Image_CameraPreview0;
            cameraPreviews[1] = Image_CameraPreview1;
            cameraPreviews[2] = Image_CameraPreview2;
            cameraPreviews[3] = Image_CameraPreview3;

            // Initialize previewBitmaps
            for (int i = 0; i < cameraCount; i++) {
                previewBitmaps[i] = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
            }

            // Link the previewBitmaps to the according image-controls
            for (int i = 0; i < cameraCount; i++) {

                cameraPreviews[i].Source = previewBitmaps[i];
            }

            // Link the canvas-controls to canvases array;
            canvases[0] = Canvas_CameraPreview0;
            canvases[1] = Canvas_CameraPreview1;
            canvases[2] = Canvas_CameraPreview2;
            canvases[3] = Canvas_CameraPreview3;

            // Initialize all webcam-controls
            for (int i = 0; i < cameraCount; i++) {
                webCamControls[i] = new WebCamControl(i, canvases[i], ref previewBitmaps[i]);
            }

            // Load all positions, that were saved in "ReadPositions.xml"
            WebCamControl.LoadAllPositionsFromXml();
        }

        private void Image_CameraPreview_MouseDown(object sender, MouseButtonEventArgs e) {

            Image clickedImage = (Image)sender;
            Point clickPosition = e.GetPosition(clickedImage);

            // Calculate relativeX and relativeY from clickPosition
            double relativeX = clickPosition.X / clickedImage.ActualWidth;
            double relativeY = clickPosition.Y / clickedImage.ActualHeight;

            // Determine, which cameraPreview was clicked
            int cameraIndex = Array.IndexOf(cameraPreviews, clickedImage);

            // Calibrate Reference Colors
            if (Settings.CalibrateColors && e.ChangedButton == MouseButton.Left) {

                cubeColorDialog = new CubeColorDialog();
                if (cubeColorDialog.ShowDialog() == true) {

                    CubeColor resultColor = cubeColorDialog.Result;
                    if (resultColor != null && resultColor != CubeColor.NONE) {

                        ColorIdentification.ChangeReferenceColor(resultColor, webCamControls[cameraIndex].ReadColorAtPosition(relativeX, relativeY));
                    }
                }
            }

            // Manual Position Adding
            if (Settings.PositionEditingAllowed && e.ChangedButton == MouseButton.Left && WebCamControl.TotalPositionCount < WebCamControl.MAXPOSITIONSTOREAD) {

                readPositionDialog = new ReadPositionDialog();

                int[] indicies;

                if (readPositionDialog.ShowDialog() // Waits until dialog gets closed
                    == true) {

                    /* "indicies" stores the indicies of the position to add
                     * [0] faceIndex
                     * [1] rowIndex
                     * [2] colIndex
                     */
                    indicies = readPositionDialog.Result;
                }
                else {
                    return;
                }

                ReadPosition tempPos = new ReadPosition(
                        clickPosition.X / clickedImage.ActualWidth, // calculate relativeX
                        clickPosition.Y / clickedImage.ActualHeight, // calculate relativeY
                        indicies[0], // faceIndex
                        indicies[1], // rowIndex
                        indicies[2], // colIndex
                        cameraIndex
                    );

                Log.LogMessage(WebCamControl.AddPosition(tempPos, cameraIndex));

                return;
            }
        }

        private void CheckBox_MultiTurn_Click(object sender, RoutedEventArgs e) {

            Settings.UseMultiTurn = CheckBox_MultiTurn.IsChecked.Value;
        }

        private void CheckBox_CalRefColors_Click(object sender, RoutedEventArgs e) {

            Settings.CalibrateColors = CheckBox_CalRefColors.IsChecked.Value;
        }

        private void CheckBox_UseRefColors_Click(object sender, RoutedEventArgs e) {

            Settings.UseReferenceColors = CheckBox_UseRefColors.IsChecked.Value;
        }

        private void CheckBox_AutoReadout_Click(object sender, RoutedEventArgs e) {

            if (CheckBox_AutoReadout.IsChecked.Value) {

                WebCamControl.CubeGenerationRequested = ReadUtility.ReadoutRequested.AUTO_READOUT;
            }
            else {

                WebCamControl.CubeGenerationRequested = ReadUtility.ReadoutRequested.DISABLED;
            }
        }

        private void Button_ManualReadout_Click(object sender, RoutedEventArgs e) {

            WebCamControl.CubeGenerationRequested = ReadUtility.ReadoutRequested.SINGLE_READOUT;
        }

        private void Image_CameraPreview_SizeChanged(object sender, SizeChangedEventArgs e) {

            RedrawAllCircles();
        }

        public void RedrawAllCircles() {

            for (int i = 0; i < canvases.Length; i++) {
                canvases[i].Children.Clear();
            }

            List<ReadPosition> allPositions = WebCamControl.AllReadPositions();

            foreach (ReadPosition readPosition in allPositions) {

                WebCamControl.DrawCircleAtPosition(readPosition, canvases[readPosition.CameraIndex]);
            }

            for (int i = 0; i < canvases.Length; i++) {

                canvases[i].InvalidateVisual();
            }
        }
    }
}
