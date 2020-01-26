using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using RubinatorMobile;
using Xamarin.Forms;

using OpenTK.Graphics.ES30;
using Android.Content.Res;
using OpenTK.Graphics;

[assembly: Dependency(typeof(RubinatorMobile.Droid.OpenGLViewSharedCodeService))]
namespace RubinatorMobile.Droid {
    class OpenGLViewSharedCodeService : IOpenGLViewSharedCodeService {
        private DrawCube drawCube;

        private GraphicsContext context;

        private Shader cubeShader, flatShader;

        public void Init() {
            drawCube = new DrawCube();

            cubeShader = LoadShader("Shaders/CubeShader");
            flatShader = LoadShader("Shaders/FlatShader");

            OpenTK.Toolkit.Init();

            GL.ClearColor(Color.Black);            
        }

        public void OnDisplay(Rectangle r) {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);                                   

            int vbo;
            GL.GenBuffers(1, out vbo);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * vertices.Length), vertices, BufferUsage.StaticDraw);

            GL.DrawArrays(BeginMode.Triangles, 0, 3);            
        }

        private static readonly float[] vertices = {
            -0.5f, -0.5f, 1.0f,
             0.5f, -0.5f, 1.0f,
             0.0f,  0.5f, 1.0f
        };

        private Shader LoadShader(string filename) {
            AssetManager assets = Android.App.Application.Context.Assets;

            string vertShaderSource, fragShaderSource;
            using (StreamReader reader = new StreamReader(assets.Open(filename + ".vert"))) {
                vertShaderSource = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(assets.Open(filename + ".frag"))) {
                fragShaderSource = reader.ReadToEnd();
            }

            return new Shader(vertShaderSource, fragShaderSource);
        }
    }
}
