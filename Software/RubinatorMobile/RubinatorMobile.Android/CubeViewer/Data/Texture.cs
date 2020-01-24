using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.ES30;
using RubinatorMobile.Droid;
using RubinatorCore;
using Android.App;
using Android.Graphics;
using System.IO;

namespace RubinatorMobile.Droid {
    public class Texture {
        private readonly int texture;

        public Texture(string filePath, Activity activity) {
            Log.LogMessage(string.Format("Loading texture {0}.", filePath));

            Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeStream(activity.Assets.Open(filePath));

            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            //BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
            //    OpenTK.Graphics.ES30.PixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0,
                OpenTK.Graphics.ES30.PixelFormat.Rgba, PixelType.UnsignedByte, bitmap.LockPixels());

            bitmap.UnlockPixels();

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
