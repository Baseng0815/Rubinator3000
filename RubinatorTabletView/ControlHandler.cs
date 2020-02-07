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

namespace RubinatorTabletView {
    public static class ControlHandler {
        public static BluetoothDevice device;

        private static BluetoothSocket socket;
        private static BluetoothAdapter adapter;
        private static Stream outStream;
        private static Stream inStream;

        public static event EventHandler<byte> DataReceived;

        private static readonly UUID SERVICE_UUID = UUID.FromString("053eaaaf-f981-4b64-a39e-ea4f5f44bb57");

        private static void ReceiveDataThread() {
            StreamReader reader = new StreamReader(inStream);

            while (true) {
                try {
                    byte content = Convert.ToByte(reader.Read());
                    DataReceived?.Invoke(null, content);
                    System.Diagnostics.Debug.WriteLine(content);
                } catch (Exception e) {
                    return;
                }
            }
        }

        public static void AddButtonEvents(LinearLayout cubeViewLayout) {
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_l).Click += LeftButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_li).Click += LeftiButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_r).Click += RightButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_ri).Click += RightiButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_f).Click += FrontButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_fi).Click += FrontiButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_b).Click += BackButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_bi).Click += BackiButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_u).Click += UpButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_ui).Click += UpiButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_d).Click += DownButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_di).Click += DowniButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.control_downsync).Click += DownSyncButtonPressed;
            cubeViewLayout.FindViewById<Button>(Resource.Id.control_upsync).Click += UpSyncButtonPressed;
        }

        /// <summary>
        /// Open a new activity from which you can select the device
        /// </summary>
        /// <param name="mainActivity">The main activity from which to launch the new one</param>
        public static void GetAddress(Activity mainActivity) {
            var intent = new Intent(mainActivity, typeof(DeviceListActivity));
            mainActivity.StartActivityForResult(intent, 2);
        }

        public static bool TryConnect(string address) {
            if (adapter == null)
                adapter = BluetoothAdapter.DefaultAdapter;

            try {
                var device = adapter.GetRemoteDevice(address);
                socket = device.CreateInsecureRfcommSocketToServiceRecord(SERVICE_UUID);
                socket.Connect();
            } catch (Exception e) {
                return false;
            }

            outStream = socket.OutputStream;
            inStream = socket.InputStream;

            new Thread(ReceiveDataThread).Start();

            return true;
        }

        public static void Write(byte b) {
            Write(new byte[] { b });
        }

        public static void Write(byte[] b) {
            try {
                outStream.Write(b, 0, b.Length);
            } catch (Exception e) {
                return;
            }
        }

        // same codes as specified in RubinatorCommunicationProtocol.txt
        private static void DownSyncButtonPressed(object sender, EventArgs e) {

        }
        private static void UpSyncButtonPressed(object sender, EventArgs e) {
            Write(0x01);
            for (CubeFace face = CubeFace.LEFT; face <= CubeFace.BACK; face++) {
                for (int tile = 0; tile < 9; tile++) {
                    Write(Convert.ToByte(((MainActivity)MainActivity.context).cube_view.renderer.At(face, tile)));
                }
            }
        }
        private static void LeftButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.LEFT));
            Write(0x02);
        }
        private static void LeftiButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.LEFT, -1));
            Write(0x03);
        }
        private static void RightButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.RIGHT));
            Write(0x0A);
        }
        private static void RightiButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.RIGHT, -1));
            Write(0x0B);
        }
        private static void FrontButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.FRONT));
            Write(0x06);
        }
        private static void FrontiButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.FRONT, -1));
            Write(0x07);
        }
        private static void BackButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.BACK));
            Write(0x0C);
        }
        private static void BackiButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.BACK, -1));
            Write(0x0D);
        }
        private static void UpButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.UP));
            Write(0x04);
        }
        private static void UpiButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.UP, -1));
            Write(0x05);
        }
        private static void DownButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.DOWN));
            Write(0x08);
        }
        private static void DowniButtonPressed(object sender, EventArgs e) {
            ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new RubinatorCore.Move(RubinatorCore.CubeFace.DOWN, -1));
            Write(0x09);
        }
    }
}