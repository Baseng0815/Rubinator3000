using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenTK;
using OpenTK.Graphics.ES30;
using OpenTK.Platform.Android;

namespace RubinatorTabletView {

    class CubeView : AndroidGameView {

        private bool initialized = false;
        private CubeRenderer renderer;
        private View view;

        private float prevTouchX;
        private float prevTouchY;

        public CubeView(Context context) : base(context) {

        }

        public CubeView(Context context, Android.Util.IAttributeSet transfer) : base(context, transfer) {

        }

        public CubeView(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) {

        }

        private void Init() {
            GL.ClearColor(OpenTK.Graphics.Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            view = new View(Width, Height, Settings.CameraFov, Settings.CameraDistance);            

            Vector3[] renderColors = new Vector3[]
            {
                new Vector3(255, 106, 0),
                new Vector3(255, 255, 255),
                new Vector3(0, 255, 0),
                new Vector3(255, 255, 0),
                new Vector3(255, 0, 0),
                new Vector3(0, 0, 255)
            };

            for (int i = 0; i < renderColors.Length; i++)
                renderColors[i] /= 255f;

            renderer = new CubeRenderer();
            renderer.Init(renderColors);
        }

        protected override void CreateFrameBuffer() {
            this.ContextRenderingApi = OpenTK.Graphics.GLVersion.ES3;
            base.CreateFrameBuffer();
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            if (!initialized) {
                Init();
                initialized = true;
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderer.Draw(view);

            SwapBuffers();         
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);

            if (view != null)
                view.SetSize(Width, Height);
        }

        public override bool OnTouchEvent(MotionEvent e) {
            if (e.Action == MotionEventActions.Down) {
                prevTouchX = e.GetX();
                prevTouchY = e.GetY();
            }
            else if (e.Action == MotionEventActions.Move) {
                float dx = e.GetX() - prevTouchX;
                float dy = e.GetY() - prevTouchY;

                var cubeRotation = renderer.Transformation.Rotation;

                // X ^= pitch
                // Y ^= yaw
                if (cubeRotation.X % 360 < 90 || cubeRotation.X % 360 < -90)
                    cubeRotation.Y += dx * Settings.TouchSensitivity;
                else
                    cubeRotation.Y -= dx * Settings.TouchSensitivity;

                cubeRotation.X += dy * Settings.TouchSensitivity;

                Invalidate();
            }

            return base.OnTouchEvent(e);
        }

        private readonly float[] vertices = new float[] {
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            0.0f,  0.5f, 0.0f
        };
    }
}