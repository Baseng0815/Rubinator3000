using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Rubinator3000.Properties;

namespace Rubinator3000
{
    /// <summary>
    /// This class contains event handling
    /// </summary>
    public partial class CubeViewer
    {
        /// <summary>
        /// This function is called whenever the window size changes.
        /// It will be used to recalculate the projection matrix and to resize the viewport
        /// </summary>
        private void WindowResizeEvent(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);
            view.SetSize(window.Size.Width, window.Size.Height);
        }

        /// <summary>
        /// This function is called whenever the mouse is moved.
        /// It will be used to handle mouse motion, e.g. to rotate the cube view
        /// </summary>
        private void MouseMoveEvent(object sender, MouseMoveEventArgs e)
        {
            if (Mouse.GetState().IsButtonDown(MouseButton.Left))
            {
                var cubeRot = DrawCube.Transformation.Rotation;

                // X ^= pitch
                // Y ^= yaw
                if (cubeRot.X % 360 < 90 || cubeRot.X % 360 < -90)
                    cubeRot.Y += e.XDelta * Settings.MouseSensitivity;
                else
                    cubeRot.Y -= e.XDelta * Settings.MouseSensitivity;

                cubeRot.X += e.YDelta * Settings.MouseSensitivity;

                DrawCube.Transformation.Rotation = cubeRot;
            }
        }

        /// <summary>
        /// Mouse wheel event, used for zooming
        /// </summary>
        private void MouseWheelEvent(object sender, MouseWheelEventArgs args)
        {
            view.ChangeFov(-args.DeltaPrecise);
        }

        // map keyboard to face moves
        private static Dictionary<Key, CubeFace> keyFaceMappings = new Dictionary<Key, CubeFace>()
        {
            { Key.F, CubeFace.FRONT },
            { Key.U, CubeFace.UP },
            { Key.B, CubeFace.BACK },
            { Key.R, CubeFace.RIGHT },
            { Key.L, CubeFace.LEFT },
            { Key.D, CubeFace.DOWN }
        };

        /// <summary>
        /// This function is called whenever a key is pressed.
        /// It will be used to handle input, e.g. to toggle the fullscreen
        /// </summary>
        private void KeyDownEvent(object sender, KeyboardKeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    window.Close();
                    break;
                case Key.V:
                {
                    if (!isFullscreen)
                    {
                        prevState.Width = window.Size.Width;
                        prevState.Height = window.Size.Height;
                        prevState.X = window.X;
                        prevState.Y = window.Y;

                        window.Size = new System.Drawing.Size(DisplayDevice.Default.Width, DisplayDevice.Default.Height);
                        window.X = 0;
                        window.Y = 0;

                        isFullscreen = true;
                    }
                    else
                    {
                        window.Size = new System.Drawing.Size(prevState.Width, prevState.Height);
                        window.X = prevState.X;
                        window.Y = prevState.Y;

                        isFullscreen = false;
                    }
                    break;
                }
            }

            bool isPrime = Keyboard.GetState().IsKeyDown(Key.LShift);
            if (keyFaceMappings.ContainsKey(args.Key))
            {
                ((MainWindow)Application.Current.MainWindow).Cube.DoMove(new Move(keyFaceMappings[args.Key], isPrime));

            }
        }

        /// <summary>
        /// This function is called whenever a frame render is due
        /// </summary>
        private void RenderFrameEvent(object sender, EventArgs args)
        {
            FrameEventArgs e = (FrameEventArgs)args;

            // clear the window
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Renderer.Render(view);

            // swap back and front buffers
            window.SwapBuffers();
        }

        

    }
}
