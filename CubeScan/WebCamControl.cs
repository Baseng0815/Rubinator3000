using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan
{
    class WebCamControl
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private System.Windows.Controls.Image previewImage;
        private Canvas faceRectCanvas;

        private static List<int> cameraIndexesInUse = new List<int>();

        public int CameraIndex { get; set; }
        private VideoCapture videoCapture;

        private Bitmap currentBitmap = null;
        private bool accessingBitmap = false;

        private Thread thread;
        private bool shouldStop = true;

        // This array stores the rgb colors from all faces of the cube. Its used to differentiate between the 6 different colors
        private static Color[,,] colors;

        private List<FacePosition> facesToRead = new List<FacePosition>();

        public WebCamControl(int cameraIndex, ref System.Windows.Controls.Image previewImage, ref System.Windows.Controls.Canvas drawingCanvas)
        {
            this.previewImage = previewImage;
            this.faceRectCanvas = drawingCanvas;
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

            long loopStart = -1, loopEnd = -1;

            while (!shouldStop)
            {
                loopStart = Helper.CurrentTimeMillis();
                loopEnd = loopStart + 1000;

                // Code in while loop

                if (currentBitmap != null && !accessingBitmap)
                {
                    foreach (FacePosition pos in facesToRead)
                    {
                        int absoluteX = Convert.ToInt32(pos.RelativeX * currentBitmap.Width);
                        int absoluteY = Convert.ToInt32(pos.RelativeY * currentBitmap.Height);

                        int redSum = -1, greenSum = -1, blueSum = -1;
                        int pixelCount = 0;

                        for (int x = absoluteX - 3; x < absoluteX + 3; x++)
                        {
                            for (int y = absoluteY - 3; y < absoluteY + 3; y++)
                            {
                                if (currentBitmap != null)
                                {
                                    Color c = currentBitmap.GetPixel(x, y);
                                    redSum += c.R;
                                    greenSum += c.G;
                                    blueSum += c.B;
                                    pixelCount++;
                                }
                            }
                        }

                        if (pixelCount > 1)
                        {
                            int red = Convert.ToInt32(redSum / pixelCount);
                            int green = Convert.ToInt32(greenSum / pixelCount);
                            int blue = Convert.ToInt32(blueSum / pixelCount);

                            // TODO what is row/col
                            colors[pos.FaceIndex, pos.RowIndex, pos.ColIndex] = Color.FromArgb(red, green, blue);
                            Log.LogStuff(Helper.WhichColor(colors[pos.FaceIndex, pos.RowIndex, pos.ColIndex]).ToString());
                        }
                    }
                    if (CubeIsFullyScanned())
                    {
                        List<Color> colorsList = Helper.ColorsAsList(colors);
                    }
                }

                // Code in while loop

                while (Helper.CurrentTimeMillis() < loopEnd)
                {
                    Thread.Sleep(Convert.ToInt32(loopEnd - Helper.CurrentTimeMillis()));
                }
            }
        }

        private void Init()
        {
            facesToRead.Add(new FacePosition(0.5, 0.5, 0, 0, 0));
            if (cameraIndexesInUse.Contains(CameraIndex))
            {
                throw new Exception("Camera in use already");
            }
            else
            {
                Log.LogStuff(String.Format("Initialization of Camera {0} started", CameraIndex));

                // try to setup videoCapture
                videoCapture = new VideoCapture(CameraIndex);
                // if setup was unsuccessful (no camera connected)
                if (!videoCapture.IsOpened)
                {
                    Log.LogStuff(String.Format("Initialization of Camera {0} failed", CameraIndex));
                    return;
                }
                cameraIndexesInUse.Add(CameraIndex);
                videoCapture.ImageGrabbed += ProcessCapturedFrame;
                videoCapture.Start();

                Log.LogStuff(String.Format("Initialization of Camera {0} finished", CameraIndex));

                shouldStop = false;
            }
        }

        private void ProcessCapturedFrame(object sender, EventArgs e)
        {
            Mat frameAsMat = new Mat();
            videoCapture.Retrieve(frameAsMat, 0);

            accessingBitmap = true;
            currentBitmap = frameAsMat.Bitmap;

            // TODO preview bitmap in window

            accessingBitmap = false;

            // Draw rectangle on image
            foreach (FacePosition pos in facesToRead)
            {
                if (!pos.IsRectangleDrawn)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                        rect.Width = 7;
                        rect.Height = 7;
                        rect.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                        rect.StrokeThickness = 2;
                        faceRectCanvas.Children.Add(rect);
                        Canvas.SetLeft(rect, pos.RelativeX * previewImage.ActualWidth);
                        Canvas.SetTop(rect, pos.RelativeY * previewImage.ActualHeight);
                        pos.IsRectangleDrawn = true;
                    });
                }
            }
        }

        // Converts Bitmap to BitmapSource
        private BitmapSource BitmapToSource(Bitmap bitmap)
        {
            return (bitmap == null) ? null : System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
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
