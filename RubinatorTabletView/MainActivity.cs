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
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener {

        private RelativeLayout display_area;

        private RelativeLayout layout_correction;
        private LinearLayout layout_cube_view;
        private RelativeLayout layout_settings;

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

            display_area = FindViewById<RelativeLayout>(Resource.Id.display_area);

            // Load layouts
            layout_correction = (RelativeLayout)LayoutInflater.Inflate(Resource.Layout.layout_correction, null);
            layout_cube_view = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.layout_cube_view, null);
            layout_settings = (RelativeLayout)LayoutInflater.Inflate(Resource.Layout.layout_settings, null);

            ControlHandler.AddButtonEvents(layout_cube_view);

            layout_cube_view.FindViewById<Button>(Resource.Id.button_pairBluetooth).Click += (sender, e) => {
                ControlHandler.GetAddress(this);
            };

            // Load references to views of inside the layouts
            cube_view = layout_cube_view.FindViewById<CubeView>(Resource.Id.cube_view);

            display_area.AddView(layout_cube_view);
            cube_view.Run(60.0f);

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
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
                        if (!ControlHandler.TryConnect(address)) {
                            Android.App.AlertDialog.Builder connectionFailedAlert = new Android.App.AlertDialog.Builder(this);
                            connectionFailedAlert.SetTitle("Error");
                            connectionFailedAlert.SetMessage("Failed to connect to device '" + name + "'");

                            Dialog dialog = connectionFailedAlert.Create();
                            dialog.Show();
                        } else {
                            layout_cube_view.FindViewById<TextView>(Resource.Id.textView1).SetText(Resource.String.bluetooth_connected);
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

        public bool OnNavigationItemSelected(IMenuItem item) {

            display_area.RemoveAllViews();

            switch (item.ItemId) {
                case Resource.Id.navigation_cube_view:
                    display_area.AddView(layout_cube_view);
                    return true;
                case Resource.Id.navigation_correction:
                    display_area.AddView(layout_correction);
                    return true;
                case Resource.Id.navigation_settings:
                    display_area.AddView(layout_settings);
                    return true;
            }
            return false;
        }
    }
}

