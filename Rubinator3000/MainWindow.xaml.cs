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
        private Queue<string> messages = new Queue<string>();
        private Cube cube;
        bool undoMode, redoMode;
        private Stack<Move> undoneMoves = new Stack<Move>();

        public Stack<Move> MoveHistory;

        public Cube Cube {
            get => cube;
            set {
                if (cube != null)
                    cube.OnMoveDone -= Cube_OnMoveDone;

                MoveHistory.Clear();
                cube = value;
                value.OnMoveDone += Cube_OnMoveDone;
                DrawCube.SetState(value);
                DrawFlat.SetState(value);
            }
        }

        public MainWindow() {
            InitializeComponent();
            MoveHistory = new Stack<Move>();

            Cube = new Cube();
        }

        private void Cube_OnMoveDone(object sender, MoveEventArgs e) {
            if (sender is Cube c && c.Equals(cube)) {
                if (undoMode) {
                    undoneMoves.Push(e.Move);
                }
                else {
                    if (!redoMode)
                        undoneMoves.Clear();

                    MoveHistory.Push(e.Move);
                }

                textBoxMoves.Clear();

                textBoxMoves.Text = string.Join("\r\n", MoveHistory.Reverse().Select(m => m.ToString()));
            }
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
            if (sender is System.Windows.Controls.TextBox textBox) {
                while (messages.Count > 0)
                    textBox.Text += $"{messages.Dequeue()}\r\n";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            DrawCube.StopDrawing();

            System.Windows.Application.Current.Shutdown();
        }
    }
}
