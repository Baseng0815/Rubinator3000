using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RubinatorTabletView {

    [Activity(Label = "@string/activity_device_list_name")]
    public class DeviceListActivity : Activity {
        public const string EXTRA_DEVICE_ADDRESS = "device_address";
        public const string EXTRA_DEVICE_NAME = "device_name";

        BluetoothAdapter adapter;
        static ArrayAdapter<string> newDevicesArrayAdapter;
        DeviceDiscoveredReceiver receiver;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.IndeterminateProgress);
            SetContentView(Resource.Layout.activity_device_list);

            SetResult(Result.Canceled);

            var scanButton = FindViewById<Button>(Resource.Id.button_scan);
            scanButton.Click += (sender, e) => {
                DoDiscovery();
                (sender as Android.Views.View).Visibility = ViewStates.Gone;
            };

            // init array adapters
            var pairedDevicesArrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.device_name);
            newDevicesArrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.device_name);

            // setup list views
            var pairedListView = FindViewById<ListView>(Resource.Id.paired_devices);
            pairedListView.Adapter = pairedDevicesArrayAdapter;
            pairedListView.ItemClick += DeviceListView_ItemClick;

            var newListView = FindViewById<ListView>(Resource.Id.new_devices);
            newListView.Adapter = newDevicesArrayAdapter;
            newListView.ItemClick += DeviceListView_ItemClick;

            // register for broadcasts when a device is discovered
            receiver = new DeviceDiscoveredReceiver();
            var filter = new IntentFilter(BluetoothDevice.ActionFound);
            RegisterReceiver(receiver, filter);

            // register for broadcasts when discovery has finished
            filter = new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished);
            RegisterReceiver(receiver, filter);

            adapter = BluetoothAdapter.DefaultAdapter;

            var pairedDevices = adapter.BondedDevices;

            if (pairedDevices.Count > 0) {
                FindViewById(Resource.Id.title_paired_devices).Visibility = ViewStates.Visible;
                foreach (var device in pairedDevices)
                    pairedDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
            } else {
                var noDevices = Resources.GetText(Resource.String.none_paired);
                pairedDevicesArrayAdapter.Add(noDevices);
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            if (adapter != null)
                adapter.CancelDiscovery();
            UnregisterReceiver(receiver);
        }

        void DoDiscovery() {
            SetProgressBarIndeterminateVisibility(true);
            SetTitle(Resource.String.scanning);

            FindViewById<Android.Views.View>(Resource.Id.title_new_devices).Visibility = ViewStates.Visible;

            if (adapter.IsDiscovering)
                adapter.CancelDiscovery();

            adapter.StartDiscovery();
        }

        void DeviceListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e) {
            adapter.CancelDiscovery();

            var info = ((TextView)e.View).Text;
            var address = info.Substring(info.Length - 17);
            var name = info.Substring(0, info.Length - 18);

            var intent = new Intent();
            intent.PutExtra(EXTRA_DEVICE_ADDRESS, address);
            intent.PutExtra(EXTRA_DEVICE_NAME, name);

            SetResult(Result.Ok, intent);
            Finish();
        }

        public class DeviceDiscoveredReceiver : BroadcastReceiver {
            public override void OnReceive(Context context, Intent intent) {
                string action = intent.Action;

                if (action == BluetoothDevice.ActionFound) {
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    if (device.BondState != Bond.Bonded)
                        newDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
                }
            }
        }
    }
}