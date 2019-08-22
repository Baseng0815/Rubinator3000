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
    public static class Renderer
    {
        private static readonly Shader cubeShader;

        // textured quad
        private static readonly Shader tqShader;

        // colored quad
        private static readonly Shader cqShader;

        /// <summary>
        /// Static constructors are called on first access
        /// </summary>
        static Renderer()
        {
            
            cubeShader = new Shader("Resources/CubeShader");
            tqShader = new Shader("Resources/TQShader");

            cubeShader.Bind();

            // texture units to sampler
            for (int i = 0; i < 2; i++)
                cubeShader.Upload("texture" + i.ToString(), i);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.Black);
        }

        /// <summary>
        /// Renders the cube
        /// Does NOT swap buffers or clears window
        /// </summary>
        public static void Render(View view)
        {
            cubeShader.Bind();

            // view and projection matrices
            cubeShader.Upload("viewMatrix", view.ViewMatrix);
            cubeShader.Upload("projectionMatrix", view.ProjectionMatrix);

            DrawCube.Draw(cubeShader);
        }
    }
}
