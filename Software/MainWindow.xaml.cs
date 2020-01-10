//#define Camera

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
        private MoveSynchronizer moveSynchronizer;

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

            moveSynchronizer = new MoveSynchronizer(moveHistoryOutput);

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

        // @TODO: put logging loop into Log.cs
        internal void LoggingLoop() {
            while (logging) {
                while (messages.Count == 0) {
                    Thread.Sleep(20);
                    if (!logging)
                        return;
                };

                string message = messages.Dequeue();
                if (logging) {
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
        }
    }
}