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

        private Queue<string> messages = new Queue<string>();
        private volatile Cube cube;

        private WriteableBitmap[] previewBitmaps = new WriteableBitmap[4];
        private WebCamControl[] webCamControls = new WebCamControl[4];
        public static Canvas[] canvases = new Canvas[4];

        public Cube Cube {
            get => cube;
            set {
                if (cube != null)
                    cube.OnMoveDone -= Cube_OnMoveDone;

                cube = value;
                value.OnMoveDone += Cube_OnMoveDone;
                DrawFlat.SetState(value);
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
            for (int i = 0; i < 4; i++) {
                previewBitmaps[i] = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
            }

            cameraPreview0.Source = previewBitmaps[0];
            cameraPreview1.Source = previewBitmaps[1];
            cameraPreview2.Source = previewBitmaps[2];
            cameraPreview3.Source = previewBitmaps[3];

            canvases[0] = cameraCanvas0;
            canvases[1] = cameraCanvas1;
            canvases[2] = cameraCanvas2;
            canvases[3] = cameraCanvas3;

            webCamControls[0] = new WebCamControl(0, ref canvases[0], ref previewBitmaps[0]);
            webCamControls[1] = new WebCamControl(1, ref canvases[1], ref previewBitmaps[1]);
            webCamControls[2] = new WebCamControl(2, ref canvases[2], ref previewBitmaps[2]);
            webCamControls[3] = new WebCamControl(3, ref canvases[3], ref previewBitmaps[3]);
            
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

            if (WebCamControl.PositionsToReadAt.Count == WebCamControl.MAXPOSITIONSTOREAD) {
                return;
            }

            bool? positionAddingAllowed = allowPosAdd.IsChecked;

            // If chbxPosAdd is not checked, then return
            if (positionAddingAllowed == null || positionAddingAllowed == false) {
                return;
            }

            Image clickedImage = (Image)sender;
            Point clickPosition = e.GetPosition(clickedImage);

            var colorDialog = new ColorDialog();

            int[] indicies;

            if (colorDialog.ShowDialog() == true) {
                indicies = colorDialog.Result;
            }
            else {
                return;
            }

            int cameraIndex = -1;

            if (clickedImage == cameraPreview0)
                cameraIndex = 0;

            else if (clickedImage == cameraPreview1)
                cameraIndex = 1;

            else if (clickedImage == cameraPreview2)
                cameraIndex = 2;

            else if (clickedImage == cameraPreview3)
                cameraIndex = 3;

            ReadPosition tempPos = new ReadPosition(
                    clickPosition.X / clickedImage.ActualWidth,
                    clickPosition.Y / clickedImage.ActualHeight,
                    indicies[0],
                    indicies[1],
                    indicies[2],
                    cameraIndex
                );

            WebCamControl.AddPosition(tempPos, cameraIndex);
        }

        private void WinFormsHost_Initialized(object sender, EventArgs e) {

            winFormsHost.Child = CubeViewer.Window;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

            DrawCube.StopDrawing();

            WebCamControl.SaveAllPositionsToXml();

            // Without this line, the program would throw an exception on close
            Environment.Exit(0);

            System.Windows.Application.Current.Shutdown();
        }

    }
}

