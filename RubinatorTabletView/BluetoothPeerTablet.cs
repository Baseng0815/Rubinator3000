using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using System.IO;
using Java.Util;
using System.Threading;
using RubinatorCore;
using RubinatorCore.Communication;

namespace RubinatorTabletView {
    public class BluetoothPeerTablet : BluetoothPeer {
        private readonly UUID SERVICE_UUID;
        private BluetoothSocket socket;
        private BluetoothAdapter adapter;
        private Stream outStream;
        private Stream inStream;

        public BluetoothDevice device;

        protected override void ReceiveDataThread() {
            StreamReader reader = new StreamReader(inStream);

            while (inStream.CanRead) {
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

            Log.LogMessage("Breakout from receive tablet thread");
        }

        public BluetoothPeerTablet() {
            SERVICE_UUID = UUID.FromString(UUID_STRING);

            currentPacket = new Packet();
        }

        protected override void WriteByte(byte b) {
            WriteBytes(new byte[] { b });
        }

        protected override void WriteBytes(byte[] bytes) {
            System.Diagnostics.Debug.WriteLine("WRITE BYTES " + BitConverter.ToString(bytes));
            if (outStream != null && outStream.CanWrite) {
                outStream.Write(bytes, 0, bytes.Length);
            }
        }

        public override bool Connect(string address) {
            if (adapter == null)
                adapter = BluetoothAdapter.DefaultAdapter;

            try {
                var device = adapter.GetRemoteDevice(address);
                socket = device.CreateInsecureRfcommSocketToServiceRecord(SERVICE_UUID);
                socket.Connect();
            } catch (Exception) {
                return false;
            }

            outStream = socket.OutputStream;
            inStream = socket.InputStream;

            new Thread(ReceiveDataThread).Start();

            return true;
        }

        public override void Disconnect() {
            if (socket != null) {
                socket.Close();
                outStream.Close();
                inStream.Close();
            }
        }
    }
}