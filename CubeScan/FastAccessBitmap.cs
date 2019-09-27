using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan
{
    class FastAccessBitmap
    {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public int Width { get; set; }
        public int Height { get; set; }


        private readonly EventWaitHandle accessingBitsAwaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

        private Bitmap _bitmap;
        private BitmapData _bitmapData;
        private bool _accesseingBits = false;
        private int _bytesPerPixel = -1;

        public FastAccessBitmap(Bitmap bitmap)
        {
            SetBitmap(bitmap);
        }

        public Color ReadPixel(int x, int y, int deltaX, int deltaY)
        {
            if (_bitmapData == null)
            {
                return Color.Empty;
            }

            // This method sums up all the r-, g- and b-values and divides them by the number of counted pixel
            int red = 0, green = 0, blue = 0, pixelCount = 0;

            unsafe
            {
                StartAccessingBits();

                byte* ptrFirstPixel = (byte*)_bitmapData.Scan0;

                for (int i = y; i < y + deltaY; i++)
                {
                    for (int j = x; j < x + deltaX; j++)
                    {
                        int offset = y * _bitmapData.Stride + x * _bytesPerPixel;
                        blue += ptrFirstPixel[offset];
                        green += ptrFirstPixel[offset + 1];
                        red += ptrFirstPixel[offset + 2];

                        pixelCount++;
                    }
                }
                StopAccessingBits();
            }

            Thread.Sleep(1);

            return Color.FromArgb(Convert.ToInt32(red / pixelCount), Convert.ToInt32(green / pixelCount), Convert.ToInt32(blue / pixelCount));
        }

        private void LockBits()
        {
            if (_accesseingBits)
            {
                accessingBitsAwaitHandle.WaitOne();
            }
            StartAccessingBits();
            _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            _bytesPerPixel = Image.GetPixelFormatSize(_bitmap.PixelFormat) / 8;
            StopAccessingBits();
        }

        private void UnlockBits()
        {
            if (_accesseingBits)
            {
                accessingBitsAwaitHandle.WaitOne();
            }
            StartAccessingBits();
            _bitmap.UnlockBits(_bitmapData);
            _bitmapData = null;
            StopAccessingBits();
        }

        private void StartAccessingBits()
        {
            if (_accesseingBits)
            {
                accessingBitsAwaitHandle.WaitOne();
            }
            accessingBitsAwaitHandle.Reset();
            _accesseingBits = true;
        }

        private void StopAccessingBits()
        {
            _accesseingBits = false;
            accessingBitsAwaitHandle.Set();
        }

        public void SetBitmap(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                if (_bitmapData != null) UnlockBits();
                _bitmap = new Bitmap(bitmap);
                bitmap.Dispose();
                Width = _bitmap.Width;
                Height = _bitmap.Height;
                LockBits();
            }
        }

        public bool IsNull()
        {
            bool b = _bitmap == null;
            return b;
        }

        public void DisplayOnWpfImageControl(WriteableBitmap writeableBitmap)
        {
            StartAccessingBits();
            Application.Current.Dispatcher.Invoke(() =>
            {
                writeableBitmap.Lock();
                unsafe
                {
                    // CopyMemory(destPointer, sourcePointer, byteLength to copy);
                    CopyMemory(writeableBitmap.BackBuffer, _bitmapData.Scan0, writeableBitmap.BackBufferStride * Height);
                }

                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
                writeableBitmap.Unlock();
            });
            StopAccessingBits();
        }
    }
}
