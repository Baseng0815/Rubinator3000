using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan {
    class FastAccessBitmap {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public int Width { get; set; }
        public int Height { get; set; }

        // This object is to prevent multiple threads, to access _bitmapData at the same time
        private readonly EventWaitHandle accessingBitsAwaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

        // Stores the bitmap
        private Bitmap _bitmap;

        // Holds the Locked data of _bitmap
        private BitmapData _bitmapData;

        private int _bytesPerPixel = -1;

        public FastAccessBitmap(Bitmap bitmap) {
            SetBitmap(bitmap);
        }

        public Color ReadPixel(int x, int y, int deltaX, int deltaY) {

            // _bitmapData is null, if no bitmap was added yet
            if (_bitmapData == null) {
                return Color.Empty;
            }

            // This method sums up all the r-, g- and b-values and divides them by the number of counted pixel
            int red = 0, green = 0, blue = 0, pixelCount = 0;

            unsafe {

                // Tell the program, that this method is going to access the bits of the bitmap
                StartAccessingBits();

                // This if statement
                if (_bitmapData == null) {
                    return Color.Empty;
                }

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

                // Tell the program, that this method has finished accessing the bits of the bitmap
                EndAccessingBits();
            }


            return Color.FromArgb(Convert.ToInt32(red / pixelCount), Convert.ToInt32(green / pixelCount), Convert.ToInt32(blue / pixelCount));
        }

        private void LockBits() {

            accessingBitsAwaitHandle.WaitOne();
            StartAccessingBits();
            _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            _bytesPerPixel = Image.GetPixelFormatSize(_bitmap.PixelFormat) / 8;
            EndAccessingBits();
        }

        private void UnlockBits() {

            accessingBitsAwaitHandle.WaitOne();
            StartAccessingBits();
            _bitmap.UnlockBits(_bitmapData);
            _bitmapData = null;
            EndAccessingBits();
        }

        private void StartAccessingBits() {

            accessingBitsAwaitHandle.WaitOne();
            accessingBitsAwaitHandle.Reset();
        }

        private void EndAccessingBits() {

            accessingBitsAwaitHandle.Set();
        }

        public void SetBitmap(Bitmap bitmap) {

            // When _bitmap is set, its data gets locked (in _bitmapData) and only unlocked if it is updated by this method
            if (bitmap != null) {
                if (_bitmapData != null) UnlockBits();
                _bitmap = new Bitmap(bitmap);
                bitmap.Dispose();
                Width = _bitmap.Width;
                Height = _bitmap.Height;
                LockBits();
            }
        }

        public bool IsNull() {

            return _bitmap == null;
        }

        public void DisplayOnWpfImageControl(WriteableBitmap writeableBitmap) {

            StartAccessingBits();
            Application.Current.Dispatcher.Invoke(() => {

                // Reserve the backBuffer of writeableBitmap for updates
                writeableBitmap.Lock();
                unsafe {
                    // CopyMemory(destPointer, sourcePointer, byteLength to copy);
                    CopyMemory(writeableBitmap.BackBuffer, _bitmapData.Scan0, writeableBitmap.BackBufferStride * Height);
                }

                // Specify the area of the bitmap, that changed (in this case, the whole bitmap changed)
                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));

                // Release the backBuffer of writeableBitmap and make it available for display
                writeableBitmap.Unlock();
            });
            EndAccessingBits();
        }
    }
}
