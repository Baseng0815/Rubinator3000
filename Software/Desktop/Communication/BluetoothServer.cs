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

        private Stream inputStream;
        private BluetoothClient client;
        private readonly BluetoothListener listener;

        public event EventHandler<byte> DataReceived;

        protected void OnDataReceived(byte data) {
            EventHandler<byte> handler = DataReceived;
            if (handler != null)
                handler(this, data);
        }

        private void ReceiveDataThread() {
            Log.LogMessage("Start receiving bluetooth data...");

            StreamReader reader = new StreamReader(inputStream);

            while (!MainWindow.ctSource.IsCancellationRequested) {
                try {
                    byte content = Convert.ToByte(reader.Read());
                    OnDataReceived(content);
                } catch (Exception e) {
                    Log.LogMessage(e.Message);
                }
            }
        }

        public BluetoothServer() {
            listener = new BluetoothListener(SERVICE_UUID);
        }

        /// <summary>
        /// Discovers the device to connect to and receives data
        /// </summary>
        public void StartDiscovering() {
            Task.Run(() => {
                Log.LogMessage("Start listening for bluetooth devices...");

                listener.Start();
                client = listener.AcceptBluetoothClient();
                Log.LogMessage("Bluetooth device with name '" + client.RemoteMachineName + "' connected.");

                inputStream = client.GetStream();

                new Thread(ReceiveDataThread).Start();
            });
        }
    }
}
