using Rubinator3000.CubeScan.RelativeElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan.CameraControl {
    public class CameraPreview {

        public System.Windows.Controls.Image Image { get; private set; }

        public WriteableBitmap WriteableBitmap { get; set; }

        public Canvas Canvas { get; set; }

        private Dictionary<string, RelativeCanvasElement> RelativeCanvasChildren { get; set; }

        public CameraPreview(System.Windows.Controls.Image image, Canvas canvas, WriteableBitmap writeableBitmap) {

            Image = image;
            Canvas = canvas;
            WriteableBitmap = writeableBitmap;
            RelativeCanvasChildren = new Dictionary<string, RelativeCanvasElement>();
        }

        public void SetSource(WriteableBitmap writeableBitmap) {

            Application.Current.Dispatcher.Invoke(() => {

                WriteableBitmap = writeableBitmap;
                Image.Source = WriteableBitmap;
            });
        }

        public void DisplayFrame(Bitmap bitmapToDisplay) {

            if (bitmapToDisplay == null || WriteableBitmap == null) {
                return;
            }

            WriteableBitmap.Dispatcher.Invoke(() => {

                // Reserve the backBuffer of previewBitmap for updates
                WriteableBitmap.Lock();

                // Lock "bitmapToDisplay" to be able to fast-copy the bytes to previewBitmap
                BitmapData tempData = bitmapToDisplay.LockBits(new Rectangle(0, 0, bitmapToDisplay.Width, bitmapToDisplay.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // CopyMemory(destPointer, sourcePointer, byteLength to copy);
                NativeMethods.CopyMemory(WriteableBitmap.BackBuffer, tempData.Scan0, WriteableBitmap.BackBufferStride * Convert.ToInt32(WriteableBitmap.Height));


                bitmapToDisplay.UnlockBits(tempData);
                tempData = null;

                // Specify the area of the bitmap, that changed (in this case, the whole writeableBitmap)
                WriteableBitmap.AddDirtyRect(new Int32Rect(0, 0, Convert.ToInt32(WriteableBitmap.Width), Convert.ToInt32(WriteableBitmap.Height)));
                bitmapToDisplay.Dispose();

                // Release the backBuffer of previewBitmap and make it available for display
                WriteableBitmap.Unlock();
            });
        }

        public void SetRelativeCanvasChildren(Dictionary<string, RelativeCanvasElement> relativeCanvasChildren) {

            Canvas.Children.Clear();
            RelativeCanvasChildren.Clear();
            foreach (string name in relativeCanvasChildren.Keys) {

                AddRelativeCanvasElement(name, relativeCanvasChildren[name]);
            }
        }

        public void AddRelativeCanvasElement(string name, RelativeCanvasElement relativeUIElement) {

            if (RelativeCanvasChildren.ContainsKey(name)) {
                RelativeCanvasChildren.Remove(name);
            }
            Canvas.Children.Add(relativeUIElement.GenerateUIElement(Canvas.ActualWidth, Canvas.ActualHeight));
            RelativeCanvasChildren.Add(name, relativeUIElement);
        }

        public void UpdateAllCanvasElements() {

            Canvas.Children.Clear();
            foreach (RelativeCanvasElement rce in RelativeCanvasChildren.Values) {

                Canvas.Children.Add(rce.GenerateUIElement(Canvas.ActualWidth, Canvas.ActualHeight));
            }
        }

        public int GetNumber() {

            int number = -1;
            Application.Current.Dispatcher.Invoke(() => {

                number = Convert.ToInt32(char.GetNumericValue(Image.Name.Last()));
            });
            return number;
        }

        public Dictionary<string, RelativeCanvasElement> GetClonedRelativeCanvasChildren() {

            return RelativeCanvasChildren.ToDictionary(entry => entry.Key, entry => (RelativeCanvasElement)entry.Value.Clone());
        }
    }
}
