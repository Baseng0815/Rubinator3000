using RubinatorCore.Solving;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Rubinator3000;
using System.Linq;
using RubinatorCore;

namespace Rubinator3000 {
    partial class MainWindow {

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
                    case "HistoryClear":
                        TextBox_MoveHistoryOutput.Clear();
                        break;
                    default:
                        break;
                }
            }
        }

        // Hardware

        private void MenuItem_Connect_Click(object sender, RoutedEventArgs e) {
            ConnectArduino();
        }

        private void MenuItem_Disconnect_Click(object sender, RoutedEventArgs e) {
            moveSynchronizer.DisconnectArduino();
            Button_Connect.Content = "Connect";
        }

        private void MenuItem_BluetoothSetup_Click(object sender, RoutedEventArgs e) {
            moveSynchronizer.SetupBluetooth();
        }

        private void MenuItem_BluetoothUnsetup_Click(object sender, RoutedEventArgs e) {
            moveSynchronizer.UnsetupBluetooth();
        }

        private void MenuItem_ReinitializeCameras_Click(object sender, RoutedEventArgs e) {
            for (int i = 0; i < webCamControls.Length; i++) {
                webCamControls[i].TryInitializeAndStart();
            }
        }

        private void MenuItem_SaveCameraInput_Click(object sender, RoutedEventArgs e) {
            DateTime dt = DateTime.Now;

            if (!System.IO.Directory.Exists("Images"))
                System.IO.Directory.CreateDirectory("Images");

            for(int i = 0; i < webCamControls.Length; i++) {
                string filename = $"Images/{dt.ToString("yyyyMMddHHmmss")}_Camera{i}.bmp";
                webCamControls[i].SaveBitmap(filename);
            }
        }
    }
}