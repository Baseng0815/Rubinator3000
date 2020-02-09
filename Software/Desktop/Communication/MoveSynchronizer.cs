using RubinatorCore;
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
    // wraps arduino and bluetooth server
    public class MoveSynchronizer {
        private Arduino arduino = null;
        private readonly TextBox moveHistory;
        private BluetoothServer bluetoothServer;

        private Cube receivingState = new Cube();
        private int tilesReceived = 54;

        private void HandleBluetoothData(byte data) {
            // handle incoming state data
            if (tilesReceived < 54) {
                try {
                    CubeFace face = (CubeFace)(tilesReceived / 9);
                    int tile = tilesReceived % 9;
                    receivingState.SetTile(face, tile, (CubeColor)data);

                    tilesReceived++;
                    if (tilesReceived == 54) {
                        Application.Current.Dispatcher.Invoke(delegate {
                            ((MainWindow)Application.Current.MainWindow).cube = (Cube)receivingState.Clone();
                        });
                        DrawCube.AddState(receivingState);
                    }
                } catch (Exception e) {
                    Log.LogMessage(e.ToString());
                }
            // do move
            } else if (data > 0x01 && data < 0x0E)
                RunAsync(RubinatorCore.Utility.ByteToMove(data), false);

            // start receiving state from client
            else if (data == 0x01) {
                tilesReceived = 0;
            // send state to client
            } else if (data == 0x00) {
                for (CubeFace face = CubeFace.LEFT; face <= CubeFace.BACK; face++) {
                    for (int tile = 0; tile < 9; tile++) {
                        Application.Current.Dispatcher.Invoke(delegate {
                            bluetoothServer.Write((byte)((MainWindow)Application.Current.MainWindow).cube.At(face, tile));
                        });
                    }
                }
            }
        }

        private void SendBluetoothMove(Move move) {
            bluetoothServer.Write(RubinatorCore.Utility.MoveToByte(move));
        }

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
            if (arduino != null)
                arduino.Disconnect();
        }

        public void SetupBluetooth() {
            bluetoothServer = new BluetoothServer();

            bluetoothServer.DataReceived += (obj, data) => {
                HandleBluetoothData(data);
                Log.LogMessage("Bluetooth data received: " + data);
            };

            bluetoothServer.StartDiscovering();
        }

        public void UnsetupBluetooth() {
            bluetoothServer.Disconnect();
        }

        public Task RunAsync(Move move, bool btSend = true) {
            return Task.Run(delegate {
                DrawCube.AddMove(move);
                if (bluetoothServer != null && btSend)
                    SendBluetoothMove(move);

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

        public Task RunAsync(MoveCollection moves, bool btSend = true) {
            return Task.Run(delegate {
                bool confirmationNeeded = true;

                for (int i = 0; i < moves.Count; i++) {
                    bool multiTurn = false;
                    CubeFace currentFace = moves[i].Face;
                    CubeFace nextFace = i < moves.Count - 1 ? moves[i + 1].Face : CubeFace.NONE;

                    if (nextFace != CubeFace.NONE && Cube.IsOpponentFace(currentFace, nextFace) && Settings.UseMultiTurn) {
                        // multi turn
                        if (confirmationNeeded) {
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
                        if (confirmationNeeded) {
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
                    if (bluetoothServer != null && btSend)
                        SendBluetoothMove(moves[i]);
                    if (multiTurn) {
                        DrawCube.AddMove(moves[i + 1]);
                        if (bluetoothServer != null && btSend)
                            SendBluetoothMove(moves[i]);
                    }

                    Application.Current.Dispatcher.Invoke(delegate {
                        ((MainWindow)Application.Current.MainWindow).cube.DoMove(moves[i]);
                        if (multiTurn)
                            ((MainWindow)Application.Current.MainWindow).cube.DoMove(moves[i + 1]);

                        if (moveHistory.Text.Length == 0)
                            moveHistory.AppendText(moves[i].ToString());
                        else
                            moveHistory.AppendText(", " + moves[i].ToString());

                        if (multiTurn)
                            moveHistory.AppendText(", " + moves[i + 1].ToString());
                    });

                    if (multiTurn)
                        i++;
                }
            });
        }
    }
}
