using Rubinator3000;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rubinator3000 {
    class ArduinoUSB : Arduino {

        private SerialPort serial;
        private bool connected = false;
        private Move currentMove;        

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
            serial.DataReceived += DataReceived;

            // connect to Arduino
            serial.WriteLine("connect");            
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e) {
            if(sender is SerialPort port) {
                string data = port.ReadExisting();

                switch (data) {
                    case "Connected":
                        connected = true;
                        break;
                    case "Disconnected":
                        connected = false;
                        break;
                    case "Move done":
                        InvokeMoveDone(this, new MoveEventArgs(currentMove));                        
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Dispose() {
            serial.Close();
        }

        public override void SendMove(Move move) {
            if (serial ==  null || !serial.IsOpen)
                throw new InvalidOperationException("Der Port ist nicht geöffnet!");

            if (!connected)
                throw new InvalidOperationException("Der Arduino ist nicht verbunden!");

            currentMove = move;
            
            string moveStr = move.ToString();

            serial.WriteLine(moveStr);                       
        }

        public override void SendMoves(IEnumerable<Move> moves) {
            if (serial == null || !serial.IsOpen)
                throw new InvalidOperationException("Der Port ist nicht geöffnet!");

            if (!connected)
                throw new InvalidOperationException("Der Arduino ist nicht verbunden!");

            string data = string.Join("\r\n", moves.Select(m => m.ToString()));

            serial.WriteLine(data);
        }
    }
}
