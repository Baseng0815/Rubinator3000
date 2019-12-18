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

            if (!portNames.Contains(portName))
                throw new ArgumentException($"Der Port\"{portName}\" wurde nicht gefunden!", nameof(portName));

            serial = new SerialPort(portName, baudRate);

            // open serial port
            try {
                serial.Open();
            }
            catch (Exception e) {
                throw new InvalidOperationException("Der Port kann nicht geöffnet werden.", e);
            }
        }

        public override void Connect() {
            serial.Write(new byte[] { 0xA1 }, 0, 1);

            byte response = (byte)serial.ReadByte();
            if (response != 0xF1) {
                throw new InvalidProgramException("Aduinoprogramm ist nicht korrekt");
            }
            else {
                connected = true;
            }
        }

        public override void Disconnect() {
            serial.Write(new byte[] { 0xA0 }, 0, 1);

            if (serial.ReadByte() != 0xF0) {
                throw new InvalidProgramException("Aduinoprogramm ist nicht korrekt");
            }
            else {
                connected = false;
            }
        }

        public override void Dispose() {
            serial.Close();
        }

        public override async void SendMove(Move move) {
            if (serial == null || !serial.IsOpen)
                throw new InvalidOperationException("Der Port ist nicht geöffnet!");

            if (!connected)
                throw new InvalidOperationException("Der Arduino ist nicht verbunden!");

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
                        throw new Exception();
                    }
                    else {
                        responseCount++;
                    }
                }
                else {
                    throw new TimeoutException("Arduino is not resposing");
                }
            } while (responseCount != moveData.Length);
        }

        public override void SendMoves(IEnumerable<Move> moves) {
            if (serial == null || !serial.IsOpen)
                throw new InvalidOperationException("Der Port ist nicht geöffnet!");

            if (!connected)
                throw new InvalidOperationException("Der Arduino ist nicht verbunden!");

            foreach (Move move in moves)
                SendMove(move);
        }
    }
}
