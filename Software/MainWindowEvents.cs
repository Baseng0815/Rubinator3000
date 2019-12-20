using Rubinator3000.CubeScan;
using Rubinator3000.Solving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Rubinator3000 {
    public partial class MainWindow {
        private void WinFormsHost_Initialized(object sender, EventArgs e) {
            winFormsHost.Child = CubeViewer.Window;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
#if Camera
            DrawCube.StopDrawing();

            for (int i = 0; i < webCamControls.Length; i++) {

                webCamControls[i].StopThread();
            }
            WebCamControl.SaveAllPositionsToXml();

            // Without this line, the program would throw an exception on close
            Environment.Exit(0);

            System.Windows.Application.Current.Shutdown();
#endif
            logging = false;
            logThread.Join();
        }

        private void CameraPreviewMenuItem_Click(object sender, RoutedEventArgs e) {
#if Camera
            Image cameraPreivew = (Image)(((System.Windows.Controls.ContextMenu)((System.Windows.Controls.MenuItem)sender).Parent).PlacementTarget);

            int cameraIndex = Array.IndexOf(cameraPreviews, cameraPreivew);
            webCamControls[cameraIndex].TryInitializeAndStart();
#endif
        }
        
        private void AllowPosEdit_Click(object sender, RoutedEventArgs e) {

            PositionEditingAllowed = allowPosEdit.IsChecked.Value;
        }

        private async void SolveCube() {
            CubeSolver solver = new CubeSolverFridrich(cube);

            solver.SolveCube();

            MoveCollection solvingMoves = solver.SolvingMoves;

            await moveSynchronizer.RunAsync(solvingMoves);
        }

        private async void ShuffleCube() {
            Random rnd = new Random();

            MoveCollection shuffleMoves = cube.Shuffle(rnd.Next(5, 20));

            await moveSynchronizer.RunAsync(shuffleMoves);
        }

        private void Log_OnLogging(LoggingEventArgs e) {
            messages.Enqueue(e.Message);
        }

        private void WebCamControl_OnCubeScanned(object sender, CubeScanEventArgs e) {
            
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
                cube.DoMove(move);
                DrawCube.AddMove(move);
            }
        }

        private void InitalizeCameraPreviews() {
#if Camera
            const int width = 640;
            const int height = 480;

            // Link the image-controls to cameraPreviews-array
            cameraPreviews[0] = cameraPreview0;
            cameraPreviews[1] = cameraPreview1;
            cameraPreviews[2] = cameraPreview2;
            cameraPreviews[3] = cameraPreview3;

            // Initialize previewBitmaps
            for (int i = 0; i < cameraCount; i++) {
                previewBitmaps[i] = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
            }

            // Link the previewBitmaps to the according image-controls
            for (int i = 0; i < cameraCount; i++) {

                cameraPreviews[i].Source = previewBitmaps[i];
            }

            // Link the canvas-controls to canvases array;
            canvases[0] = cameraCanvas0;
            canvases[1] = cameraCanvas1;
            canvases[2] = cameraCanvas2;
            canvases[3] = cameraCanvas3;

            // Initialize all webcam-controls
            for (int i = 0; i < cameraCount; i++) {
                webCamControls[i] = new WebCamControl(i, /*ref*/ canvases[i], ref previewBitmaps[i]);
            }

            // Load all positions, that were saved in "ReadPositions.xml"
            WebCamControl.LoadAllPositionsFromXml();
#endif
        }

        private void CameraPreview_MouseDown(object sender, MouseButtonEventArgs e) {
#if Camera
            // Manual Position Adding

            if (!PositionEditingAllowed || e.ChangedButton != MouseButton.Left || WebCamControl.TotalPositionCount == WebCamControl.MAXPOSITIONSTOREAD) {
                return;
            }

            Image clickedImage = (Image)sender;
            Point clickPosition = e.GetPosition(clickedImage);

            colorDialog = new ColorDialog();

            int[] indicies;

            if (colorDialog.ShowDialog() // Waits until dialog gets closed
                == true) {

                /* "indicies" stores the indicies of the position to add
                 * [0] faceIndex
                 * [1] rowIndex
                 * [2] colIndex
                 */
                indicies = colorDialog.Result;
            }
            else {
                return;
            }

            // Determine, which cameraPreview was clicked
            int cameraIndex = Array.IndexOf(cameraPreviews, clickedImage);

            ReadPosition tempPos = new ReadPosition(
                    clickPosition.X / clickedImage.ActualWidth, // calculate relativeX
                    clickPosition.Y / clickedImage.ActualHeight, // calculate relativeY
                    indicies[0], // faceIndex
                    indicies[1], // rowIndex
                    indicies[2], // colIndex
                    cameraIndex
                );

            Log.LogMessage(WebCamControl.AddPosition(tempPos, cameraIndex));
#endif
        }
    }
}
