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
    class CameraIdView : SurfaceView, ISurfaceHolderCallback, IPreviewCallback {

        private static Paint paint = new Paint();

        public Android.Hardware.Camera Camera { get; set; }

        public Size PreviewSize { get; set; }

        public byte[] BytesData { get; set; }

        private Rect[] readSquares;

        public CameraIdView(Context context, Android.Hardware.Camera camera) : base(context) {

            Camera = camera;
            Holder.AddCallback(this);
            Holder.SetType(SurfaceType.PushBuffers);
        }

        protected override void OnDraw(Canvas canvas) {
            base.OnDraw(canvas);

            paint.StrokeWidth = 8;
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = Color.Black;

            int centerX = Convert.ToInt32(PreviewSize.Width / 2);
            int centerY = Convert.ToInt32(PreviewSize.Height / 2);
            int rectRadius = Convert.ToInt32(PreviewSize.Height / 10);
            int offset = rectRadius * 3;

            readSquares = new Rect[9];

            for (int i = 0; i < 9; i++) {

                // Temp Center X, Y
                int tcx = centerX + (((i % 3) - 1) * offset);
                int tcy = centerY + (((i / 3) - 1) * offset);
                readSquares[i] = new Rect(tcx - rectRadius, tcy - rectRadius, tcx + rectRadius, tcy + rectRadius);
                canvas.DrawRect(readSquares[i], paint);
            }
        }

        #region SurfaceEvents

        public void SurfaceCreated(ISurfaceHolder holder) {

            //Context.GetSystemService(Context.WindowService);
            Camera.SetPreviewDisplay(Holder);

            Camera.SetPreviewCallback(this);

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

        public void OnPreviewFrame(byte[] data, Android.Hardware.Camera camera) {

            BytesData = data;
        }

        #endregion
    }
}