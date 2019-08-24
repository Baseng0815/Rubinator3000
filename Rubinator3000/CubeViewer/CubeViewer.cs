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

        public static CubeDisplayMode DisplayMode;

        /// <summary>
        /// Initialize CubeViewer
        /// </summary>
        static CubeViewer()
        {
            OpenTK.Toolkit.Init();

            DrawCube.Init(new Vector3[]
            {
                new Vector3(255, 165, 0),
                new Vector3(255, 255, 255),
                new Vector3(0, 255, 0),
                new Vector3(255, 255, 0),
                new Vector3(255, 0, 0),
                new Vector3(0, 0, 255)
            });

            Window = new GLControl();
            Window.VSync = false;

            view = new View(Window.Width, Window.Height,
                Settings.CameraFov, Settings.CameraDistance);

            // add events to window
            Window.Resize += WindowResizeEvent;
            Window.MouseMove += MouseMoveEvent;
            Window.MouseUp += MouseButtonUpEvent;
            Window.KeyDown += KeyDownEvent;
            Window.Paint += RenderFrameEvent;
            Window.MouseWheel += MouseWheelEvent;
        }
    }
}
