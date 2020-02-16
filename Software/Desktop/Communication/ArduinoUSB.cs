using Rubinator3000;
using RubinatorCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Rubinator3000 {
    class ArduinoUSB : Arduino {

        private SerialPort serial;

        public override bool Connected => connected;
        private bool connected = false;

        public ArduinoUSB(string portName, int baudRate = 9600) {
            string[] portNames = SerialPort.GetPortNames();

            if (!portNames.Contains(portName)) {
                Log.LogMessage($"Der Port\"{portName}\" wurde nicht gefunden!");
                return;
            }

            serial = new SerialPort(portName, baudRate);
            serial.ReadTimeout = Settings.ArduinoTimeout;

            // open serial port
            try {
                serial.Open();

                Log.LogMessage("Arduino connected");
            }
            catch (Exception e) {
                Log.LogMessage("Der Port kann nicht geöffnet werden:\r\n" + e.ToString());
                return;
            }
        }

        public override void Connect() {
            try {
                byte response = SendCommand(0xA1)[0];
                if (response != 0xF1) {
                    Log.LogMessage("Arduinoprogramm ist nicht korrekt. Response: " + Convert.ToString(response, 16));
                    return;
                }
                else
                    connected = true;
            }
            catch (Exception e) {
                Log.LogMessage(e.ToString());
                return;
            }
        }

        public override void Disconnect() {
            if (serial?.IsOpen ?? false) {
                try {
                    byte response = SendCommand(0xA0)[0];
                    if (response != 0xF0) {
                        Log.LogMessage("Aduinoprogramm ist nicht korrekt");
                    }

                    connected = false;


                    serial.Close();
                }
                catch (Exception e) {
                    Log.LogMessage(e.ToString());
                }

            }
        }

        public override void Dispose() {
            Disconnect();
        }

        ~ArduinoUSB() {
            Dispose();
        }

        public override void SendMove(Move move) {
            if (serial == null || !serial.IsOpen) {
                Log.LogMessage("Der Port ist nicht geöffnet!");
                return;
            }

            if (!connected) {
                Log.LogMessage("Der Arduino ist nicht verbunden!");
                return;
            }

            byte[] moveData = RubinatorCore.Utility.MoveToByte(move);

            // send and receive arduino response
            SendCommand(moveData);
        }

        public override void SendMultiTurnMove(Move move1, Move move2) {
            if (serial == null || !serial.IsOpen) {
                Log.LogMessage("Der Port ist nicht geöffnet!");
                return;
            }

            if (!connected) {
                Log.LogMessage("Der Arduino ist nicht verbunden!");
                return;
            }

            byte[] moveData = RubinatorCore.Utility.MultiTurnToByte(move1, move2);

            SendCommand(moveData);
        }

        public override void SendLedCommand(ArduinoLEDs leds, byte brightness) {
            if (!connected || !serial.IsOpen)
                return;

            byte command = (byte)(0x40 | (byte)leds);

            SendCommand(command, brightness);
        }

        public override void SetSolvedState(bool state) {
            if (state) {
                SendCommand(0xA2);
            }
            else {
                SendCommand(0xA1);
            }
        }

        protected override byte[] SendCommand(params byte[] command) {
            byte[] response = new byte[command.Length];

            serial.Write(command, 0, command.Length);
            serial.Read(response, 0, command.Length);

            return response;
        }
    }
}
