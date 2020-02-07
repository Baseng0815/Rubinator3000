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
            }
            catch (Exception e) {
                Log.LogMessage("Der Port kann nicht geöffnet werden:" + e.ToString());
                return;
            }
        }

        public override void Connect() {
            try {
                serial.Write(new byte[] { 0xA1 }, 0, 1);

                byte response;
                response = (byte)serial.ReadByte();
                if (response != 0xF1) {
                    Log.LogMessage("Arduinoprogramm ist nicht korrekt. Response: " + response);
                    return;
                } else
                    connected = true;
            } catch (Exception e) {
                Log.LogMessage(e.ToString());
                return;
            }
        }

        public override void Disconnect() {
            if (serial.IsOpen) {
                serial.Write(new byte[] { 0xA0 }, 0, 1);

                try {
                    if (serial.ReadByte() != 0xF0) {
                        Log.LogMessage("Aduinoprogramm ist nicht korrekt");
                        return;
                    } else {
                        connected = false;
                    }

                    serial.Close();
                } catch (Exception e) {
                    Log.LogMessage(e.ToString());
                }

            }
        }

        public override void Dispose() {
            Disconnect();
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

            Debug.WriteLine(BitConverter.ToString(moveData));
            serial.Write(moveData, 0, moveData.Length);
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

            byte[] moveData = RubinatorCore.Utility.MulitTurnMoveToByte(move1, move2);

            serial.Write(moveData, 0, moveData.Length);
        }
    }
}
