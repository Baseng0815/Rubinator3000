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

        public MainWindow() {
            InitializeComponent();

            InitalizeCameraPreviews();

            moveSynchronizer = new MoveSynchronizer(TextBox_MoveHistoryOutput);

            ComboBox_COMPort.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
            WebCamControl.OnCubeScanned += WebCamControl_OnCubeScanned;

            KeyDown += MainWindow_KeyDown;

            // init Log
            Log.LogMessage("Init Log");

#if DEBUG
            MenuItem debugMenu = new MenuItem() {
                Header = "Debug"
            };

            MenuItem ollDebug = new MenuItem() {
                Header = "OLL Debug"
            };
            ollDebug.Click += MenuItem_OllDebug_Click;
            debugMenu.Items.Add(ollDebug);

            MenuItem pllDebug = new MenuItem() {
                Header = "PLL Debug"
            };
            pllDebug.Click += MenuItem_PllDebug_Click;
            debugMenu.Items.Add(pllDebug);

            Menu_MenuBar.Items.Add(debugMenu);
#endif
        }        
    }
}