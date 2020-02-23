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

        public CameraPreview CameraPreview { get; set; }

        public bool CameraPreviewEnabled { get; private set; }

        private Thread thread;
        private readonly int ticksPerSecond;
        private bool threadShouldStop = true;

        // Capture-object to retrieve bitmaps from usb camera
        private VideoCapture VideoCapture { get; set; }

        // Holds CubeScanFrame for edge-detecting-color-identification
        public CubeScanFrame CubeScanFrame { get; private set; } = new CubeScanFrame();

        #endregion Member Variables

        public WebCamControl2(CameraPreview cameraPreview, int cameraIndex) {

            CameraPreview = cameraPreview;
            CameraIndex = cameraIndex;
        }

        /// <summary>
        /// Initialize WebCamControl to be able to retrieve and process images from webcamcontrol
        /// <param name="cameraIndex">Camera Index from usb-camera, when this method is called, it should be checked, that it is not used already</param>
        /// <returns>If Initialization was successful</returns>
        public bool InitCameraCapture(int cameraIndex) {

            Log.LogMessage(string.Format("Initialization of Camera at index {0} started", cameraIndex));

            // Try to connect to usb camera at cameraIndex
            VideoCapture = new VideoCapture(cameraIndex);

            // if setup was unsuccessful (if no camera connected at "CameraIndex")
            if (!VideoCapture.IsOpened) {

                Log.LogMessage(string.Format("Initialization of Camera {0} failed - (No camera connected at index \"{0}\")", cameraIndex));
            }
            else {

                VideoCapture.ImageGrabbed += ProcessCapturedFrame;
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
            VideoCapture.Read(mat);
            Bitmap readBitmap = mat.Bitmap;

            if (readBitmap != null) {

                CameraPreview.DisplayFrame(readBitmap);
                CubeScanFrame.Reinitialize(mat.ToImage<Bgr, byte>());
            }
        }

        public Color ReadColorInsideContour(Contour contour) {

            return CubeScanFrame.ReadColorInsideContour(contour);
        }

        public Color ReadColorAtRelativePosition(double relativeX, double relativeY) {

            if (CubeScanFrame.Initialized) {

                return CubeScanFrame.ReadColorAtClosestContour(CubeScanFrame.FindClosestContour(relativeX, relativeY));
            }

            return Color.Empty;
        }
    }
}
