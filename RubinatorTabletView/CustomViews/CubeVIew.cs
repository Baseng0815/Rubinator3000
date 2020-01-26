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

        private int vbo;

        public CubeView(Context context) : base(context) {
            Init();
        }

        public CubeView(Context context, Android.Util.IAttributeSet transfer) : base(context, transfer) {
            Init();
        }

        public CubeView(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) {
            Init();
        }

        private void Init() {
            GL.GenBuffers(1, out vbo);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(9 * sizeof(float)), vertices, BufferUsage.StaticDraw);            
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            Render();
        }

        private void Render() {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.DrawArrays(BeginMode.Triangles, 0, 3);

            SwapBuffers();
        }

        private readonly float[] vertices = new float[] {
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            0.0f,  0.5f, 0.0f
        };
    }
}