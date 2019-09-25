using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;

namespace Rubinator3000
{
    public enum CubeDisplayMode { NONE = 0, FLAT = 1, CUBE = 2 };

    public static class Renderer
    {
        private static readonly Shader cubeShader;
        private static readonly Shader flatShader;

        public static CubeDisplayMode DisplayMode = CubeDisplayMode.CUBE;

        /// <summary>
        /// Static constructors are called on first access
        /// </summary>
        static Renderer()
        {
            
            cubeShader = new Shader("Resources/CubeShader");
            flatShader = new Shader("Resources/FlatShader");

            cubeShader.Bind();

            // texture units to sampler
            for (int i = 0; i < 2; i++)
                cubeShader.Upload(string.Format("texture{0}", i.ToString()), i);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.Black);
        }

        /// <summary>
        /// Renders the cube
        /// Does NOT swap buffers or clears window
        /// </summary>
        public static void Render(View view)
        {
            if (DisplayMode == CubeDisplayMode.CUBE)
            {
                cubeShader.Bind();

                // view and projection matrices
                cubeShader.Upload("viewMatrix", view.ViewMatrix);
                cubeShader.Upload("projectionMatrix", view.ProjectionMatrix);

                DrawCube.Draw(cubeShader);
            } else if (DisplayMode == CubeDisplayMode.FLAT)
            {
                flatShader.Bind();

                DrawFlat.Draw(flatShader);
            }
        }
    }
}
