using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan {
    class CameraPreview {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public System.Windows.Controls.Image Image { get; private set; }

        private WriteableBitmap WriteableBitmap { get; set; }

        private Canvas Canvas { get; set; }

        private Dictionary<string, RelativeCanvasElement> RelativeCanvasChildren { get; set; }

        public CameraPreview(System.Windows.Controls.Image image, Canvas canvas, WriteableBitmap writeableBitmap) {

            Image = image;
            Canvas = canvas;
            WriteableBitmap = writeableBitmap;
            RelativeCanvasChildren = new Dictionary<string, RelativeCanvasElement>();
        }

        public void DisplayFrame(Bitmap bitmapToDisplay) {

            if (bitmapToDisplay == null) {
                return;
            }

            Application.Current.Dispatcher.Invoke(() => {

                // Reserve the backBuffer of previewBitmap for updates
                WriteableBitmap.Lock();

                // Lock "bitmapToDisplay" to be able to fast-copy the bytes to previewBitmap
                BitmapData tempData = bitmapToDisplay.LockBits(new Rectangle(0, 0, bitmapToDisplay.Width, bitmapToDisplay.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // CopyMemory(destPointer, sourcePointer, byteLength to copy);
                CopyMemory(WriteableBitmap.BackBuffer, tempData.Scan0, WriteableBitmap.BackBufferStride * Convert.ToInt32(WriteableBitmap.Height));


                bitmapToDisplay.UnlockBits(tempData);
                tempData = null;

                // Specify the area of the bitmap, that changed (in this case, the wole bitmap)
                WriteableBitmap.AddDirtyRect(new Int32Rect(0, 0, bitmapToDisplay.Width, bitmapToDisplay.Height));

                // Release the backBuffer of previewBitmap and make it available for display
                WriteableBitmap.Unlock();
            });
        }

        public void AddRelativeCanvasElement(string name, RelativeCanvasElement relativeUIElement) {

            Canvas.Children.Add(relativeUIElement.GenerateUIElement(Canvas.ActualWidth, Canvas.ActualHeight));
            RelativeCanvasChildren.Add(name, relativeUIElement);
        }

        public void RemoveRelativeElement(string name) {

            Canvas.Children.Remove(RelativeCanvasChildren[name].GenerateUIElement(Canvas.ActualWidth, Canvas.ActualHeight));
        }

        public void UpdateAllCanvasElements() {

            Canvas.Children.Clear();
            foreach (RelativeCanvasElement rce in RelativeCanvasChildren.Values) {

                Canvas.Children.Add(rce.GenerateUIElement(Canvas.ActualWidth, Canvas.ActualHeight));
            }
        }
    }
}
