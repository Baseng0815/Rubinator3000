using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    public class DrawFlat
    {
        private static Cube currentState;

        private static Vector3[] renderColors;

        public static TRSTransformation Transformation;

        public static void Init(Vector3[] _renderColors, Cube cube = null)
        {
            if (cube != null)
                SetState(cube);
            else 
                currentState = new Cube();

            renderColors = _renderColors;

            Transformation = new TRSTransformation();
        }

        public static void SetState(Cube cube)
        {
            // deep copy because otherwise, the arrays would refer to the same memory
            currentState = Utility.DeepClone(cube);
            CubeViewer.Window.Invalidate();
        }

        public static void Draw(Shader shader)
        {
            var data = currentState.GetData();

            for (CubeFace face = 0; (int)face < 6; face++)
            {
                for (int tile = 0; tile < 9; tile++)
                {
                    int ind = (int)face * 9 + tile;
                    shader.Upload(string.Format("modelMatrix[{0}]", ind), FlatTransformations.Transformations[(int)face, tile].GetMatrix());

                    CubeColor color = data[(int)face][tile];
                    shader.Upload(string.Format("color[{0}]", ind), renderColors[(int)color]);
                }
            }

            ResourceManager.LoadedModels["flatPlane"].BindVao();
            ResourceManager.LoadedTextures["flatBlendFrame"].Bind(0);

            GL.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (IntPtr)0, 54);
        }
    }
}
