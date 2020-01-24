using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.IO;
using RubinatorCore;

namespace Rubinator3000
{
    public class Shader
    {
        public int ShaderProgram { get; }

        private readonly int vs, fs;

        private int CompileShader(string file, ShaderType type)
        {
            int shader = GL.CreateShader(type);
            string shaderSource = File.ReadAllText(file);
            Log.LogMessage(string.Format("Shader type {0} source: {1}", type, shaderSource));
            GL.ShaderSource(shader, File.ReadAllText(file));
            GL.CompileShader(shader);
            var info = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrWhiteSpace(info))
                Console.WriteLine(info);

            return shader;
        }

        public Shader(string file)
        {
            Log.LogMessage(string.Format("Loading shader {0}.", file));

            ShaderProgram = GL.CreateProgram();

            vs = CompileShader(file + ".vert", ShaderType.VertexShader);
            fs = CompileShader(file + ".frag", ShaderType.FragmentShader);

            GL.AttachShader(ShaderProgram, vs);
            GL.AttachShader(ShaderProgram, fs);
            GL.LinkProgram(ShaderProgram);

            Log.LogMessage("Shaders attached and program linked.");

            var info = GL.GetProgramInfoLog(ShaderProgram);
            if (!string.IsNullOrWhiteSpace(info))
                Log.LogMessage(info);

            GL.DetachShader(ShaderProgram, vs);
            GL.DetachShader(ShaderProgram, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
        }

        /// <summary>
        /// Uploads values to the shader program
        /// </summary>
        public void Upload(string location, Matrix4 mat)
        {
            int loc = GL.GetUniformLocation(ShaderProgram, location);
            GL.UniformMatrix4(loc, false, ref mat);
        }

        /// <summary>
        /// Uploads values to the shader program
        /// </summary>
        public void Upload(string location, int val)
        {
            int loc = GL.GetUniformLocation(ShaderProgram, location);
            GL.Uniform1(loc, val);
        }

        /// <summary>
        /// Uploads values to the shader program
        /// </summary>
        public void Upload(string location, Vector3 vec)
        {
            int loc = GL.GetUniformLocation(ShaderProgram, location);
            GL.Uniform3(loc, ref vec);
        }

        /// <summary>
        /// Binds the shader program to be used in the pipeline
        /// </summary>
        public void Bind()
        {
            GL.UseProgram(ShaderProgram);
        }

    }
}
