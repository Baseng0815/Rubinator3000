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
using Rubinator3000.Properties;

namespace Rubinator3000
{
    /// <summary>
    /// This class contains event handling
    /// </summary>
    public static partial class CubeViewer
    {
        /// <summary>
        /// This function is called whenever the window size changes.
        /// It will be used to recalculate the projection matrix and to resize the viewport
        /// </summary>
        private static void WindowResizeEvent(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, Window.Width, Window.Height);
            view.SetSize(Window.Size.Width, Window.Size.Height);
        }

        /// <summary>
        /// This function is called whenever the mouse is moved.
        /// It will be used to handle mouse motion, e.g. to rotate the cube view
        /// </summary>
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

        /// <summary>
        /// Mouse wheel event, used for zooming
        /// </summary>
        private static void MouseWheelEvent(object sender, MouseEventArgs args)
        {
            view.ChangeFov(-args.Delta * Settings.ScrollSensitivity);
            Window.Invalidate();
        }

        // map keyboard to face moves
        private static Dictionary<Keys, CubeFace> keyFaceMappings = new Dictionary<Keys, CubeFace>()
        {
            { Keys.F, CubeFace.FRONT },
            { Keys.U, CubeFace.UP },
            { Keys.B, CubeFace.BACK },
            { Keys.R, CubeFace.RIGHT },
            { Keys.L, CubeFace.LEFT },
            { Keys.D, CubeFace.DOWN }
        };

        /// <summary>
        /// This function is called whenever a key is pressed.
        /// It will be used to handle input, e.g. to toggle the fullscreen
        /// </summary>
        private static void KeyDownEvent(object sender, KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.Escape:
                    // @TODO
                    // EXIT PROGRAM
                    break;
            }

            bool isPrime = args.Shift;
            if (keyFaceMappings.ContainsKey(args.KeyCode))
            {
                // @TODO
                // Implement manual move
                //((MainWindow)Application.Current.MainWindow).Cube.DoMove(new Move(keyFaceMappings[args.KeyCode], isPrime));
            }
        }

        /// <summary>
        /// This function is called whenever a frame render is due
        /// </summary>
        private static void RenderFrameEvent(object sender, EventArgs args)
        {
            // clear the window
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Renderer.Render(view);

            // swap back and front buffers
            Window.SwapBuffers();
        }
    }
}
