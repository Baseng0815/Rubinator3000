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

namespace Rubinator3000 {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public static bool PositionEditingAllowed = false;

        private Queue<string> messages = new Queue<string>();
        public volatile Cube cube;
        private MoveCollection moves = new MoveCollection();

        private const int cameraCount = 4;
        private readonly Image[] cameraPreviews = new Image[cameraCount];
        private readonly WriteableBitmap[] previewBitmaps = new WriteableBitmap[cameraCount];
        private readonly WebCamControl[] webCamControls = new WebCamControl[cameraCount];
        private readonly Canvas[] canvases = new Canvas[cameraCount];
        private CubeColor[,] scanData = new CubeColor[6, 9];

        private ColorDialog colorDialog;
        private bool logging;
        private Thread logThread;
        private Arduino arduino;

        public Cube Cube {
            get => cube;
            set {
                if (cube != null)
                    cube.OnMoveDone -= Cube_OnMoveDone;

                cube = value;
                value.OnMoveDone += Cube_OnMoveDone;
                DrawCube.AddMove(value);
            }
        }

        public MainWindow() {                                    
            InitializeComponent();                          

            InitalizeCameraPreviews();

            menuItemCOMPort.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
            WebCamControl.OnCubeScanned += WebCamControl_OnCubeScanned;            

            KeyDown += MainWindow_KeyDown;

            Cube = new Cube(isRenderCube: true);

            // init Log
            Log.OnLogging += Log_OnLogging;
            logging = true;
            logThread = new Thread(new ThreadStart(LogStuff));
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
                    cube.DoMove(CubeFace.LEFT, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? -1 : 1);
                    break;
                case Key.U:
                    cube.DoMove(CubeFace.UP, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? -1 : 1);
                    break;
                case Key.F:
                    cube.DoMove(CubeFace.FRONT, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? -1 : 1);
                    break;
                case Key.D:
                    cube.DoMove(CubeFace.DOWN, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? -1 : 1);
                    break;
                case Key.R:
                    cube.DoMove(CubeFace.RIGHT, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? -1 : 1);
                    break;
                case Key.B:
                    cube.DoMove(CubeFace.BACK, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? -1 : 1);
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

        private void Cube_OnMoveDone(object sender, MoveEventArgs e) {
            moves.Add(e.Move);
        }

        internal void LogStuff() {
            while (logging) {
                while (messages.Count == 0) ;
                    
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

            Log.LogStuff(WebCamControl.AddPosition(tempPos, cameraIndex));
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

            await solver.SolveCubeAsync();

            moveHistoryOutput.Clear();
            moveHistoryOutput.Text = string.Join(", ", solver.SolvingMoves.Select(m => m.ToString()));
            cube.DoMoves(solver.SolvingMoves);

            if (arduino != null)
                arduino.SendMoves(solver.SolvingMoves);
        }

        private void ShuffleCube() {
            
            Random rnd = new Random();

            Cube.Shuffle(rnd.Next(5, 20));
        }        
    }
}