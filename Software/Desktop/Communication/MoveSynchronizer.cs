

using Rubinator3000.CubeView;
using RubinatorCore;
using RubinatorCore.CubeRepresentation;
using System;
using System.Diagnostics;
using System.Threading;
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

        public bool ArduinoConnected => arduino?.Connected ?? false;

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
                }
                catch (Exception e) {
                    Log.LogMessage(e.ToString());
                }
                // do move
            }
            else if (data > 0x01 && data < 0x0E)
                RunAsync(RubinatorCore.Utility.ByteToMove(data), false);

            // start receiving state from client
            else if (data == 0x01) {
                tilesReceived = 0;
                // send state to client
            }
            else if (data == 0x00) {
                for (CubeFace face = CubeFace.LEFT; face <= CubeFace.BACK; face++) {
                    for (int tile = 0; tile < 9; tile++) {
                        Application.Current.Dispatcher.Invoke(delegate {
                            bluetoothServer.Write((byte)((MainWindow)Application.Current.MainWindow).cube.At(face, tile));
                        });
                    }
                }
                // solve cube
            }
            else if (data == 0x30) {
                Application.Current.Dispatcher.Invoke(delegate {
                    ((MainWindow)Application.Current.MainWindow).SolveCube();
                });
                // shuffle cube
            }
            else if (data == 0x31) {
                Application.Current.Dispatcher.Invoke(delegate {
                    ((MainWindow)Application.Current.MainWindow).ShuffleCube();
                });
            }
        }

        private void SendBluetoothMove(Move move) {
            bluetoothServer.Write(RubinatorCore.Utility.MoveToByte(move));
        }

        private void SendBluetoothMove(Move move1, Move move2) {
            bluetoothServer.Write(RubinatorCore.Utility.MultiTurnToByte(move1, move2));
        }

        public MoveSynchronizer(TextBox moveHistory) {
            this.moveHistory = moveHistory;
        }

        public void ConnectArduino(string portName) {
            if (arduino != null)
                arduino.Disconnect();

            arduino = new ArduinoUSB(portName);
            arduino.Connect();

            arduino.SendLedCommand(ArduinoLEDs.ALL, 0);
        }

        public void DisconnectArduino() {
            if (arduino != null)
                arduino.Disconnect();
        }

        public void SetSolvedState(bool state) {
            if (arduino != null && arduino.Connected) {
                arduino.SetSolvedState(state);
            }
        }

        public void SetArduinoLEDs(ArduinoLEDs leds, byte brightness) {
            if (arduino != null)
                arduino.SendLedCommand(leds, brightness);
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
            return Task.Run(async delegate {
                Task arduinoMoveTask = Task.Factory.StartNew(() => { return; });
                if (arduino == null)
                    Log.LogMessage("Arduino not connected");
                else
                    arduinoMoveTask = arduino.SendMoveAsync(move);

                DrawCube.AddMove(move);
                if (bluetoothServer != null && btSend)
                    SendBluetoothMove(move);


                Application.Current.Dispatcher.Invoke(delegate {
                    ((MainWindow)Application.Current.MainWindow).cube.DoMove(move);

                    if (moveHistory.Text.Length == 0)
                        moveHistory.AppendText(move.ToString());
                    else
                        moveHistory.AppendText(", " + move.ToString());
                });

                await arduinoMoveTask;

                Thread.Sleep(Settings.StepDelay);
            });
        }

        public Task RunAsync(MoveCollection moves, bool btSend = true) {
            return Task.Run(async delegate {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();
                for (int i = 0; i < moves.Count; i++) {
                    bool multiTurn = false;
                    CubeFace currentFace = moves[i].Face;
                    CubeFace nextFace = i < moves.Count - 1 ? moves[i + 1].Face : CubeFace.NONE;
                    Task arduinoMoveTask = Task.Factory.StartNew(() => { return; });

                    if (nextFace != CubeFace.NONE && Cube.IsOpponentFace(currentFace, nextFace) && Settings.UseMultiTurn) {
                        // multi turn                        
                        if (arduino != null)
                            arduinoMoveTask = arduino.SendMultiTurnMoveAsync(moves[i], moves[i + 1]);

                        if (bluetoothServer != null && btSend)
                            SendBluetoothMove(moves[i], moves[i + 1]);

                        multiTurn = true;
                    }
                    else {
                        // normal move                        
                        if (arduino != null)
                            arduinoMoveTask = arduino.SendMoveAsync(moves[i]);
                        if (bluetoothServer != null && btSend)
                            SendBluetoothMove(moves[i]);
                    }

                    DrawCube.AddMove(moves[i]);
                    if (multiTurn) {
                        DrawCube.AddMove(moves[i + 1]);
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

                    await arduinoMoveTask;

                    Thread.Sleep(Settings.StepDelay);
                }

                stopwatch.Stop();
                Log.LogMessage("Time needed: " + stopwatch.ElapsedMilliseconds + "ms");
            });
        }
    }
}
