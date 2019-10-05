using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using Rubinator3000.Properties;

namespace Rubinator3000
{

    public static partial class CubeViewer
    {
        // (Window) GLControl and fullscreen
        public static readonly GLControl Window;

        private static View view;

        // mouse delta
        private static bool firstMouse = true;
        private static int prevMouseX, prevMouseY;

        /// <summary>
        /// Initialize CubeViewer
        /// </summary>
        static CubeViewer()
        {
            OpenTK.Toolkit.Init();
            Log.LogStuff("OpenTK initialized.");

            Vector3[] renderColors = new Vector3[]
            {
                new Vector3(255, 165, 0),
                new Vector3(255, 255, 255),
                new Vector3(0, 255, 0),
                new Vector3(255, 255, 0),
                new Vector3(255, 0, 0),
                new Vector3(0, 0, 255)
            };

            for (int i = 0; i < renderColors.Length; i++)
                renderColors[i] /= 255f;

            DrawCube.Init(renderColors);
            Log.LogStuff("DrawCube initialized.");

            Window = new GLControl();
            Window.VSync = false;

            view = new View(Window.Width, Window.Height,
                Settings.CameraFov, Settings.CameraDistance);

            Log.LogStuff("Window and view initialized.");

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.Black);

            // add events to window
            Window.Resize += WindowResizeEvent;
            Window.Paint += RenderFrameEvent;
            AttachInputEvents();
        }
    }
}
