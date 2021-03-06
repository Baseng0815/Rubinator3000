﻿using OpenTK.Graphics.ES31;
using RubinatorCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RubinatorTabletView {
    public class Model {       
        public  readonly int DrawCount;        
        private int vao;
        private int vbo;
        private int ebo;

        public Model(Vertex[] vertices, bool useTangentSpace = false, uint[] indices = null) {
            GL.GenVertexArrays(1, out vao);

            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Count() * Marshal.SizeOf(vertices[0])), vertices, BufferUsage.StaticDraw);

            // only if indexed drawing is used
            if (indices != null) {
                GL.GenBuffers(1, out ebo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(vertices.Count() * Marshal.SizeOf(vertices[0])), indices, BufferUsage.StaticDraw);

                DrawCount = indices.Length;
            }
            else DrawCount = vertices.Length;

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            if (useTangentSpace) {
                GL.EnableVertexAttribArray(3);
                GL.EnableVertexAttribArray(4);
            }

            int offset = 0;

            // positions
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(vertices[0]), offset);
            offset += Marshal.SizeOf(vertices[0].Position);

            // tex coord
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf(vertices[0]), offset);
        }

        public void BindVao() {
            GL.BindVertexArray(vao);
        }
    }
}
