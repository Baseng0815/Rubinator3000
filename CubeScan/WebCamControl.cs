using Emgu.CV;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        // This list is to prevent, that multiple "WebCamControl"-Objects access the same usb camera
        private static List<int> cameraIndexesInUse = new List<int>();

        public int CameraIndex { get; set; }

        // Capture-object to retrieve images from usb camera
        private VideoCapture videoCapture;

        // This Bitmap caches the most recent bitmap from the videoCapture
        private FastAccessBitmap currentBitmap = new FastAccessBitmap(null);
        private WriteableBitmap previewBitmap;

        private Thread thread;
        private bool threadShouldStop = true;

        // capturing decides, if the camera should update "currentBitmap" and the gui-camerastream
        private bool capturing = false;

        // This array stores the rgb colors from all faces of the cube. Its used to differentiate between the 6 different colors
        private static Color[,,] colors;

        // This List stores, where the Program reads out the colors
        private List<FacePosition> circlePositions = new List<FacePosition>();
        

        // This canvas is to draw the circles, at the positions in "circlePositions"
        private Canvas drawingCanvas;

        public WebCamControl(int cameraIndex, ref Canvas drawingCanvas, ref WriteableBitmap previewBitmap)
        {
            this.drawingCanvas = drawingCanvas;
            this.previewBitmap = previewBitmap;
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

        private void Init()
        {
            // This position is for debug uses
            circlePositions.Add(new FacePosition(0.5, 0.5, 0, 0, 0));

            // Prevent 2 "WebCamControl"-objects from accessing the same usb camera
            if (cameraIndexesInUse.Contains(CameraIndex))
            {
                throw new Exception("Camera in use already");
            }
            else
            {
                Log.LogStuff(String.Format("Initialization of Camera {0} started", CameraIndex));

                // try to setup videoCapture
                videoCapture = new VideoCapture(CameraIndex);

                // if setup was unsuccessful (if no camera connected)
                if (!videoCapture.IsOpened)
                {
                    Log.LogStuff(String.Format("Initialization of Camera {0} failed", CameraIndex));
                    return;
                }
                else
                {
                    cameraIndexesInUse.Add(CameraIndex);
                    videoCapture.ImageGrabbed += ProcessCapturedFrame;

                    // start the video apture
                    videoCapture.Start();

                    Log.LogStuff(String.Format("Initialization of Camera {0} finished", CameraIndex));

                    threadShouldStop = false;
                    // tell the program, that "currentBitmap" and the gui-camerastream should be updated
                    capturing = true;
                }
            }
        }

        private void Run()
        {
            Init();

            long loopStart = -1, loopEnd = -1;

            int counter = 0;

            while (!threadShouldStop)
            {
                loopStart = Helper.CurrentTimeMillis();
                loopEnd = loopStart + 1000;

                // Code in while loop

                counter++;

                if (!currentBitmap.IsNull() && capturing)
                {
                    // Draw rectangle on image
                    foreach (FacePosition pos in circlePositions)
                    {
                        if (!pos.IsCircleDrawn)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                System.Windows.Shapes.Ellipse circle = new System.Windows.Shapes.Ellipse();
                                circle.Width = 7;
                                circle.Height = 7;
                                circle.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                                circle.StrokeThickness = 2;
                                drawingCanvas.Children.Add(circle);
                                Canvas.SetLeft(circle, pos.RelativeX * drawingCanvas.ActualWidth);
                                Canvas.SetTop(circle, pos.RelativeY * drawingCanvas.ActualHeight);
                                pos.IsCircleDrawn = true;
                            });
                        }
                    }

                    foreach (FacePosition pos in circlePositions)
                    {
                        int absoluteX = Convert.ToInt32(pos.RelativeX * currentBitmap.Width);
                        int absoluteY = Convert.ToInt32(pos.RelativeY * currentBitmap.Height);

                        // TODO what is row/col
                        colors[pos.FaceIndex, pos.RowIndex, pos.ColIndex] = currentBitmap.ReadPixel(absoluteX - 1, absoluteY - 1, 3, 3);
                        Log.LogStuff(ColorIdentification.WhichColor(colors[pos.FaceIndex, pos.RowIndex, pos.ColIndex]).ToString());

                    }
                    if (CubeIsFullyScanned())
                    {
                        List<Color> colorsList = Helper.ColorsAsList(colors);
                    }
                }

                GC.Collect();

                // Code in while loop

                while (Helper.CurrentTimeMillis() < loopEnd)
                {
                    Thread.Sleep(Convert.ToInt32(loopEnd - Helper.CurrentTimeMillis()));
                }
            }
        }

        private void ProcessCapturedFrame(object sender, EventArgs e)
        {
            if (capturing)
            {
                Mat mat = new Mat();
                videoCapture.Read(mat);
                currentBitmap.SetBitmap(mat.Bitmap);

                currentBitmap.DisplayOnWpfImageControl(previewBitmap);
            }
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

        public void Stop()
        {
            capturing = false;
            threadShouldStop = true;
        }

        public void PauseCapture()
        {
            capturing = false;
        }

        public void ContinueCapture()
        {
            capturing = true;
        }
    }
}
