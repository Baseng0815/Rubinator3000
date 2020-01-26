using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Runtime;
using Android.Views;
using static Android.Hardware.Camera;

namespace RubinatorTabletView.CameraUtility {

    [Obsolete]
    class CameraView : SurfaceView, ISurfaceHolderCallback {

        public Android.Hardware.Camera Camera { get; set; }

        public Size PreviewSize { get; set; }

        public CameraView(Context context, Android.Hardware.Camera camera) : base(context) {

            Camera = camera;
            Holder.AddCallback(this);
            Holder.SetType(SurfaceType.PushBuffers);
        }

        public void SurfaceCreated(ISurfaceHolder holder) {

            Context.GetSystemService(Context.WindowService);
            Camera.SetPreviewDisplay(Holder);

            SetWillNotDraw(false);
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height) {

            IList<Size> cameraSizes = Camera.GetParameters().SupportedPreviewSizes;

            // Get biggest possible camera-preview-size
            PreviewSize = cameraSizes[0];
            foreach (Size s in cameraSizes) {

                if ((s.Width * s.Height) > (PreviewSize.Width * PreviewSize.Height)) {
                    PreviewSize = s;
                }
            }

            Parameters camParams = Camera.GetParameters();
            camParams.SetPreviewSize(PreviewSize.Width, PreviewSize.Height);
            Camera.SetParameters(camParams);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder) {

            Camera.StopPreview();
        }


    }
}