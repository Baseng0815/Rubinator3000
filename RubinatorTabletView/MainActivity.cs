using Android;
using Android.App;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using OpenTK.Platform.Android;
using RubinatorTabletView.CameraUtility;
using System;
using static Android.Hardware.Camera;

namespace RubinatorTabletView {
    [Obsolete]
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener {

        // Permission-Request codes
        public const int PRCAMERA = 0;

        private RelativeLayout display_area;
        private CubeView cubeView;
        private FrameLayout cameraPreviewLayout;
        private CameraView cameraView;

        private Camera camera;

        protected override void OnCreate(Bundle savedInstanceState) {

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            OpenTK.Toolkit.Init();

            display_area = FindViewById<RelativeLayout>(Resource.Id.display_area);

            cubeView = new CubeView(ApplicationContext);
            display_area.AddView(cubeView);
            cubeView.Run(60.0f);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted) {

                Init_Camera();
            }
            else {

                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Camera }, PRCAMERA);
            }

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
        }

        [Obsolete]
        private void Init_Camera() {

            cameraPreviewLayout = new FrameLayout(ApplicationContext);
            camera = GetCameraObject();
            cameraView = new CameraView(ApplicationContext, camera);
            cameraPreviewLayout.AddView(cameraView);
        }

        private Camera GetCameraObject() {
            Camera cam_obj;

            cam_obj = Camera.Open();

            Parameters parameters = cam_obj.GetParameters();

            parameters.Set("orientation", "landscape");

            return cam_obj;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == PRCAMERA) {

                if (grantResults[0] == Permission.Granted) {

                    Init_Camera();
                }
                else {
                    Toast.MakeText(ApplicationContext, "Camera-Permission Denied\nCannot Initialize Camera", ToastLength.Short);
                }
            }
        }

        public bool OnNavigationItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case Resource.Id.navigation_cube_view:
                    display_area.RemoveAllViews();
                    display_area.AddView(cubeView);
                    return true;
                case Resource.Id.navigation_color_id:
                    display_area.RemoveAllViews();
                    display_area.AddView(cameraPreviewLayout);
                    return true;
                case Resource.Id.navigation_settings:
                    display_area.RemoveAllViews();
                    return true;
            }
            return false;
        }
    }
}

