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
            LoadedTextures.Add("cubeBlendFrame", new Texture("Resources/Textures/BlendFrame.png"));
            LoadedTextures.Add("cubeBumpMap", new Texture("Resources/Textures/NormalMap1.png"));
            Log.LogMessage("Texture loading finished.");

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

            Vector3 normal = new Vector3(0, 1, 0);

            // calculate tangent/bitangent vectors of both triangles
            // taken from https://learnopengl.com/Advanced-Lighting/Normal-Mapping
            Vector3 tangent1, bitangent1;
            Vector3 tangent2, bitangent2;

            // triangle 1
            // ----------
            Vector3 edge1 = positions[1] - positions[0];
            Vector3 edge2 = positions[2] - positions[0];
            Vector2 deltaUV1 = texCoords[1] - texCoords[0];
            Vector2 deltaUV2 = texCoords[2] - texCoords[0];

            float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

            tangent1.X = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
            tangent1.Y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
            tangent1.Z = f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z);
            tangent1 = tangent1.Normalized();

            bitangent1.X = f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X);
            bitangent1.Y = f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y);
            bitangent1.Z = f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z);
            bitangent1 = bitangent1.Normalized();

            // triangle 2
            // ----------
            edge1 = positions[2] - positions[0];
            edge2 = positions[3] - positions[0];
            deltaUV1 = texCoords[2] - texCoords[0];
            deltaUV2 = texCoords[3] - texCoords[0];

            f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

            tangent2.X = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
            tangent2.Y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
            tangent2.Z = f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z);
            tangent2 = tangent2.Normalized();


            bitangent2.X = f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X);
            bitangent2.Y = f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y);
            bitangent2.Z = f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z);
            bitangent2 = bitangent2.Normalized();

            // indexed drawing is not used
            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3
            };

            Vertex[] vertices = new Vertex[6];
            for (int i = 0; i < 6; i++)
            {
                if (i < 3)
                    vertices[i] = new Vertex(positions[indices[i]], normal, texCoords[indices[i]], tangent1, bitangent1);
                else
                    vertices[i] = new Vertex(positions[indices[i]], normal, texCoords[indices[i]], tangent2, bitangent2);
            }

            LoadedModels.Add("cubePlane", new Model(vertices, true));
            Log.LogMessage("Model loading finished.");
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
                new Vertex { Position = new Vector3(0, -1, 0), TexCoord = new Vector2(0, 0) }
            };

            LoadedModels.Add("flatPlane", new Model(vertices, false, indices));
            LoadedTextures.Add("flatBlendFrame", new Texture("Resources/Textures/BlendFrameThick.png"));
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
