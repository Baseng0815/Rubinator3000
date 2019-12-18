#define Camera

using Rubinator3000;
using OpenTK;
using Rubinator3000.CubeScan;
using Rubinator3000.Solving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Threading;
using Rubinator3000.Communication;

namespace Rubinator3000 {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public static bool PositionEditingAllowed = false;

        private Queue<string> messages = new Queue<string>();

        // cube only used for scanning and passing to solver
        // @TODO: make public
        public Cube cube = new Cube();
        private Arduino arduino;

        private const int cameraCount = 4;
        private readonly Image[] cameraPreviews = new Image[cameraCount];
        private readonly WriteableBitmap[] previewBitmaps = new WriteableBitmap[cameraCount];
        private readonly WebCamControl[] webCamControls = new WebCamControl[cameraCount];
        private readonly Canvas[] canvases = new Canvas[cameraCount];

        private ColorDialog colorDialog;
        private bool logging;
        private Thread logThread;

        public MainWindow() {
            InitializeComponent();

            InitalizeCameraPreviews();

            menuItemCOMPort.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
            WebCamControl.OnCubeScanned += WebCamControl_OnCubeScanned;

            KeyDown += MainWindow_KeyDown;

            // init Log
            Log.OnLogging += Log_OnLogging;
            logging = true;
            logThread = new Thread(new ThreadStart(LoggingLoop));
            logThread.Start();

#if DEBUG
            MenuItem debugMenu = new MenuItem() {
                Header = "Debug"
            };

            MenuItem ollDebug = new MenuItem() {
                Header = "Oll Debug"
            };
            ollDebug.Click += MenuItemOllDebug_Click;

            debugMenu.Items.Add(ollDebug);
            menu.Items.Add(debugMenu);
#endif
        }

        private void Log_OnLogging(LoggingEventArgs e) {
            messages.Enqueue(e.Message);
        }

        private void WebCamControl_OnCubeScanned(object sender, CubeScanEventArgs e) {
            
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            switch (e.Key) {
                case Key.L:
                    cube.DoMove(new Move(CubeFace.LEFT, isPrime: Keyboard.IsKeyDown(Key.LeftShift)));
                    break;
                case Key.U:
                    cube.DoMove(new Move(CubeFace.UP, isPrime: Keyboard.IsKeyDown(Key.LeftShift)));
                    break;
                case Key.F:
                    cube.DoMove(new Move(CubeFace.FRONT, isPrime: Keyboard.IsKeyDown(Key.LeftShift)));
                    break;
                case Key.D:
                    cube.DoMove(new Move(CubeFace.DOWN, isPrime: Keyboard.IsKeyDown(Key.LeftShift)));
                    break;
                case Key.R:
                    cube.DoMove(new Move(CubeFace.RIGHT, isPrime: Keyboard.IsKeyDown(Key.LeftShift)));
                    break;
                case Key.B:
                    cube.DoMove(new Move(CubeFace.BACK, isPrime: Keyboard.IsKeyDown(Key.LeftShift)));
                    break;
                case Key.S:
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        ShuffleCube();
                    else {
                        SolveCube();
                    }
                    break;
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


        // @TODO: put logging loop into Log.cs
        internal void LoggingLoop() {
            while (logging) {
                while (messages.Count == 0) {
                    Thread.Sleep(20);
                };

                string message = messages.Dequeue();
                Dispatcher.Invoke(() => {
                    if (textBoxLog != null)
                        textBoxLog.Text += $"{message}\r\n";

                    // Auto Scroll Implementation
                    if (winFormsHost.Child != null) {
                        textBoxLog.Focus();
                        textBoxLog.CaretIndex = textBoxLog.Text.Length;
                        textBoxLog.ScrollToEnd();
                    }
                });
            }
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

            MoveSynchronizer synchronizer = new MoveSynchronizer(solvingMoves, arduino, moveHistoryOutput);
            await Task.Run(synchronizer.Run);
        }

        private async void ShuffleCube() {
            Random rnd = new Random();

            MoveCollection shuffleMoves = cube.Shuffle(rnd.Next(5, 20));

            MoveSynchronizer synchronizer = new MoveSynchronizer(shuffleMoves, arduino, moveHistoryOutput);
            await Task.Run(synchronizer.Run);
        }
    }
}