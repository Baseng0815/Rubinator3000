using OpenTK;
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
        private bool _3dView;
        private Queue<string> messages = new Queue<string>();

        public Cube Cube { get; set; }
        public bool View3D { get => _3dView; }

        public MainWindow() {
            InitializeComponent();            

            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e) {
            DrawCube.StopDrawing();

            System.Windows.Application.Current.Shutdown();
        }

        private void WindowsFormsHost_Initialized(object sender, EventArgs e) {
            (sender as WindowsFormsHost).Child = CubeViewer.Window;
        }

        internal void LogStuff(string message) {
            if (textBoxLog != null)
                textBoxLog.Text += $"{message}\r\n";
            else
                messages.Enqueue(message);
        }

        private void TextBoxLog_Initialized(object sender, EventArgs e) {
            if(sender is System.Windows.Controls.TextBox textBox) {
                while (messages.Count > 0)
                    textBox.Text += $"{messages.Dequeue()}\r\n";
            }
        }
    }
}
