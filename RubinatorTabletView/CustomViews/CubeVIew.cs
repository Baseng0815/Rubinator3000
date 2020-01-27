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
using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.Android;

namespace RubinatorTabletView {

    class CubeView : AndroidGameView {

        private Shader shader;
        private bool initialized = false;
        private CubeRenderer renderer;

        public CubeView(Context context) : base(context) {

        }

        public CubeView(Context context, Android.Util.IAttributeSet transfer) : base(context, transfer) {

        }

        public CubeView(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) {

        }

        private void Init() {
            // load shaders
            shader = new Shader("Shaders/cubeShader");

            GL.ClearColor(OpenTK.Graphics.Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            renderer = new CubeRenderer();            
        }

        protected override void CreateFrameBuffer() {
            this.ContextRenderingApi = OpenTK.Graphics.GLVersion.ES2;
            base.CreateFrameBuffer();
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            if (!initialized) {
                Init();
                initialized = true;
            }

            Render();
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);
        }

        private void Render() {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);                        

            GL.EnableVertexAttribArray(positionHandle);

            GL.VertexAttribPointer(positionHandle, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), vertices);

            GL.DrawArrays(BeginMode.Triangles, 0, 3);

            SwapBuffers();

            GL.DisableVertexAttribArray(positionHandle);
        }

        private readonly float[] vertices = new float[] {
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            0.0f,  0.5f, 0.0f
        };
    }
}