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
using OpenTK.Graphics.ES31;
using OpenTK.Platform.Android;

namespace RubinatorTabletView {

    class CubeView : AndroidGameView {

        private bool initialized = false;
        private CubeRenderer renderer;
        private View view;

        private float prevTouchX;
        private float prevTouchY;
        private bool firstTouch = true;

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
            renderer.Transformation.Rotation = new Vector3(45, 0, 0);

            Touch += CubeView_Touch;
        }

        private void CubeView_Touch(object sender, TouchEventArgs e) {
            if ((e.Event.Action & MotionEventActions.Mask) == MotionEventActions.Move) {
                if (firstTouch) {
                    prevTouchX = e.Event.GetX();
                    prevTouchY = e.Event.GetY();
                    firstTouch = false;
                }
                else {
                    float x = e.Event.GetX();
                    float y = e.Event.GetY();

                    float dx = x - prevTouchX;
                    float dy = y - prevTouchY;

                    prevTouchX = x;
                    prevTouchY = y;

                    var cubeRotation = renderer.Transformation.Rotation;

                    // X ^= pitch
                    // Y ^= yaw
                    if (cubeRotation.X < 90 || cubeRotation.X > 270)
                        cubeRotation.Y += dx * Settings.TouchSensitivity;
                    else
                        cubeRotation.Y -= dx * Settings.TouchSensitivity;

                    cubeRotation.X += dy * Settings.TouchSensitivity;
                    cubeRotation.X = (cubeRotation.X + 360) % 360;
                    cubeRotation.Y = (cubeRotation.Y + 360) % 360;


                    renderer.Transformation.Rotation = cubeRotation;
                    Invalidate();
                }
            }
            else if((e.Event.Action & MotionEventActions.Mask) == MotionEventActions.Up) {
                firstTouch = true;
            }
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

        private readonly float[] vertices = new float[] {
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            0.0f,  0.5f, 0.0f
        };
    }
}