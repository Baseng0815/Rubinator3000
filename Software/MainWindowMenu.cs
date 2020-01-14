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


        private void MenuItem_Close_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void MenuItem_Suffle_Click(object sender, RoutedEventArgs e) {
            ShuffleCube();
        }

        private void MenuItem_ResetCube_Click(object sender, RoutedEventArgs e) {
            cube = new Cube();

            DrawCube.AddState(cube);
        }

        private void MenuItem_SolveCube_Click(object sender, RoutedEventArgs e) {
            SolveCube();
        }

        private void MenuItem_OllDebug_Click(object sender, RoutedEventArgs e) {
            OllDebugWindow ollDebug = new OllDebugWindow();

            ollDebug.Show();
        }

        private void MenuItem_PllDebug_Click(object sender, RoutedEventArgs e) {
            PllDebugWindow pllDebug = new PllDebugWindow();

            pllDebug.Show();
        }

        // View
        private void MenuItem_ChangeView_Click(object sender, RoutedEventArgs e) {
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

        // Hardware

        private void MenuItem_Connect_Click(object sender, RoutedEventArgs e) {
            string serialPort = ComboBox_COMPort.Text;
            if (!System.IO.Ports.SerialPort.GetPortNames().Contains(serialPort)) {
                MessageBox.Show("Bitte einen gültigen Port auswählen!");
            }

            moveSynchronizer.ConnectArduino(serialPort);
        }

        private void MenuItem_Disconnect_Click(object sender, RoutedEventArgs e) {
            moveSynchronizer.DisconnectArduino();
        }
    }
}