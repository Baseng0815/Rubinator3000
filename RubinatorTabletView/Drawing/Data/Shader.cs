using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenTK;
using OpenTK.Graphics.ES31;

namespace RubinatorTabletView {
    public class Shader {
        private int program;

        public Shader(string path) {
            program = GL.CreateProgram();

            int vertShader = LoadShader(path + ".vert", ShaderType.VertexShader);
            int fragShader = LoadShader(path + ".frag", ShaderType.FragmentShader);

            GL.AttachShader(program, vertShader);
            GL.AttachShader(program, fragShader);

            GL.LinkProgram(program);

            GL.DetachShader(program, vertShader);
            GL.DetachShader(program, fragShader);

            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);
        }

        private int LoadShader(string filename, ShaderType type) {
            int shader = GL.CreateShader(type);

            string shaderSource;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(Application.Context.Assets.Open(filename))) {
                shaderSource = reader.ReadToEnd();
            }

            GL.ShaderSource(shader, shaderSource);
            GL.CompileShader(shader);

            string infoLog = GL.GetShaderInfoLog(shader);
            if (infoLog != ""){
                System.Diagnostics.Debug.WriteLine(infoLog);
            }

            return shader;
        }

        public void Bind() {
            GL.UseProgram(program);
        }

        public int GetAttribLocation(string name) => GL.GetAttribLocation(program, name);
    
        public void Upload(string location, Matrix4 matrix) {
            int handle = GL.GetUniformLocation(program, location);
            GL.UniformMatrix4(handle, false, ref matrix);
        }

        public void Upload(string location, Vector3 vec) {
            int handle = GL.GetUniformLocation(program, location);
            GL.Uniform3(handle, ref vec);
        }

        public void Upload(string location, Vector2 vec) {
            int handle = GL.GetUniformLocation(program, location);
            GL.Uniform2(handle, ref vec);
        }

        public void Upload(string location, float value) {
            int handle = GL.GetUniformLocation(program, location);
            GL.Uniform1(handle, value);            
        }

        public void UpoadSampler(string location, float value) {
            int handle = GL.GetUniformLocation(program, location);
            GL.Uniform1(handle, (int)value);
            var error = GL.GetErrorCode(); 
        }
    }
}