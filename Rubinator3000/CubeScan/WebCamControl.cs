using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

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
        private static Color[,,] colors;

        private List<FacePosition> facesToRead = new List<FacePosition>();

        public WebCamControl(int cameraIndex, ref System.Windows.Controls.Image previewImage)
        {
            this.previewImage = previewImage;
            CameraIndex = cameraIndex;

            colors = new Color[6, 3, 3];
            for (int i = 0; i < colors.GetLength(0); i++)
            {
                for (int j = 0; j < colors.GetLength(1); j++)
                {
                    for (int k = 0; k < colors.GetLength(2); k++)
                    {
                        colors[i, j, k] = Color.Empty;
                    }
                }
            }

            thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }

        private void Run()
        {
            Init();

            while (!shouldStop)
            {
                if (CubeIsFullyScanned())
                {

                }
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

            facesToRead.Add(new FacePosition(0.5, 0.5, 0, 0, 0));

            foreach (FacePosition pos in facesToRead)
            {
                if (pos.IsRectangleDrawn)
                {
                    
                }
            }
        }

        // Converts Bitmap to BitmapSource
        private BitmapSource Convert(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        private bool CubeIsFullyScanned()
        {
            for (int i = 0; i < colors.GetLength(0); i++)
            {
                for (int j = 0; j < colors.GetLength(1); j++)
                {
                    for (int k = 0; k < colors.GetLength(2); k++)
                    {
                        if (colors[i, j, k] == null)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void RequestStop()
        {
            shouldStop = true;
        }
    }
}
