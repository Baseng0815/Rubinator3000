using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Rubinator3000.Communication {
    // synchronizes sending moves to the arduino and redrawing the cube
    // wraps arduino
    public class MoveSynchronizer {
        private Arduino arduino = null;
        private TextBox moveHistory;

        public MoveSynchronizer(TextBox moveHistory) {
            this.moveHistory = moveHistory;
        }

        public void ConnectArduino(string portName) {
            if (arduino != null)
                arduino.Disconnect();
            arduino = new ArduinoUSB(portName);
            arduino.Connect();
        }

        public void DisconnectArduino() {
            arduino.Disconnect();
        }

        public Task RunAsync(Move move) {
            return Task.Run(delegate {                
                DrawCube.AddMove(move);

                if (arduino == null)
                    Log.LogMessage("Arduino not connected");
                else
                    arduino.SendMove(move);

                Application.Current.Dispatcher.Invoke(delegate {
                    ((MainWindow)Application.Current.MainWindow).cube.DoMove(move);

                    if (moveHistory.Text.Length == 0)
                        moveHistory.AppendText(move.ToString());
                    else
                        moveHistory.AppendText(", " + move.ToString());
                });
            });
        }

        public Task RunAsync(MoveCollection moves) {
            return Task.Run(delegate {
                bool confirmationNeeded = true;
                foreach (Move move in moves) {
                    if (confirmationNeeded) {
                        var result = MessageBox.Show("Next move: " + move.ToString() + "\nContinue stepping?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Information, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == MessageBoxResult.No)
                            confirmationNeeded = false;
                        else if (result == MessageBoxResult.Cancel)
                            return;
                    }

                    DrawCube.AddMove(move);

                    if (arduino == null)
                        Log.LogMessage("Arduino not connected");
                    else
                        arduino.SendMove(move);

                    Application.Current.Dispatcher.Invoke(delegate {
                        ((MainWindow)Application.Current.MainWindow).cube.DoMove(move);

                        if (moveHistory.Text.Length == 0)
                            moveHistory.AppendText(move.ToString());
                        else
                            moveHistory.AppendText(", " + move.ToString());
                    });
                }
            });
        }
    }
}
