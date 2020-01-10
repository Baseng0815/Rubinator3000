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
                //foreach (Move move in moves) {


                //    DrawCube.AddMove(move);

                //    if (arduino == null)
                //        Log.LogMessage("Arduino not connected");
                //    else {                        
                //        arduino.SendMove(move);
                //    }


                //}

                for (int i = 0; i < moves.Count; i++) {
                    bool multiTurn = false;
                    CubeFace currentFace = moves[i].Face;
                    CubeFace nextFace = i < moves.Count - 1 ? moves[i + 1].Face : CubeFace.NONE;

                    if (Cube.IsOpponentFace(currentFace, nextFace)) {
                        // multi turn
                        if (confirmationNeeded && arduino != null) {
                            var result = MessageBox.Show("Next multi turn move: " + moves[i].ToString() + " " + moves[i + 1].ToString() + "\nContinue stepping?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Information, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                            if (result == MessageBoxResult.No)
                                confirmationNeeded = false;
                            else if (result == MessageBoxResult.Cancel)
                                return;
                        }

                        if (arduino != null)
                            arduino.SendMultiTurnMove(moves[i], moves[i + 1]);

                        multiTurn = true;
                    }
                    else {
                        // normal move
                        if (confirmationNeeded && arduino != null) {
                            var result = MessageBox.Show("Next move: " + moves[i].ToString() + "\nContinue stepping?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Information, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                            if (result == MessageBoxResult.No)
                                confirmationNeeded = false;
                            else if (result == MessageBoxResult.Cancel)
                                return;
                        }

                        if (arduino != null)
                            arduino.SendMove(moves[i]);
                    }

                    DrawCube.AddMove(moves[i]);
                    if (multiTurn)
                        DrawCube.AddMove(moves[i + 1]);

                    Application.Current.Dispatcher.Invoke(delegate {
                        ((MainWindow)Application.Current.MainWindow).cube.DoMove(moves[i]);
                        if (multiTurn)
                            ((MainWindow)Application.Current.MainWindow).cube.DoMove(moves[i + 1]);

                        if (moveHistory.Text.Length == 0)
                            moveHistory.AppendText(moves[i].ToString());
                        else
                            moveHistory.AppendText(", " + moves[i].ToString());

                        if(multiTurn)
                            moveHistory.AppendText(", " + moves[i + 1].ToString());
                    });

                    if (multiTurn)
                        i++;
                }
            });
        }
    }
}
