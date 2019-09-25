using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.CubeScan
{
    class FastAccessBitmap
    {
        private Bitmap bitmap { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private Bitmap pendingBitmap = null;

        private bool readingPixel = false;

        public FastAccessBitmap(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public Color GetPixel(int x, int y)
        {
            // Check if coordinates are out of bounds of the bitmap
            if (x < 0 || x > bitmap.Width || y < 0 || y > bitmap.Height)
            {
                throw new Exception("Coordinates out of bitmap");
            }
            else
            {
                byte blue = 0, green = 0, red = 0;

                readingPixel = true;

                unsafe
                {
                    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                    int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                    int heightInPixels = bitmapData.Height;
                    int widthInBytes = bitmapData.Width * bytesPerPixel;
                    byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);

                    blue = currentLine[x * bytesPerPixel];
                    green = currentLine[x * bytesPerPixel + 1];
                    red = currentLine[x * bytesPerPixel + 2];

                    bitmap.UnlockBits(bitmapData);
                }

                readingPixel = false;

                if (pendingBitmap != null)
                {
                    SetBitmap(pendingBitmap);
                    pendingBitmap = null;
                }

                return Color.FromArgb(red, green, blue);
            }
        }

        public void SetBitmap(Bitmap bitmap)
        {
            if (readingPixel)
            {
                pendingBitmap = bitmap;
            }
            else
            {
                this.bitmap = bitmap;
                Width = bitmap.Width;
                Height = bitmap.Height;
            }
        }

        public Bitmap GetBitmap()
        {
            return bitmap;
        }
    }
}
