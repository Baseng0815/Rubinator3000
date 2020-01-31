using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.ES31;
using RubinatorCore;
using Android.App;
using Android.Graphics;

namespace RubinatorTabletView {
    public class Texture {
        private readonly int texture;

        public Texture(string filePath) {

            Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeStream(Application.Context.Assets.Open(filePath));

            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0,
                OpenTK.Graphics.ES31.PixelFormat.Rgba, PixelType.UnsignedByte, bitmap.LockPixels());            

            bitmap.UnlockPixels();
            bitmap.Dispose();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        /// <summary>
        /// Bind the texture so the shader uses it
        /// </summary>
        public void Bind(int textureUnit) {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, this.texture);
        }
    }
}
