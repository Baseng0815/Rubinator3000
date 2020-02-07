using OpenTK;
using RubinatorCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubinatorTabletView
{
    public static class ResourceManager
    {
        public static Dictionary<string, Texture> LoadedTextures;
        public static Dictionary<string, Model> LoadedModels;

        private static void InitCubeData()
        {
            LoadedTextures.Add("cubeBlendFrame", new Texture("Textures/BlendFrame.png"));
            LoadedTextures.Add("cubeBumpMap", new Texture("Textures/NormalMap1.png"));

            // geometry
            Vector3[] positions = new Vector3[]
            {
                new Vector3(0.5f, 0, 0.5f),
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(0.5f, 0, -0.5f)
            };

            Vector2[] texCoords = new Vector2[]
            {
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0)
            };

            // indexed drawing is not used
            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3
            };

            Vertex[] vertices = new Vertex[6];
            for (int i = 0; i < 6; i++)
            {
                if (i < 3)
                    vertices[i] = new Vertex(positions[indices[i]], texCoords[indices[i]]);
                else
                    vertices[i] = new Vertex(positions[indices[i]], texCoords[indices[i]]);
            }

            LoadedModels.Add("cubePlane", new Model(vertices, true));
        }

        private static void InitFlatData()
        {
            uint[] indices = new uint[]
            {
                0, 1, 3, 1, 2, 3
            };

            Vertex[] vertices = new Vertex[]
            {
                new Vertex { Position = new Vector3(0, 0, 0), TexCoord = new Vector2(0, 1) },
                new Vertex { Position = new Vector3(1, 0, 0), TexCoord = new Vector2(1, 1) },
                new Vertex { Position = new Vector3(1, -1, 0), TexCoord = new Vector2(1, 0) },
                new Vertex { Position = new Vector3(0, -1, 0), TexCoord = new Vector2(0, 0) },
                new Vertex { Position = new Vector3(1, 0, 0), TexCoord = new Vector2(1, 1) },
                new Vertex { Position = new Vector3(1, -1, 0), TexCoord = new Vector2(1, 0) },
                new Vertex { Position = new Vector3(0, -1, 0), TexCoord = new Vector2(0, 0) }
            };

            LoadedModels.Add("flatPlane", new Model(vertices, false));
            LoadedTextures.Add("flatBlendFrame", new Texture("Textures/BlendFrameThick.png"));
        }

        static ResourceManager()
        {
            LoadedTextures = new Dictionary<string, Texture>();
            LoadedModels = new Dictionary<string, Model>();

            InitCubeData();
            InitFlatData();
        }
    }
}
