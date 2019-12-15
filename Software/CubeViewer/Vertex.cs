using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Rubinator3000
{
    public struct Vertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 TexCoord { get; set; }
        public Vector3 Tangent { get; set; }
        public Vector3 Bitangent { get; set; }

        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 tangent, Vector3 bitangent)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
            Tangent = tangent;
            Bitangent = bitangent;
        }
    }
}
