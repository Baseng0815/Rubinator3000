using Rubinator3000;
using RubinatorCore;
using RubinatorCore.Communication;
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

        public ArduinoUSB(string portName, int baudRate = 9600) {
            string[] portNames = SerialPort.GetPortNames();

            if (!portNames.Contains(portName)) {
                Log.LogMessage($"The port\"{portName}\" does not exist!");
                return;
            }

            serial = new SerialPort(portName, baudRate);
            serial.ReadTimeout = Settings.ArduinoTimeout;

            // open serial port
            try {
                serial.Open();

                Log.LogMessage("Serial connection open");
            }
            catch (Exception e) {
                Log.LogMessage("The port cannot be opened:\r\n" + e.ToString());
                return;
            }
        }        

        public override void Dispose() {
            Disconnect();
        }

        ~ArduinoUSB() {
            Dispose();
        }

        protected override Packet SendPacket(Packet packet) {
            if(Connected && serial.IsOpen) {
                // ensure that input stream is empty
                if (serial.BytesToRead > 0)
                    serial.ReadExisting();

                byte[] data = packet.GetData();

                serial.Write(data, 0, data.Length);

                try {
                    int instruction = serial.ReadByte();
                    if(instruction == -1) {
                        throw new Exception("No response received");
                    }

                    int lenght = Packet.InstructionLengths[(byte)instruction];

                    byte[] responseData = new byte[lenght];
                    serial.Read(responseData, 0, lenght);

                    return new Packet((byte)instruction, responseData);
                }
                catch (Exception e) {
                    throw e;
                }                
            }
            else
                throw new InvalidOperationException("Arduino not connected or port closed");
        }
    }
}
