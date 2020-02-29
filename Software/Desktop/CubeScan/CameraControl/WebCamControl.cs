using Emgu.CV;
using Emgu.CV.Structure;
using Rubinator3000.CubeScan.ColorIdentification;
using RubinatorCore;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan.CameraControl {
    public class WebCamControl {

        #region Member Variables

        public CubeScanner ParentCubeScanner { get; set; }

        // Index of usb camera in os-settings
        public int CameraIndex { get; private set; }

        public bool Initialized { get; private set; } = false;

        public bool Started { get; set; }

        // Capture-object to retrieve bitmaps from usb camera
        private VideoCapture VideoCapture { get; set; }

        // Holds CubeScanFrame for edge-detecting-color-identification
        public CubeScanFrame CubeScanFrame { get; private set; }

        #endregion Member Variables

        public WebCamControl(CubeScanner parentCubeScanner, int cameraIndex) {

            ParentCubeScanner = parentCubeScanner;
            CameraIndex = cameraIndex;
            CubeScanFrame = new CubeScanFrame(this);
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

                ParentCubeScanner.PreviewHandler.Display(this, readBitmap);
                //CubeScanFrame.Reinitialize(this, mat.ToImage<Bgr, byte>());
            }
        }

        public bool TryInitialize(int cameraIndex) {

            Application.Current.Dispatcher.Invoke(() => {

                if (Initialized) {

                    TerminateCamera();
                    TryInitialize(cameraIndex);
                }
                else {

                    Log.LogMessage(string.Format("Initialization of Camera at index {0} started", cameraIndex));

                    // Try to connect to camera at cameraIndex
                    VideoCapture = new VideoCapture(cameraIndex);

                    // if setup was unsuccessful (if no camera connected at "CameraIndex")
                    if (!VideoCapture.IsOpened) {

                        Initialized = false;
                        Log.LogMessage(string.Format("Initialization of Camera {0} failed - (No camera connected at index \"{0}\")", cameraIndex));
                    }
                    else {
                        Initialized = true;
                        Log.LogMessage(string.Format("Initialization of Camera {0} successful", CameraIndex));
                    }
                }
            });
            return Initialized;
        }

        public void StartCamera() {

            if (Initialized) {

                if (!Started) {

                    VideoCapture.ImageGrabbed += ProcessCapturedFrame;

                    VideoCapture.Start();
                    Started = true;
                }
                else {

                    Log.LogMessage(string.Format("Could not start Camera {0} - (Started already)", CameraIndex));
                }
            }
            else {

                Log.LogMessage(string.Format("Could not start Camera {0} - (Not initialized yet)", CameraIndex));
            }
        }

        public void StopCamera() {

            if (Initialized) {

                if (Started) {

                    VideoCapture.ImageGrabbed -= ProcessCapturedFrame;
                    VideoCapture.Stop();
                    Started = false;
                }
                else {

                    Log.LogMessage(string.Format("Could not stop Camera {0} - (Stopped already)", CameraIndex));
                }
            }
            else {

                Log.LogMessage(string.Format("Could not stop Camera {0} - (Not even initialized)", CameraIndex));
            }
        }

        public void TerminateCamera() {

            if (VideoCapture != null) {
                StopCamera();
                ParentCubeScanner.PreviewHandler.Unoccupy(this);
                VideoCapture.Dispose();
                Initialized = false;
                GC.Collect();
                Log.LogMessage(string.Format("Successfully terminated Camera {0}", CameraIndex));
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
