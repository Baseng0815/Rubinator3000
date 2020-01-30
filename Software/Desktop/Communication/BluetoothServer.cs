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

        private BluetoothListener listener;

        public BluetoothServer() {
            listener = new BluetoothListener(SERVICE_UUID);
        }

        public void StartListening() {
            listener.Start();
            BluetoothClient connection = listener.AcceptBluetoothClient();
            Log.LogMessage("Bluetooth device with name '" + connection.RemoteMachineName + "' connected.");

            // TODO read from stream and add thread
            Stream peerStream = connection.GetStream();

        }
    }
}
