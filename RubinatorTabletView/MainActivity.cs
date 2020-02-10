using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace RubinatorTabletView {

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity {

        private ControlHandler controlHandler;

        public CubeView cube_view;
        public CancellationTokenSource ctSource;
        public static Context context;

        public MainActivity() {
            context = this;
        }

        protected override void OnCreate(Bundle savedInstanceState) {

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            OpenTK.Toolkit.Init();

            controlHandler = new ControlHandler();
            controlHandler.AddButtonEvents(FindViewById<LinearLayout>(Resource.Id.container));

            FindViewById<Button>(Resource.Id.button_pairBluetooth).Click += (sender, e) => {
                controlHandler.GetAddress(this);
            };

            FindViewById<Button>(Resource.Id.button_unpairBluetooth).Click += (sender, e) => {
                controlHandler.Disconnect();
                FindViewById<TextView>(Resource.Id.textView1).SetText(Resource.String.bluetooth_disconnected);
            };

            // Load references to views of inside the layouts
            cube_view = FindViewById<CubeView>(Resource.Id.cube_view);

            cube_view.Run(60.0f);
        }

        protected override void OnDestroy() {
            ctSource.Cancel();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok) {
                string address = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
                string name = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_NAME);

                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Confirm connection");
                alert.SetMessage("Connect to device '" + name + "' ?");
                alert.SetPositiveButton("OK", (c, ev) => {
                    Task.Run(() => RunOnUiThread(() => {
                        if (!controlHandler.TryConnect(address)) {
                            Android.App.AlertDialog.Builder connectionFailedAlert = new Android.App.AlertDialog.Builder(this);
                            connectionFailedAlert.SetTitle("Error");
                            connectionFailedAlert.SetMessage("Failed to connect to device '" + name + "'");

                            Dialog dialog = connectionFailedAlert.Create();
                            dialog.Show();
                        } else {
                            string str = Resources.GetString(Resource.String.bluetooth_connected) + " " + name;
                            FindViewById<TextView>(Resource.Id.textView1).SetText(str.ToCharArray(), 0, str.Length);
                        }
                    }));
                });

                alert.SetNegativeButton("CANCEL", (c, ev) => { /* do nothing */ });

                Dialog dialog = alert.Create();
                dialog.Show();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

