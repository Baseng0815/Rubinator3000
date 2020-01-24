using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RubinatorCore;

namespace Rubinator3000
{
    /// <summary>
    /// This class contains event handling
    /// </summary>
    public static partial class CubeViewer
    {
        // event handling is not needed when in 2d drawing mode
        public static void DetachInputEvents()
        {
            Log.LogMessage("Input events detached");
            Window.MouseMove -= MouseMoveEvent;
            Window.MouseUp -= MouseButtonUpEvent;
            Window.MouseWheel -= MouseWheelEvent;
        }

        // event handling is needed when in 3d drawing mode
        public static void AttachInputEvents()
        {
            Log.LogMessage("Input events attached");
            Window.MouseMove += MouseMoveEvent;
            Window.MouseUp += MouseButtonUpEvent;
            Window.MouseWheel += MouseWheelEvent;
        }

        private static void WindowResizeEvent(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, Window.Width, Window.Height);
            view.SetSize(Window.Size.Width, Window.Size.Height);
        }

        private static void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            if (OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed)
            {
                if (firstMouse)
                {
                    prevMouseX = e.X;
                    prevMouseY = e.Y;
                    firstMouse = false;
                }
                else
                {
                    int dx = e.X - prevMouseX;
                    int dy = e.Y - prevMouseY;
                    prevMouseX = e.X;
                    prevMouseY = e.Y;

                    var cubeRot = DrawCube.Transformation.Rotation;

                    // X ^= pitch
                    // Y ^= yaw
                    if (cubeRot.X % 360 < 90 || cubeRot.X % 360 < -90)
                        cubeRot.Y += dx * Settings.MouseSensitivity;
                    else
                        cubeRot.Y -= dx * Settings.MouseSensitivity;

                    cubeRot.X += dy * Settings.MouseSensitivity;

                    DrawCube.Transformation.Rotation = cubeRot;
                    Window.Invalidate();
                }
            }
        }

        private static void MouseButtonUpEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                firstMouse = true;
        }

        private static void MouseWheelEvent(object sender, MouseEventArgs args)
        {
            int delta = -args.Delta / SystemInformation.MouseWheelScrollDelta;

            view.ChangeFov(delta * Settings.ScrollSensitivity);
            Window.Invalidate();
        }

        private static void RenderFrameEvent(object sender, EventArgs args)
        {
            // clear the window
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DrawCube.Draw(view);

            // swap back and front buffers
            Window.SwapBuffers();
        }
    }
}
