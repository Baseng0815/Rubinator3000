using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan {
    class FastAccessBitmap {

        public int Width { get; set; }
        public int Height { get; set; }

        // Stores the bitmap
        private Bitmap _bitmap;

        // Holds the Locked data of _bitmap
        private BitmapData _bitmapData;
        private byte[] rgbValues;

        private int _bytesPerPixel = 3;

        public FastAccessBitmap(Bitmap bitmap) {
            SetBitmap(bitmap);
        }

        public Color ReadPixel(int x, int y, int deltaX, int deltaY) {

            // This method sums up all the r-, g- and b-values and divides them by the number of counted pixel
            int red = 0, green = 0, blue = 0, pixelCount = 0;

            unsafe {

                if (_bitmap == null) {
                    return Color.Empty;
                }

                _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // Pointer to the first pixel off bitmap
                IntPtr ptrFirstPixel = _bitmapData.Scan0;

                int totalBytes = Math.Abs(_bitmapData.Stride) * _bitmap.Height;
                rgbValues = new byte[totalBytes];

                // Copy all pixels to "rgbValues"
                Marshal.Copy(ptrFirstPixel, rgbValues, 0, totalBytes);

                for (int i = y; i < y + deltaY; i++) {
                    for (int j = x; j < x + deltaX; j++) {
                        int offset = y * _bitmapData.Stride + x * _bytesPerPixel;
                        blue += rgbValues[offset];
                        green += rgbValues[offset + 1];
                        red += rgbValues[offset + 2];

                        pixelCount++;
                    }
                }

                // Copy all pixels back to bitmapData
                Marshal.Copy(rgbValues, 0, ptrFirstPixel, totalBytes);

                Thread.Sleep(10);

                _bitmap.UnlockBits(_bitmapData);
            }

            return Color.FromArgb(Convert.ToInt32(red / pixelCount), Convert.ToInt32(green / pixelCount), Convert.ToInt32(blue / pixelCount));
        }

        public void SetBitmap(Bitmap bitmap) {

            if (bitmap != null) {

                _bitmap = new Bitmap(bitmap);
                Width = _bitmap.Width;
                Height = _bitmap.Height;
            }
        }

        public bool BitmapIsValid() {

            return _bitmap != null;
        }
    }
}
