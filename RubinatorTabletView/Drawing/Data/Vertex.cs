using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace RubinatorTabletView
{
    public struct Vertex
    {
        public Vector3 Position { get; set; }
        public Vector2 TexCoord { get; set; }

        public Vertex(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }
    }
}
