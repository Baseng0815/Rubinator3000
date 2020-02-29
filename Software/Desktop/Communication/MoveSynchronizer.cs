using RubinatorCore;
using RubinatorCore.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        private BluetoothPeerDesktop bluetoothPeer;

        public bool ArduinoConnected => arduino?.Connected ?? false;

        private void HandlePacket(Packet packet) {
            // handle incoming state data
            switch (packet.Instruction) {
                // single move
                case 0x01:
                    RunAsync(Utility.ByteToMove(packet.Data[0]), false);
                    break;

                // cube sent
                case 0x03:
                    Application.Current.Dispatcher.Invoke(delegate {
                        Cube newState = new Cube();
                        for (int i = 0; i < 54; i++)
                            newState.SetTile((CubeFace)(i / 9), i % 9, (CubeColor)packet.Data[i]);
                        ((MainWindow)Application.Current.MainWindow).cube = newState;
                        DrawCube.AddState(newState);
                    });
                    break;

                // cube requested
                case 0x04:
                    Packet requestPacket = new Packet(0x03);
                    for (CubeFace face = CubeFace.LEFT; face <= CubeFace.BACK; face++) {
                        for (int tile = 0; tile < 9; tile++) {
                            Application.Current.Dispatcher.Invoke(delegate {
                                requestPacket.Data.Add((byte)((MainWindow)Application.Current.MainWindow).cube.At(face, tile));
                            });
                        }
                    }
                    bluetoothPeer.SendPacket(requestPacket);
                    break;

                // tablet control
                case 0x06:
                    if (packet.Data[0] == 0x30)
                        Application.Current.Dispatcher.Invoke(delegate {
                            ((MainWindow)Application.Current.MainWindow).SolveCube();
                        });
                    else if (packet.Data[0] == 0x31)
                        Application.Current.Dispatcher.Invoke(delegate {
                            ((MainWindow)Application.Current.MainWindow).ShuffleCube();
                        });
                    break;


                default:
                    break;
            }
        }

        private void SendBluetoothMove(Move move) {
            bluetoothPeer.SendPacket(move.GetPacketData());
        }

        private void SendBluetoothMove(Move move1, Move move2) {
            bluetoothPeer.SendPacket(Utility.GetMultiturnPacketData(move1, move2));
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
                
            }
        }

        public void SetArduinoLEDs(ArduinoLEDs leds, byte brightness) {
            if (arduino != null)
                arduino.SendLedCommand(leds, brightness);
        }

        public void SetupBluetooth() {
            bluetoothPeer = new BluetoothPeerDesktop();

            bluetoothPeer.PacketReceived += (obj, data) => {
                HandlePacket(data);
                Log.LogMessage("Bluetooth data received: " + data);
            };

            bluetoothPeer.Connect("just listening, no address needed");
        }

        public void UnsetupBluetooth() {
            bluetoothPeer.Disconnect();
        }

        public Task RunAsync(Move move, bool btSend = true) {
            return Task.Run(async delegate {
                Task arduinoMoveTask = Task.Factory.StartNew(() => { return; });
                if (arduino == null)
                    Log.LogMessage("Arduino not connected");
                else
                    arduinoMoveTask = arduino.SendMoveAsync(move);

                DrawCube.AddMove(move);
                if (bluetoothPeer != null && btSend)
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
                bool confirmationNeeded = false;

                for (int i = 0; i < moves.Count; i++) {
                    bool multiTurn = false;
                    CubeFace currentFace = moves[i].Face;
                    CubeFace nextFace = i < moves.Count - 1 ? moves[i + 1].Face : CubeFace.NONE;
                    Task arduinoMoveTask = Task.Factory.StartNew(() => { return; });

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
                            arduinoMoveTask = arduino.SendMultiTurnMoveAsync(moves[i], moves[i + 1]);

                        if (bluetoothPeer != null && btSend)
                            SendBluetoothMove(moves[i], moves[i + 1]);

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
                            arduinoMoveTask = arduino.SendMoveAsync(moves[i]);
                        if (bluetoothPeer != null && btSend)
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
            });
        }
    }
}
