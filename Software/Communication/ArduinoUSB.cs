using Rubinator3000;
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
                Log.LogMessage("Der Port kann nicht geöffnet werden:" + e.Message);
                return;
            }
        }

        public override void Connect() {
            if (!serial.IsOpen) {
                Log.LogMessage("Failed to write data, port is not open.");
                return;
            }

            serial.Write(new byte[] { 0xA1 }, 0, 1);

            byte response;
            try {
                response = (byte)serial.ReadByte();
            } catch (TimeoutException e) {
                Log.LogMessage(e.ToString());
                return;
            }

            if (response != 0xF1) {
                Log.LogMessage("Arduinoprogramm ist nicht korrekt. Response: " + response);
                return;
            }
            else {
                connected = true;
            }
        }

        public override void Disconnect() {
            if (serial.IsOpen) {
                serial.Write(new byte[] { 0xA0 }, 0, 1);

                if (serial.ReadByte() != 0xF0) {
                    Log.LogMessage("Aduinoprogramm ist nicht korrekt");
                    return;
                } else {
                    connected = false;
                }

                serial.Close();
            }
        }

        public override void Dispose() {
            Disconnect();
        }

        public override async void SendMove(Move move) {
            if (serial == null || !serial.IsOpen) {
                Log.LogMessage("Der Port ist nicht geöffnet!");
                return;
            }

            if (!connected) {
                Log.LogMessage("Der Arduino ist nicht verbunden!");
                return;
            }

            byte[] moveData = MoveToByte(move);

            serial.Write(moveData, 0, moveData.Length);

            int responseCount = 0;
            byte expectedResonse = (byte)(0x10 | moveData[0]);
            int timeout = 5000;
            Stopwatch stopwatch = new Stopwatch();

            do {
                stopwatch.Start();

                var getResponse = Task.Factory.StartNew(() => (byte)serial.ReadByte());

                while (stopwatch.ElapsedMilliseconds < timeout && !getResponse.IsCompleted) ;

                if (getResponse.IsCompleted) {
                    byte response = await getResponse;

                    if (response != expectedResonse) {
                        Log.LogMessage("Wrong response. Expected " + expectedResonse + ", but got " + response);
                        return;
                    }
                    else {
                        responseCount++;
                    }
                }
                else {
                    Log.LogMessage("Arduino is not resposing");
                    return;
                }
            } while (responseCount != moveData.Length);
        }

        public override void SendMoves(IEnumerable<Move> moves) {
            if (serial == null || !serial.IsOpen) {
                Log.LogMessage("Der Port ist nicht geöffnet!");
                return;
            }

            if (!connected) {
                Log.LogMessage("Der Arduino ist nicht verbunden!");
                return;
            }

            foreach (Move move in moves)
                SendMove(move);
        }
    }
}
