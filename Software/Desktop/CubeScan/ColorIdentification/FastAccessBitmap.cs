using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Rubinator3000.CubeScan.ColorIdentification {
    class FastAccessBitmap : IDisposable {

        public int Width { get; set; }
        public int Height { get; set; }

        // Stores the bitmap
        private Bitmap _bitmap;

        private const int BYTESPERPIXEL = 3;

        public FastAccessBitmap(Bitmap bitmap = null) {
            SetBitmap(bitmap);
        }

        public Color ReadPixels(int x, int y, int deltaX, int deltaY) {

            //    (x,y)           deltaX
            //      O ------------------------------ O                                ----------
            //      |                                |                              --          --
            //      |                                |                            --              -- 
            //      |                                |                           |                  | 
            //      |                                |                          |      O       O     |
            //      |                                |  deltaY                  |          |         |
            //      |                                |                          |         |__        |
            //      |                                |                           |     _________     |
            //      |                                |                            --   \_______/   --
            //      |                                |                              --          --
            //      O ------------------------------ O                                ----------




            // This method sums up all the r-, g- and b-values in the rectangle and divides them by the number all counted pixels (deltaX*deltaY)
            int red = 0, green = 0, blue = 0, pixelCount = 0;

            unsafe {

                if (_bitmap == null) {
                    return Color.Empty;
                }

                // Lock bits for fast access
                BitmapData _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // Pointer to the first pixel off bitmap
                byte* ptrFirstPixel = (byte*)_bitmapData.Scan0;

                for (int i = y; i < y + deltaY; i++) {

                    for (int j = x; j < x + deltaX; j++) {

                        int offset = i * _bitmapData.Stride + j * BYTESPERPIXEL;
                        blue += ptrFirstPixel[offset];
                        green += ptrFirstPixel[offset + 1];
                        red += ptrFirstPixel[offset + 2];

                        pixelCount++;
                    }
                }

                _bitmap.UnlockBits(_bitmapData);
            }

            return Color.FromArgb(Convert.ToInt32(red / pixelCount), Convert.ToInt32(green / pixelCount), Convert.ToInt32(blue / pixelCount));
        }

        public void SetBitmap(Bitmap bitmap) {

            if (bitmap != null) {

                // "bitmap" gets cloned, so the original can be disposed
                _bitmap = new Bitmap(bitmap);

                Width = _bitmap.Width;
                Height = _bitmap.Height;
            }
        }

        public bool HasValidBitmap() {

            return _bitmap != null;
        }


        // This method may be useful for automatic LED-Brightness
        public byte GetBrightness() {

            if (_bitmap == null) {

                return 1;
            }

            Color c = ReadPixels(0, 0, _bitmap.Width, _bitmap.Height);
            return Convert.ToByte((c.R + c.G + c.B) / (double)3);
        }

        public void Dispose() {

            _bitmap.Dispose();
        }
    }
}
