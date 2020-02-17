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
        public BluetoothDevice device;
        private BluetoothSocket socket;
        private BluetoothAdapter adapter;
        private Stream outStream;
        private Stream inStream;

        private UUID SERVICE_UUID;

        public BluetoothPeerTablet() {
            SERVICE_UUID = UUID.FromString(UUID_STRING);

            InstructionDataLength = new Dictionary<byte, int>();
            currentPacket = new Packet();

            // TODO add instructions here
            //InstructionDataLength.Add() ...
        }
        protected override void WriteByte(byte b) {
            WriteBytes(new byte[] { b });
        }
        protected override void WriteBytes(byte[] bytes) {
            if (outStream != null && outStream.CanWrite) {
                outStream.Write(bytes, 0, bytes.Length);
            }
        }

        protected override void HandleReceivedByte(byte b) {
            // new instruction
            if (currentPacket.Instruction == 0x00) {
                currentPacket.Instruction = b;

            // there is still data to add
            } else if (currentPacket.Data.Count < InstructionDataLength[currentPacket.Instruction]) {
                currentPacket.Data.Add(b);

            // instruction and data received, invoke callback
            } else {
                RaisePacketReceived(this, currentPacket);
                currentPacket.Instruction = 0x00;
            }

            // handle incoming state data
            if (currentPacket < 54) {
                try {
                    CubeFace face = (CubeFace)(tilesReceived / 9);
                    int tile = tilesReceived % 9;
                    receivingState.SetTile(face, tile, (CubeColor)data);

                    tilesReceived++;
                    if (tilesReceived == 54) {
                        ((MainActivity)MainActivity.context).cube_view.renderer.AddState(receivingState);
                    }
                } catch (Exception e) {

                }
            // single turn move
            } else if (data > 0x01 && data < 0x0E) {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(RubinatorCore.Utility.ByteToMove(data));
            // multi turn move
            } else if (data > 0x0F && data < 0x1C) {
                var moves = RubinatorCore.Utility.MultiTurnByteToMove(data);
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(moves[0]);
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(moves[1]);
            }
        }

        public override void SendPacket(Packet packet) {
            WriteByte(packet.Instruction);
            WriteBytes(packet.Data.ToArray());
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
                socket.Dispose();
                outStream.Dispose();
                inStream.Dispose();
            }
        }

        private void ReceiveDataThread() {
            StreamReader reader = new StreamReader(inStream);

            while (inStream.CanRead) {
                try {
                    byte b = Convert.ToByte(reader.Read());
                    HandleReceivedByte(b);
                } catch (Exception) {
                    continue;
                }
            }
        }

        // open up a new activity from which you can retrieve the BT_ADDR of a device
        // https://macaddresschanger.com/what-is-bluetooth-address-BD_ADDR
        public void GetAddress(Activity mainActivity) {
            var intent = new Intent(mainActivity, typeof(DeviceListActivity));
            mainActivity.StartActivityForResult(intent, 2);
        }


        // same codes as specified in RubinatorCommunicationProtocol.txt
        private void SyncFromServer(object sender, EventArgs e) {
            // request the server to send the state
            tilesReceived = 0;
            Write(0x00);
        }
        private void SyncToServer(object sender, EventArgs e) {
            // request the server to read the state data (54 bytes) which follows
            Write(0x01);
            for (CubeFace face = CubeFace.LEFT; face <= CubeFace.BACK; face++) {
                for (int tile = 0; tile < 9; tile++) {
                    Write(Convert.ToByte(((MainActivity)MainActivity.context).cube_view.renderer.At(face, tile)));
                }
            }
        }
    }
}