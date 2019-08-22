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
        private GLControl glView;
        private bool _3dView;

        public Cube Cube { get; set; }
        public bool View3D { get => _3dView; }

        public MainWindow() {
            InitializeComponent();
        }

        private void WindowsFormsHost_Initialized(object sender, EventArgs e) {
            glView = new GLControl();
            OpenTK.Toolkit.Init();
            glView.CreateControl();

            // init glView
            glView.Dock = DockStyle.Fill;
            glView.Paint += GlView_Paint;
            glView.Resize += GlView_Resize;
            

            (sender as WindowsFormsHost).Child = glView;
        }

        private void GlView_Resize(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void GlView_Paint(object sender, PaintEventArgs e) {
            throw new NotImplementedException();
        }
    }
}
