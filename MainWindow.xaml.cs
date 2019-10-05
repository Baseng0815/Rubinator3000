using OpenTK;
using Rubinator3000.CubeScan;
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

namespace Rubinator3000 {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public static bool PositionEditingAllowed = false;

        private Queue<string> messages = new Queue<string>();
        private volatile Cube cube;

        private const int cameraCount = 4;
        private readonly Image[] cameraPreviews = new Image[cameraCount];
        private readonly WriteableBitmap[] previewBitmaps = new WriteableBitmap[cameraCount];
        private readonly WebCamControl[] webCamControls = new WebCamControl[cameraCount];
        private readonly Canvas[] canvases = new Canvas[cameraCount];

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

#if DEBUG
            Cube = new Cube(isRenderCube: true);
#else
            Cube = new Cube();
#endif
        }

        private void InitalizeCameraPreviews() {

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

                webCamControls[i] = new WebCamControl(i, ref canvases[i], ref previewBitmaps[i]);
            }

            // Load all positions, that were saved in "ReadPositions.xml"
            WebCamControl.LoadAllPositionsFromXml();
        }

        private void Cube_OnMoveDone(object sender, MoveEventArgs e) {

            if (sender is Cube c && c.Equals(cube)) {

            }
        }

        internal void LogStuff(string message) {

            if (textBoxLog != null)
                textBoxLog.Text += $"{message}\r\n";
            else
                messages.Enqueue(message);

            // Auto Scroll Implementation
            if (winFormsHost.Child != null) {
                textBoxLog.Focus();
                textBoxLog.CaretIndex = textBoxLog.Text.Length;
                textBoxLog.ScrollToEnd();
            }

        }

        private void TextBoxLog_Initialized(object sender, EventArgs e) {

            if (sender == textBoxLog) {

                while (messages.Count > 0) {

                    // Append message to textBoxLog-Control
                    textBoxLog.Text += $"{messages.Dequeue()}\r\n";
                }
            }
        }

        private void CameraPreview_MouseDown(object sender, MouseButtonEventArgs e) {

            // Manual Position Adding

            if (!PositionEditingAllowed || e.ChangedButton != MouseButton.Left || WebCamControl.TotalPositionCount == WebCamControl.MAXPOSITIONSTOREAD) {
                return;
            }

            Image clickedImage = (Image)sender;
            Point clickPosition = e.GetPosition(clickedImage);

            var colorDialog = new ColorDialog();

            int[] indicies;

            if (colorDialog.ShowDialog() == true) {

                /* indicies stores the indicies of the position to add
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
        }

        private void WinFormsHost_Initialized(object sender, EventArgs e) {

            winFormsHost.Child = CubeViewer.Window;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

            DrawCube.StopDrawing();

            for (int i = 0; i < webCamControls.Length; i++) {

                webCamControls[i].StopThread();
            }
            WebCamControl.SaveAllPositionsToXml();

            // Without this line, the program would throw an exception on close
            Environment.Exit(0);

            System.Windows.Application.Current.Shutdown();
        }

        private void CameraPreviewMenuItem_Click(object sender, RoutedEventArgs e) {

            Image cameraPreivew = (Image)(((System.Windows.Controls.ContextMenu)((System.Windows.Controls.MenuItem)sender).Parent).PlacementTarget);

            int cameraIndex = Array.IndexOf(cameraPreviews, cameraPreivew);
            webCamControls[cameraIndex].TryInitializeAndStart();
        }

        private void AllowPosEdit_Click(object sender, RoutedEventArgs e) {

            PositionEditingAllowed = allowPosEdit.IsChecked.Value;
        }
    }
}