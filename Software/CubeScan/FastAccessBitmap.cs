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

        private int _bytesPerPixel = 3;

        public FastAccessBitmap(Bitmap bitmap) {
            SetBitmap(bitmap);
        }

        public Color ReadPixel(int x, int y, int deltaX, int deltaY) {

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
                _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // Pointer to the first pixel off bitmap
                byte* ptrFirstPixel = (byte*)_bitmapData.Scan0;

                for (int i = y; i < y + deltaY; i++) {
                    for (int j = x; j < x + deltaX; j++) {
                        int offset = y * _bitmapData.Stride + x * _bytesPerPixel;
                        blue += ptrFirstPixel[offset];
                        green += ptrFirstPixel[offset + 1];
                        red += ptrFirstPixel[offset + 2];

                        pixelCount++;
                    }
                }

                //Thread.Sleep(10); Maybe not necessary 

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
    }
}
