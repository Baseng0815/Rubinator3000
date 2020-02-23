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

        public int CameraPreviewIndex { get; set; }

        public bool Initialized { get; private set; } = false;

        public bool Started { get; set; }

        // Capture-object to retrieve bitmaps from usb camera
        private VideoCapture VideoCapture { get; set; }

        // Holds CubeScanFrame for edge-detecting-color-identification
        public CubeScanFrame CubeScanFrame { get; private set; }

        public Resolution Resolution { get; set; }

        #endregion Member Variables

        public WebCamControl(CubeScanner parentCubeScanner, System.Windows.Controls.Image image, Canvas canvas, int cameraIndex, int cameraPreviewIndex) {

            ParentCubeScanner = parentCubeScanner;
            CameraPreviewIndex = cameraPreviewIndex;
            ParentCubeScanner.cameraPreviews[cameraPreviewIndex] = new CameraPreview(image, canvas, null);
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

                GetCameraPreview().DisplayFrame(readBitmap);
                CubeScanFrame.Reinitialize(this, mat.ToImage<Bgr, byte>());
            }
        }

        public bool TryInitialize(Resolution resolution, int cameraIndex = -1) {

            Resolution = resolution;

            Application.Current.Dispatcher.Invoke(() => {

                if (Initialized) {

                    TerminateCamera();
                    TryInitialize(Resolution);
                }
                else {

                    if (cameraIndex == -1) {

                        cameraIndex = CameraIndex;
                    }

                    Log.LogMessage(string.Format("Initialization of Camera at index {0} started", cameraIndex));

                    // Try to connect to camera at cameraIndex
                    VideoCapture = new VideoCapture(cameraIndex);
                    /*
                    // if setup was unsuccessful (if no camera connected at "CameraIndex")
                    if (!VideoCapture.IsOpened) {

                        Initialized = false;
                        Log.LogMessage(string.Format("Initialization of Camera {0} failed - (No camera connected at index \"{0}\")", cameraIndex));
                    }
                    else {
                        */
                    UpdatePreviewSource(Resolution);
                    Initialized = true;
                    Log.LogMessage(string.Format("Initialization of Camera {0} successful - (Displaing at CameraPreview \"{0}\")", GetCameraPreview().GetNumber()));
                    //}
                }
            });
            return Initialized;
        }

        public void UpdatePreviewSource(Resolution resolution) {

            GetCameraPreview().SetSource(new WriteableBitmap(resolution.Width, resolution.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null));
            GetCameraPreview().UpdateAllCanvasElements();
        }

        public void StartCamera() {

            if (Initialized) {

                if (!Started) {

                    // Testing

                    Bitmap bmp = new Bitmap(string.Format("C:\\Users\\Tim\\Desktop\\RubinatorBilder\\0_{0}.png", CameraIndex));
                    GetCameraPreview().DisplayFrame(bmp);
                    bmp = new Bitmap(string.Format("C:\\Users\\Tim\\Desktop\\RubinatorBilder\\0_{0}.png", CameraIndex));
                    CubeScanFrame.Reinitialize(this, new Image<Bgr, byte>(bmp));

                    // Testing

                    //VideoCapture.ImageGrabbed += ProcessCapturedFrame;//

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

        public CameraPreview GetCameraPreview() {

            return ParentCubeScanner.cameraPreviews[CameraPreviewIndex];
        }
    }
}
