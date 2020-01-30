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

namespace RubinatorTabletView {
    public static class ControlHandler {
        public static BluetoothDevice device;

        private static BluetoothSocket socket;
        private static BluetoothAdapter adapter;
        private static Stream outStream;

        private static readonly UUID SERVICE_UUID = UUID.FromString("053eaaaf-f981-4b64-a39e-ea4f5f44bb57");

        public static void AddButtonEvents(TableLayout controlLayout) {
            controlLayout.FindViewById<Button>(Resource.Id.button_l).Click += LeftButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_li).Click += LeftiButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_r).Click += RightButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_ri).Click += RightiButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_f).Click += FrontButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_fi).Click += FrontiButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_b).Click += BackButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_bi).Click += BackiButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_u).Click += UpButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_ui).Click += UpiButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_d).Click += DownButtonPressed;
            controlLayout.FindViewById<Button>(Resource.Id.button_di).Click += DowniButtonPressed;
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

            return true;
        }

        public static void Write(byte[] buffer) {
            try {
                outStream.Write(buffer, 0, buffer.Length);
            } catch (Exception e) {
                return;
            }
        }

        private static void LeftButtonPressed(object sender, EventArgs e) {
            Write(new byte[] { 0x1 });
        }
        private static void LeftiButtonPressed(object sender, EventArgs e) {

        }
        private static void RightButtonPressed(object sender, EventArgs e) {

        }
        private static void RightiButtonPressed(object sender, EventArgs e) {

        }
        private static void FrontButtonPressed(object sender, EventArgs e) {

        }
        private static void FrontiButtonPressed(object sender, EventArgs e) {

        }
        private static void BackButtonPressed(object sender, EventArgs e) {

        }
        private static void BackiButtonPressed(object sender, EventArgs e) {

        }
        private static void UpButtonPressed(object sender, EventArgs e) {

        }
        private static void UpiButtonPressed(object sender, EventArgs e) {

        }
        private static void DownButtonPressed(object sender, EventArgs e) {

        }
        private static void DowniButtonPressed(object sender, EventArgs e) {

        }
    }
}