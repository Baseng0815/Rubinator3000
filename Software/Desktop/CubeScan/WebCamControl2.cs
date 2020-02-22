using Emgu.CV;
using Emgu.CV.Structure;
using RubinatorCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan {
    class WebCamControl2 {

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        #region Member Variables

        // Index of usb camera in os-settings
        public int CameraIndex { get; private set; }

        public bool Initialized { get; private set; } = false;

        public bool CaptureStarted { get; private set; }

        public bool CameraPreviewEnabled { get; private set; }

        private Thread thread;
        private readonly int ticksPerSecond;
        private bool threadShouldStop = true;

        // Capture-object to retrieve bitmaps from usb camera
        private VideoCapture videoCapture;

        // Holds CubeScanFrame for edge-detecting-color-identification
        private readonly CubeScanFrame cubeScanFrame = new CubeScanFrame();

        // This WriteablbeBitmap points to the bitmap, that will be shown in the wpf-image-control
        private WriteableBitmap previewBitmap;

        private Canvas drawingCanvas;

        #endregion Member Variables

        public WebCamControl2(int cameraIndex) {

        }

        /// <summary>
        /// Initialize WebCamControl to be able to retrieve and process images from webcamcontrol
        /// <param name="cameraIndex">Camera Index from usb-camera, when this method is called, it should be checked, that it is not used already</param>
        /// <returns>If Initialization was successful</returns>
        public bool InitCameraCapture(int cameraIndex) {

            Log.LogMessage(string.Format("Initialization of Camera at index {0} started", cameraIndex));

            // Try to connect to usb camera at cameraIndex
            videoCapture = new VideoCapture(cameraIndex);

            // if setup was unsuccessful (if no camera connected at "CameraIndex")
            if (!videoCapture.IsOpened) {

                Log.LogMessage(string.Format("Initialization of Camera {0} failed - (No camera connected at index \"{0}\")", cameraIndex));
            }
            else {

                videoCapture.ImageGrabbed += ProcessCapturedFrame;
            }

            return false;
        }

        /// <summary>
        /// Handle VideoCapture input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcessCapturedFrame(object sender, EventArgs e) {

            Mat mat = new Mat();
            videoCapture.Read(mat);
            Bitmap readBitmap = mat.Bitmap;

            if (readBitmap != null) {

                PreviewInWpf(readBitmap);
                cubeScanFrame.Reinitialize(mat.ToImage<Bgr, byte>());
            }
        }

        /// <summary>
        /// Display bitmap on previewBitmap
        /// </summary>
        /// <param name="bitmapToDisplay">Bitmap that should be displayed on previewBitmap</param>
        private void PreviewInWpf(Bitmap bitmapToDisplay) {

            if (bitmapToDisplay == null) {

                return;
            }

            Application.Current.Dispatcher.Invoke(() => {

                // Reserve the backBuffer of previewBitmap for updates
                previewBitmap.Lock();

                // Lock "bitmapToDisplay" to be able to fast-copy the bytes to previewBitmap
                BitmapData tempData = bitmapToDisplay.LockBits(new Rectangle(0, 0, bitmapToDisplay.Width, bitmapToDisplay.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // CopyMemory(destPointer, sourcePointer, byteLength to copy);
                CopyMemory(previewBitmap.BackBuffer, tempData.Scan0, previewBitmap.BackBufferStride * Convert.ToInt32(previewBitmap.Height));


                bitmapToDisplay.UnlockBits(tempData);
                tempData = null;

                // Specify the area of the bitmap, that changed (in this case, the wole bitmap)
                previewBitmap.AddDirtyRect(new Int32Rect(0, 0, bitmapToDisplay.Width, bitmapToDisplay.Height));

                // Release the backBuffer of previewBitmap and make it available for display
                previewBitmap.Unlock();
            });

        }

        public Color ReadColorAtRelativePosition(double relativeX, double relativeY) {

            if (cubeScanFrame.Initialized) {

                return cubeScanFrame.ReadColorAtClosestContour(relativeX, relativeY);
            }

            return Color.Empty;
        }
    }
}
