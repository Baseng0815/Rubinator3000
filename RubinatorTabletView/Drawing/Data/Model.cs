using OpenTK.Graphics.ES30;
using RubinatorCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RubinatorTabletView {
    public class Model {
        private float[] vertices;
        private float[] texCoords;
        private float[] normals;
        private float[] tangents;
        private float[] bitangents;

        private int drawCount;
        private bool useTangentSpace;

        public Model(Vertex[] vertices, bool useTangentSpace = false) {
            this.useTangentSpace = useTangentSpace;
            drawCount = vertices.Length;

            this.vertices = Enumerable.Aggregate(vertices, new List<float>(),
                (l, v) => {
                    l.AddRange(v.Position.ToArray());
                    return l;
                }).ToArray();

            texCoords = Enumerable.Aggregate(vertices, new List<float>(),
                (l, v) => {
                    l.AddRange(v.TexCoord.ToArray());
                    return l;
                }).ToArray();

            normals = Enumerable.Aggregate(vertices, new List<float>(),
                (l, v) => {
                    l.AddRange(v.Normal.ToArray());
                    return l;
                }).ToArray();

            if (useTangentSpace) {
                tangents = Enumerable.Aggregate(vertices, new List<float>(),
                    (l, v) => {
                        l.AddRange(v.Tangent.ToArray());
                        return l;
                    }).ToArray();

                bitangents = Enumerable.Aggregate(vertices, new List<float>(),
                    (l, v) => {
                        l.AddRange(v.Bitangent.ToArray());
                        return l;
                    }).ToArray();
            }
        }

        public void Draw(Shader shader) {
            int positionHandle = shader.GetAttribLocation("position");
            GL.EnableVertexAttribArray(positionHandle);
            GL.VertexAttribPointer(positionHandle, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), vertices);

            int texCoordHandle = shader.GetAttribLocation("texCoord");
            GL.EnableVertexAttribArray(texCoordHandle);
            GL.VertexAttribPointer(texCoordHandle, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), texCoords);

            int normalHandle = shader.GetAttribLocation("normal");
            GL.EnableVertexAttribArray(normalHandle);
            GL.VertexAttribPointer(normalHandle, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), normals);

            int tangentHandle = -1, bitangentHandle = -1;
            if (useTangentSpace) {
                tangentHandle = shader.GetAttribLocation("tangent");
                GL.EnableVertexAttribArray(tangentHandle);
                GL.VertexAttribPointer(tangentHandle, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), tangents);

                bitangentHandle = shader.GetAttribLocation("bitangent");
                GL.EnableVertexAttribArray(bitangentHandle);
                GL.VertexAttribPointer(bitangentHandle, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), bitangents);
            }

            GL.DrawArrays(BeginMode.Triangles, 0, drawCount);

            GL.DisableVertexAttribArray(positionHandle);
            GL.DisableVertexAttribArray(texCoordHandle);
            GL.DisableVertexAttribArray(normalHandle);
            if (useTangentSpace) {
                GL.DisableVertexAttribArray(tangentHandle);
                GL.DisableVertexAttribArray(bitangentHandle);
            }
        }        
    }
}
