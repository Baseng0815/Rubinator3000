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

        public WebCamControl Occupant { get; set; }

        private List<RelativeCanvasElement> RelativeCanvasChildren { get; set; }

        public CameraPreview(System.Windows.Controls.Image image, Canvas canvas) {

            Image = image;
            WriteableBitmap = new WriteableBitmap(1, 1, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null);
            Image.Source = WriteableBitmap;
            Canvas = canvas;
            RelativeCanvasChildren = new List<RelativeCanvasElement>();
        }

        public void DisplayFrame(Bitmap bitmapToDisplay) {

            if (bitmapToDisplay == null) {
                return;
            }

            WriteableBitmap.Dispatcher.Invoke(() => {

                // Make sure, that "Image"-Source is compatible with "bitmapToDisplay"
                if (WriteableBitmap == null || WriteableBitmap.PixelWidth != bitmapToDisplay.Width || WriteableBitmap.PixelHeight != bitmapToDisplay.Height) {

                    WriteableBitmap = new WriteableBitmap(bitmapToDisplay.Width, bitmapToDisplay.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null);
                    Image.Source = WriteableBitmap;
                }

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

        public void SetRelativeCanvasChildren(List<RelativeCanvasElement> newRelativeCanvasChildren) {

            RelativeCanvasChildren.Clear();
            for (int i = 0; i < newRelativeCanvasChildren.Count; i++) {

                AddRelativeCanvasElement(newRelativeCanvasChildren[i]);
            }
        }

        public void AddRelativeCanvasElement(RelativeCanvasElement relativeUIElement) {

            if (RelativeCanvasChildren.Where(o => o.CanvasElement == relativeUIElement.CanvasElement).Count() > 0) {

                return;
            }

            Canvas.Dispatcher.Invoke(() => {

                UIElement uiElement = relativeUIElement.GenerateUIElement(Canvas.ActualWidth, Canvas.ActualHeight);
                Canvas.Children.Add(uiElement);
                relativeUIElement.CanvasElement = uiElement;
                RelativeCanvasChildren.Add(relativeUIElement);
            });
        }

        public void UpdateAllCanvasElements() {

            Canvas.Children.Clear();
            foreach (RelativeCanvasElement rce in RelativeCanvasChildren) {

                Canvas.Children.Add(rce.GenerateUIElement(Canvas.ActualWidth, Canvas.ActualHeight));
            }
        }

        public List<RelativeCanvasElement> GetRelativeCanvasChildren(bool clone = false) {

            if (clone) {

                return RelativeCanvasChildren.Select(o => (RelativeCanvasElement)o.Clone()).ToList();
            }
            else {

                return RelativeCanvasChildren;
            }
        }
    }
}
