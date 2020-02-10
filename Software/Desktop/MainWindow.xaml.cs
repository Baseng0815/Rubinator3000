//#define Camera

using RubinatorCore;
using OpenTK;
using Rubinator3000.CubeScan;
using RubinatorCore.Solving;
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
using Application = System.Windows.Application;

namespace Rubinator3000 {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public Cube cube = new Cube();
        private MoveSynchronizer moveSynchronizer;

        private const int cameraCount = 4;
        private readonly Image[] cameraPreviews = new Image[cameraCount];
        private readonly WriteableBitmap[] previewBitmaps = new WriteableBitmap[cameraCount];
        private readonly WebCamControl[] webCamControls = new WebCamControl[cameraCount];
        private readonly Canvas[] canvases = new Canvas[cameraCount];

        private ReadPositionDialog readPositionDialog;
        public static CubeColorDialog cubeColorDialog;

        public static CancellationTokenSource ctSource = new CancellationTokenSource();

        public MainWindow() {
            InitializeComponent();

            InitalizeCameraPreviews();

            moveSynchronizer = new MoveSynchronizer(TextBox_MoveHistoryOutput);

            ComboBox_COMPort.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
            WebCamControl.OnCubeScanned += WebCamControl_OnCubeScanned;

            KeyDown += MainWindow_KeyDown;

            // init Log
            Log.LogCallback = (message) => {
                Application.Current.Dispatcher.Invoke(() => {
                    MainWindow window = (MainWindow)Application.Current.MainWindow;
                    if (window.TextBox_Log != null)
                        window.TextBox_Log.Text += $"{message}\r\n";

                    // Auto Scroll Implementation
                    if (window.WindowsFormsHost_CubePreview.Child != null) {
                        window.TextBox_Log.Focus();
                        window.TextBox_Log.CaretIndex = window.TextBox_Log.Text.Length;
                        //window.TextBox_Log.ScrollToEnd();
                    }
                });
            };

            Log.LogMessage("Init Log");
        }
    }
}