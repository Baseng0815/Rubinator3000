using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Rubinator3000.CubeScan
{
    class WebCamControl
    {
        private System.Windows.Controls.Image previewImage;

        private static List<int> cameraIndexesInUse = new List<int>();

        public int CameraIndex { get; set; }
        private VideoCapture videoCapture;

        private FastAccessBitmap currentBitmap;

        private Thread thread;
        private bool shouldStop = true;

        // This array stores the rgb colors from all faces of the cube. Its used to differentiate between the 6 different colors
        private static Color[,,] colors = new Color[6,3,3];

        private List<FacePosition> facesToRead = new List<FacePosition>();

        public WebCamControl(int cameraIndex, ref System.Windows.Controls.Image previewImage)
        {
            this.previewImage = previewImage;
            CameraIndex = cameraIndex;

            thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }

        private void Run()
        {
            Init();

            while (!shouldStop)
            {

            }
        }

        private void Init()
        {
            if (cameraIndexesInUse.Contains(CameraIndex))
            {
                throw new Exception("Camera in use already");
            }
            else
            {
                Log.LogStuff(String.Format("Initialization of Camera {0} started", CameraIndex));

                videoCapture = new VideoCapture(CameraIndex);
                cameraIndexesInUse.Add(CameraIndex);
                videoCapture.ImageGrabbed += ProcessCapturedFrame;
                videoCapture.Start();

                currentBitmap = new FastAccessBitmap(null);

                Log.LogStuff(String.Format("Initialization of Camera {0} finished", CameraIndex));

                shouldStop = false;
            }
        }

        private void ProcessCapturedFrame(object sender, EventArgs e)
        {
            Mat frameAsMat = new Mat();
            videoCapture.Retrieve(frameAsMat, 0);
            currentBitmap.SetBitmap(frameAsMat.Bitmap);

            // previewImage.Source has to be set by the thread, that owns the previewImage-control
            Application.Current.Dispatcher.Invoke(() => { previewImage.Source = Convert(currentBitmap.GetBitmap()); });

            // TODO Process Bitmap
        }

        // Converts Bitmap to BitmapSource
        public System.Windows.Media.Imaging.BitmapSource Convert(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public void RequestStop()
        {
            shouldStop = true;
        }
    }
}
