using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    public class Model
    {
        private int vao, vbo, ebo;
        public readonly int DrawCount;

        public Model(Vertex[] vertices, bool useTangentSpace = false, uint[] indices = null)
        {
            GL.GenVertexArrays(1, out vao);

            // bind vao so all subsequent operations take place on it
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count() * Marshal.SizeOf(vertices[0]), vertices, BufferUsageHint.StaticDraw);

            // only if indexed drawing is used
            if (indices != null)
            {
                GL.GenBuffers(1, out ebo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count() * Marshal.SizeOf(indices[0]), indices, BufferUsageHint.StaticDraw);

                DrawCount = indices.Length;
            }
            else DrawCount = vertices.Length;

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            if (useTangentSpace)
            {
                GL.EnableVertexAttribArray(3);
                GL.EnableVertexAttribArray(4);
            }

            int offset = 0;

            // positions
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(vertices[0]), offset);
            offset += Marshal.SizeOf(vertices[0].Position);

            // normal
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(vertices[0]), offset);
            offset += Marshal.SizeOf(vertices[0].Normal);

            // tex coord
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf(vertices[0]), offset);
            offset += Marshal.SizeOf(vertices[0].TexCoord);

            if (useTangentSpace)
            {
                // tangent
                GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(vertices[0]), offset);
                offset += Marshal.SizeOf(vertices[0].Tangent);

                // bitangent
                GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(vertices[0]), offset);
            }
        }

        public void BindVao()
        {
            GL.BindVertexArray(vao);
        }
    }
}
