using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.Bluetooth;
using System.IO;
using Java.Util;
using System.Threading;
using RubinatorCore.CubeRepresentation;

namespace RubinatorTabletView {
    public class ControlHandler {
        public BluetoothDevice device;

        private BluetoothSocket socket;
        private BluetoothAdapter adapter;
        private Stream outStream;
        private Stream inStream;

        private Cube receivingState = new Cube();
        private int tilesReceived = 54;

        private readonly UUID SERVICE_UUID = UUID.FromString("053eaaaf-f981-4b64-a39e-ea4f5f44bb57");

        private void HandleBluetoothData(byte data) {
            // handle incoming state data
            if (tilesReceived < 54) {
                try {
                    CubeFace face = (CubeFace)(tilesReceived / 9);
                    int tile = tilesReceived % 9;
                    receivingState.SetTile(face, tile, (CubeColor)data);

                    tilesReceived++;
                    if (tilesReceived == 54) {
                        ((MainActivity)MainActivity.context).cube_view.renderer.AddState(receivingState);
                    }
                }
                catch (Exception) {

                }
                // single turn move
            }
            else if (data > 0x01 && data < 0x0E) {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(RubinatorCore.Utility.ByteToMove(data));
                // multi turn move
            }
            else if (data > 0x0F && data < 0x1C) {
                var moves = RubinatorCore.Utility.MultiTurnByteToMove(data);
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(moves[0]);
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(moves[1]);
            }
        }

        private void ReceiveDataThread() {
            StreamReader reader = new StreamReader(inStream);

            while (true) {
                try {
                    byte content = Convert.ToByte(reader.Read());
                    System.Diagnostics.Debug.WriteLine("CLIENT RECEIVED " + BitConverter.ToString(new byte[] { content }));
                    HandleBluetoothData(content);
                }
                catch (Exception) {
                    return;
                }
            }
        }

        public void AddButtonEvents(LinearLayout cubeViewLayout) {
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_l).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.LEFT));
                Write(0x02);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_li).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.LEFT, -1));
                Write(0x03);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_r).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.RIGHT));
                Write(0x0A);
            }; ;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_ri).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.RIGHT, -1));
                Write(0x0B);
            }; ;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_f).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.FRONT));
                Write(0x06);
            }; ;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_fi).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.FRONT, -1));
                Write(0x07);
            }; ;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_b).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.BACK));
                Write(0x0C);
            }; ;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_bi).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.BACK, -1));
                Write(0x0D);
            }; ;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_u).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.UP));
                Write(0x04);
            }; ;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_ui).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.UP, -1));
                Write(0x05);
            }; ;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_d).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.DOWN));
                Write(0x08);
            }; ;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_di).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(CubeFace.DOWN, -1));
                Write(0x09);
            };

            cubeViewLayout.FindViewById<Button>(Resource.Id.control_syncfromserver).Click += SyncFromServer;
            cubeViewLayout.FindViewById<Button>(Resource.Id.control_synctoserver).Click += SyncToServer;

            cubeViewLayout.FindViewById<Button>(Resource.Id.control_solve).Click += (obj, e) => {
                Write(0x30);
            };

            cubeViewLayout.FindViewById<Button>(Resource.Id.control_shuffle).Click += (obj, e) => {
                Write(0x31);
            };
        }

        // open up a new activity from which you can retrieve the BT_ADDR of a device
        // https://macaddresschanger.com/what-is-bluetooth-address-BD_ADDR
        public void GetAddress(Activity mainActivity) {
            var intent = new Intent(mainActivity, typeof(DeviceListActivity));
            mainActivity.StartActivityForResult(intent, 2);
        }

        public bool TryConnect(string address) {
            if (adapter == null)
                adapter = BluetoothAdapter.DefaultAdapter;

            try {
                var device = adapter.GetRemoteDevice(address);
                socket = device.CreateInsecureRfcommSocketToServiceRecord(SERVICE_UUID);
                socket.Connect();
            }
            catch (Exception) {
                return false;
            }

            outStream = socket.OutputStream;
            inStream = socket.InputStream;

            new Thread(ReceiveDataThread).Start();

            return true;
        }

        public void Disconnect() {
            if (socket != null)
                socket.Dispose();
        }

        public void Write(byte b) {
            Write(new byte[] { b });
        }

        public void Write(byte[] b) {
            try {
                outStream.Write(b, 0, b.Length);
                System.Diagnostics.Debug.WriteLine("CLIENT SENT " + BitConverter.ToString(b));
            }
            catch (Exception) {
                return;
            }
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