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

namespace CubeLibrary
{
    /// <summary>
    /// This class contains event handling
    /// </summary>
    public static partial class CubeViewer
    {
        // event handling is not needed when in 2d drawing mode
        public static void DetachInputEvents()
        {
            Log.LogStuff("Input events detached");
            Window.MouseMove -= MouseMoveEvent;
            Window.MouseUp -= MouseButtonUpEvent;
            Window.KeyDown -= KeyDownEvent;
            Window.MouseWheel -= MouseWheelEvent;
        }

        // event handling is needed when in 3d drawing mode
        public static void AttachInputEvents()
        {
            Log.LogStuff("Input events attached");
            Window.MouseMove += MouseMoveEvent;
            Window.MouseUp += MouseButtonUpEvent;
            Window.KeyDown += KeyDownEvent;
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
                        cubeRot.Y += dx * CubeLibrary.MouseSensitivity;
                    else
                        cubeRot.Y -= dx * CubeLibrary.MouseSensitivity;

                    cubeRot.X += dy * CubeLibrary.MouseSensitivity;

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

            view.ChangeFov(delta * CubeLibrary.ScrollSensitivity);
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
