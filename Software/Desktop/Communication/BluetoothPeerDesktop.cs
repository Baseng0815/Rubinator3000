using InTheHand.Net.Sockets;
using Java.Util;
using RubinatorCore;
using RubinatorCore.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rubinator3000.Communication {
    public class BluetoothPeerDesktop : BluetoothPeer {
        private readonly Guid SERVICE_UUID;
        private Stream stream;
        private BluetoothClient client;
        private readonly BluetoothListener listener;

        protected override void ReceiveDataThread() {
            StreamReader reader = new StreamReader(stream);

            while (stream.CanRead) {
                try {
                    int read = reader.Read();
                    if (read == -1) continue;
                    byte b = Convert.ToByte(read);
                    HandleReceivedByte(b);
                } catch (Exception e) {
                    Log.LogMessage(e.Message);
                    continue;
                }
            }

            Log.LogMessage("Breakout from receive desktop thread");
        }

        public BluetoothPeerDesktop() {
            SERVICE_UUID = new Guid(UUID_STRING);

            listener = new BluetoothListener(SERVICE_UUID);
        }

        protected override void WriteByte(byte b) {
            WriteBytes(new byte[] { b });
        }

        protected override void WriteBytes(byte[] bytes) {
            if (stream != null && stream.CanWrite) {
                stream.Write(bytes, 0, bytes.Length);
            } else
                throw new IOException("Stream not open.");
        }

        public override bool Connect(string address) {
            // start listening
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

            return true;
        }

        public override void Disconnect() {
            if (listener != null) {
                listener.Stop();
            }

            if (client != null) {
                client.Dispose();
                stream.Dispose();
            }
        }

    }
}
