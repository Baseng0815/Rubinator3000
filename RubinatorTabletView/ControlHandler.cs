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

namespace RubinatorTabletView {
    public static class ControlHandler {
        public static BluetoothDevice device;

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

        public static void Connect(Activity activity) {
            var intent = new Intent(activity, typeof(DeviceListActivity));
            activity.StartActivityForResult(intent, 2);

            /*AlertDialog.Builder alert = new AlertDialog.Builder(activity);
            alert.SetTitle("Connect to device");
            alert.SetMessage(intent.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS));

            Dialog dialog = alert.Create();
            dialog.Show();
            */
        }

        private static void LeftButtonPressed(object sender, EventArgs e) {

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