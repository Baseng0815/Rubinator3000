using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using OpenTK.Graphics.ES30;
using OpenTK;
using System.Drawing;
using RubinatorCore;
using Xamarin.Forms;

namespace RubinatorMobile.Droid
{

    public static partial class CubeViewer
    {        

        public static View View;

        // mouse delta
        private static bool firstMouse = true;
        private static int prevMouseX, prevMouseY;

        /// <summary>
        /// Initialize CubeViewer
        /// </summary>
        static CubeViewer() {
            OpenTK.Toolkit.Init();

            Log.LogMessage("OpenTK initialized.");

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

            Log.LogMessage("DrawCube initialized.");            

            DrawCube.Init(renderColors);

            View = new View(((MainPage)Application.Current.MainPage).GLViewWidth, ((MainPage)Application.Current.MainPage).GLViewHeight,
                Settings.CameraFov, Settings.CameraDistance);

            Log.LogMessage("Window and view initialized.");

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(System.Drawing.Color.Black);

            // add events to window            
            //AttachInputEvents();
        }        
    }
}
