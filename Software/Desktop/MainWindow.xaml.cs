using Rubinator3000.Communication;
using Rubinator3000.CubeScan;
using RubinatorCore;
using RubinatorCore.CubeRepresentation;
using System.Threading;
using System.Windows;

namespace Rubinator3000 {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public Cube cube = new Cube();
        public MoveSynchronizer moveSynchronizer;

        public CubeScanner cubeScanner;

        private ReadPositionDialog readPositionDialog;
        public static CubeColorDialog cubeColorDialog;

        public static CancellationTokenSource ctSource = new CancellationTokenSource();

        public MainWindow() {
            InitializeComponent();

            InitalizeCameraPreviews();

            moveSynchronizer = new MoveSynchronizer(TextBox_MoveHistoryOutput);

            ComboBox_COMPort.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
            CubeScanner.OnCubeScanned += WebCamControl_OnCubeScanned;

            KeyDown += MainWindow_KeyDown;

            // init Log
            Log.LogCallback = (message) => {
                Application.Current.Dispatcher.Invoke(() => {
                    MainWindow window = (MainWindow)Application.Current.MainWindow;
                    if (window.TextBox_Log != null)
                        window.TextBox_Log.Text += $"{message}\r\n";

                    // Auto Scroll Implementation
                    if (window.WindowsFormsHost_CubePreview.Child != null) {
                        window.TextBox_Log.CaretIndex = window.TextBox_Log.Text.Length;
                        window.TextBox_Log.ScrollToEnd();
                    }
                });
            };

            Log.LogMessage("Init Log");
        }
    }
}