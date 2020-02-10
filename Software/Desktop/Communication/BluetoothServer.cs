using InTheHand.Net.Sockets;
using RubinatorCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rubinator3000.Communication {
    public class BluetoothServer {
        private static readonly Guid SERVICE_UUID = new Guid("053eaaaf-f981-4b64-a39e-ea4f5f44bb57");

        private Stream stream;
        private BluetoothClient client;
        private readonly BluetoothListener listener;

        public event EventHandler<byte> DataReceived;

        private void ReceiveDataThread() {
            Log.LogMessage("Start receiving bluetooth data...");

            StreamReader reader = new StreamReader(stream);

            while (!MainWindow.ctSource.IsCancellationRequested) {
                try {
                    var read = reader.Read();
                    byte content = Convert.ToByte(read);
                    DataReceived?.Invoke(null, content);
                } catch (Exception e) {
                    Log.LogMessage(e.ToString());
                    return;
                }
            }
        }

        public BluetoothServer() {
            listener = new BluetoothListener(SERVICE_UUID);
        }

        public void Disconnect() {
            if (listener != null) {
                listener.Stop();
            }

            if (client != null) {
                client.Dispose();
            }
        }

        public void Write(byte b) {
            Write(new byte[] { b });
        }
        public void Write(byte[] b) {
            try {
                stream.Write(b, 0, b.Length);
            } catch (Exception e) {
                Log.LogMessage(e.ToString());
            }
        }

        /// <summary>
        /// Discovers the device to connect to and receives data
        /// </summary>
        public void StartDiscovering() {
            Task.Run(() => {
                Log.LogMessage("Start listening for bluetooth devices...");

                listener.Start();
                try {
                    client = listener.AcceptBluetoothClient();
                } catch (Exception e) {
                    Log.LogMessage(e.ToString());
                    return;
                }

                Log.LogMessage("Bluetooth device with name '" + client.RemoteMachineName + "' connected.");

                stream = client.GetStream();

                new Thread(ReceiveDataThread).Start();
            });
        }
    }
}
