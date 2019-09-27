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

        private Canvas drawingCanvas;

        private static List<int> cameraIndexesInUse = new List<int>();

        public int CameraIndex { get; set; }
        private VideoCapture videoCapture;

        private Bitmap currentBitmap = null;
        private bool bitmapLocked = false;
        private WriteableBitmap previewBitmap;

        private Thread thread;
        private bool shouldStop = true;
        private bool capturing = false;

        // This array stores the rgb colors from all faces of the cube. Its used to differentiate between the 6 different colors
        private static Color[,,] colors;

        private List<FacePosition> circlePositions = new List<FacePosition>();

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
            //thread.Start();

            Init();
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
                capturing = true;
            }
        }

        private void Run()
        {
            long loopStart = -1, loopEnd = -1;

            while (!shouldStop)
            {
                loopStart = Helper.CurrentTimeMillis();
                loopEnd = loopStart + 1000;

                // Code in while loop

                if (currentBitmap != null && capturing)
                {
                    // Draw rectangle on image
                    foreach (FacePosition pos in circlePositions)
                    {
                        if (!pos.IsRectangleDrawn)
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
                                pos.IsRectangleDrawn = true;
                            });
                        }
                    }

                    foreach (FacePosition pos in circlePositions)
                    {
                        int absoluteX = Convert.ToInt32(pos.RelativeX * currentBitmap.Width);
                        int absoluteY = Convert.ToInt32(pos.RelativeY * currentBitmap.Height);

                        int redSum = -1, greenSum = -1, blueSum = -1;
                        int pixelCount = 0;

                        for (int x = absoluteX - 3; x < absoluteX + 3; x++)
                        {
                            for (int y = absoluteY - 3; y < absoluteY + 3; y++)
                            {
                                if (currentBitmap != null && !bitmapLocked)
                                {
                                    Color c = currentBitmap.GetPixel(x, y);

                                    GC.Collect();

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
                            Log.LogStuff(ColorIdentification.WhichColor(colors[pos.FaceIndex, pos.RowIndex, pos.ColIndex]).ToString());
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

        private void ProcessCapturedFrame(object sender, EventArgs e)
        {
            if (capturing)
            {
                Mat frameAsMat = new Mat();
                videoCapture.Retrieve(frameAsMat, 0);

                currentBitmap = frameAsMat.Bitmap;

                UpdatePreviewImage();

                Process();
            }
        }

        private void Process()
        {
            if (currentBitmap != null && capturing)
            {
                // Draw rectangle on image
                foreach (FacePosition pos in circlePositions)
                {
                    if (!pos.IsRectangleDrawn)
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
                            pos.IsRectangleDrawn = true;
                        });
                    }
                }

                foreach (FacePosition pos in circlePositions)
                {
                    int absoluteX = Convert.ToInt32(pos.RelativeX * currentBitmap.Width);
                    int absoluteY = Convert.ToInt32(pos.RelativeY * currentBitmap.Height);

                    int redSum = -1, greenSum = -1, blueSum = -1;
                    int pixelCount = 0;

                    for (int x = absoluteX - 3; x < absoluteX + 3; x++)
                    {
                        for (int y = absoluteY - 3; y < absoluteY + 3; y++)
                        {
                            if (currentBitmap != null && !bitmapLocked)
                            {
                                Color c = currentBitmap.GetPixel(x, y);

                                GC.Collect();

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
                        Log.LogStuff(ColorIdentification.WhichColor(colors[pos.FaceIndex, pos.RowIndex, pos.ColIndex]).ToString());
                    }
                }
                if (CubeIsFullyScanned())
                {
                    List<Color> colorsList = Helper.ColorsAsList(colors);
                }
            }
        }

        private void UpdatePreviewImage()
        {

            long pBackBuffer = 0, backBufferStride = 0;

            Application.Current.Dispatcher.Invoke(() =>
            {
                //lock bitmap in ui thread
                previewBitmap.Lock();
                pBackBuffer = (long)previewBitmap.BackBuffer; //Make pointer available to background thread
                backBufferStride = previewBitmap.BackBufferStride;
            });

            unsafe
            {
                // Lock all pixels of "currentBitmap"
                bitmapLocked = true;
                BitmapData bitmapData = currentBitmap.LockBits(new Rectangle(0, 0, currentBitmap.Width, currentBitmap.Height), ImageLockMode.ReadOnly, currentBitmap.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(currentBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                int totalBytes = Convert.ToInt32((widthInBytes * heightInPixels));
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                for (int i = 0; i < totalBytes - bytesPerPixel; i += bytesPerPixel)
                {
                    int color_data = ptrFirstPixel[i + 2] << 16; // R
                    color_data |= ptrFirstPixel[i + 1] << 8;   // G
                    color_data |= ptrFirstPixel[i] << 0;   // B

                    long bufferWithOffset = pBackBuffer + i;
                    *((int*)bufferWithOffset) = color_data;
                }

                currentBitmap.UnlockBits(bitmapData);
                bitmapLocked = false;

            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                //UI thread does post update operations
                int width = Convert.ToInt32(previewBitmap.Width);
                int height = Convert.ToInt32(previewBitmap.Height);
                previewBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                previewBitmap.Unlock();
            });
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
            shouldStop = true;
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
