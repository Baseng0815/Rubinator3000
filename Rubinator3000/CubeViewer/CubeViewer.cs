using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Input;
using System.Drawing;

namespace Rubinator3000
{
    public partial class CubeViewer
    {
        // window and fullscreen
        private GameWindow window;
        private bool isFullscreen = false;
        private Rectangle prevState;

        private View view;

        /// <summary>
        /// Initialize CubeViewer
        /// </summary>
        public CubeViewer()
        {
            window = new GameWindow(Settings.WindowWidth,Settings.WindowHeight);
            window.VSync = VSyncMode.Off;

            view = new View(Settings.WindowWidth, Settings.WindowHeight, 
                Settings.CameraFov, Settings.CameraDistance);

            // add events to window
            window.Resize += this.WindowResizeEvent;
            window.MouseMove += this.MouseMoveEvent;
            window.KeyDown += this.KeyDownEvent;
            window.RenderFrame += this.RenderFrameEvent;
            window.MouseWheel += this.MouseWheelEvent;
        }

        /// <summary>
        /// Enters the event and drawing loop
        /// </summary>
        public void Run()
        {
            window.Run(30, 120);
            while (!window.IsExiting)
                window.ProcessEvents();
        }

    }
}
