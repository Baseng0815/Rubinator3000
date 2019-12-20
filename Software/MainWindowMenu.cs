using Rubinator3000.DebugWindows;
using Rubinator3000.Solving;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Rubinator3000;
using System.Linq;

namespace Rubinator3000 {
    partial class MainWindow {
        // File


        private void MenuItemClose_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void MenuItemSuffle_Click(object sender, RoutedEventArgs e) {
            ShuffleCube();
        }

        private void MenuItemResetCube_Click(object sender, RoutedEventArgs e) {
            cube = new Cube();
        }

        private void MenuItemSolveCube_Click(object sender, RoutedEventArgs e) {
            SolveCube();
        }

        private void MenuItemOllDebug_Click(object sender, RoutedEventArgs e) {
            OllDebugWindow ollDebug = new OllDebugWindow(cube);

            ollDebug.Show();
        }

        private void MenuItemPllDebug_Click(object sender, RoutedEventArgs e) {

        }

        // View
        private void MenuItemChangeView_Click(object sender, RoutedEventArgs e) {
            if (sender is MenuItem menuItem) {
                switch (menuItem.Tag as string) {
                    case "2D":
                        DrawCube.DisplayMode = CubeDisplayMode.FLAT;
                        CubeViewer.Window.Invalidate();
                        CubeViewer.DetachInputEvents();
                        break;
                    case "3D":
                        DrawCube.DisplayMode = CubeDisplayMode.CUBE;
                        CubeViewer.Window.Invalidate();
                        CubeViewer.AttachInputEvents();
                        break;
                    default:
                        break;
                }
            }
        }

        private void MenuItemConnect_Click(object sender, RoutedEventArgs e) {
            string serialPort = menuItemCOMPort.Text;
            if (!System.IO.Ports.SerialPort.GetPortNames().Contains(serialPort)) {
                MessageBox.Show("Bitte einen gültigen Port auswählen!");
            }

            moveSynchronizer.SetArduino(serialPort);
        }
    }
}