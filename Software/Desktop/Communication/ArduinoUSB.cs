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
            } catch (Exception e) {
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

        protected override Packet SendPacket(Packet packet, bool requireConnection = true) {
            if (!(!Connected && requireConnection) && serial.IsOpen) {
                // ensure that input stream is empty
                if (serial.BytesToRead > 0)
                    serial.ReadExisting();

                byte[] data = /*packet.GetData();*/ (new Packet(0x05, new byte[2] { 0x03, 0xFF }).GetData());

                serial.Write(data, 0, data.Length);
                serial.Write(new byte[1] { 0xFF }, 0, 1);

                try {
                    Packet response = ReadPacket();

                    return response;
                } catch (Exception e) {
                    throw e;
                }
            } else
                throw new InvalidOperationException("Arduino not connected or port closed");
        }

        private Packet ReadPacket() {
            byte[] buffer = new byte[64];

            int bytesRead = 0;
            byte readByte = 0x00;

            while ((readByte = (byte)serial.ReadByte()) != 0xFF) {
                buffer[bytesRead] = readByte;
                bytesRead++;
            }

            return new Packet(buffer[0], buffer.Skip(1).TakeWhile(b => b != 0x00).ToArray());
        }
    }
}
