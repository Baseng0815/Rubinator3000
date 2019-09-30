using Emgu.CV;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using static Rubinator3000.CubeScan.XmlDesignations;

namespace Rubinator3000.CubeScan {

    class WebCamControl {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        #region Static Members

        // This list is to prevent, that multiple "WebCamControl"-Objects access the same usb camera
        private static List<int> cameraIndexesInUse = new List<int>();

        public static int ReadRadius = 3;

        public const int MAXPOSITIONSTOREAD = 48;

        // This list stores the rgb colors at all tiles of the cube. Its used to differentiate between the 6 different colors
        public static readonly List<ReadPosition> PositionsToReadAt = new List<ReadPosition>();
        private static readonly Queue<ReadPosition>[] pendingPositions = {
            new Queue<ReadPosition>(),
            new Queue<ReadPosition>(),
            new Queue<ReadPosition>(),
            new Queue<ReadPosition>()
        };

        public static readonly List<ReadPosition> middleTiles = new List<ReadPosition>() {
            new ReadPosition(-1, -1, 0, 2, 2, -1, Color.FromArgb(255, 255, 165, 0)), // Orange
            new ReadPosition(-1, -1, 0, 2, 2, -1, Color.FromArgb(255, 255, 255, 255)), // White
            new ReadPosition(-1, -1, 0, 2, 2, -1, Color.FromArgb(255, 0, 0, 255)), // Green
            new ReadPosition(-1, -1, 0, 2, 2, -1, Color.FromArgb(255, 255, 255, 0)), // Yellow
            new ReadPosition(-1, -1, 0, 2, 2, -1, Color.FromArgb(255, 255, 0, 0)), // Red
            new ReadPosition(-1, -1, 0, 2, 2, -1, Color.FromArgb(255, 0, 0, 255)) // Blue
            };

        private static long lastCubeGeneration = Helper.CurrentTimeMillis();

        private static string PathToXml => ".\\ReadPositions.xml";

        #endregion

        #region Members
        // Capture-object to retrieve images from usb camera
        private VideoCapture videoCapture;

        private readonly int cameraIndex;

        // This Bitmap caches the most recent bitmap from the videoCapture
        private readonly FastAccessBitmap currentBitmap = new FastAccessBitmap(null);

        // This WriteableBitmap points to the bitmap, that is used by the camera stream
        private readonly WriteableBitmap previewBitmap;

        // Points on the gui canvas, where the circles of the positions should be drawn on
        private readonly Canvas drawingCanvas;

        private readonly Thread thread;
        private readonly int ticksPerSecond;
        private bool threadShouldStop = true;

        private readonly Queue<Bitmap> frameUpdates = new Queue<Bitmap>();
        #endregion

        public WebCamControl(int cameraIndex, ref Canvas drawingCanvas, ref WriteableBitmap previewBitmap, int ticksPerSecond = 60) {

            this.drawingCanvas = drawingCanvas;
            this.previewBitmap = previewBitmap;
            this.cameraIndex = cameraIndex;
            this.ticksPerSecond = ticksPerSecond;

            thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }

        #region Member Functions

        private void Init() {

            // Prevent 2 "WebCamControl"-objects from accessing the same usb-camera
            if (cameraIndexesInUse.Contains(cameraIndex)) {

                throw new Exception("Camera in use already");
            }
            else {
                Log.LogStuff(string.Format("Initialization of Camera {0} started", cameraIndex));

                // try to setup videoCapture
                videoCapture = new VideoCapture(cameraIndex);

                // if setup was unsuccessful (if no camera connected at "CameraIndex")
                if (!videoCapture.IsOpened) {

                    Log.LogStuff(String.Format("Initialization of Camera {0} failed", cameraIndex));
                    return;
                }
                else {

                    cameraIndexesInUse.Add(cameraIndex);

                    // setup usb-camera-input handling
                    videoCapture.ImageGrabbed += ProcessCapturedFrame;

                    // start the video apture
                    videoCapture.Start();

                    Log.LogStuff(string.Format("Initialization of Camera {0} finished", cameraIndex));

                    threadShouldStop = false;
                }
            }
        }

        private void Run() {

            Init();

            while (!threadShouldStop) {

                long loopStart = Helper.CurrentTimeMillis();
                long loopEnd = loopStart + 1000 / ticksPerSecond;

                // Code in while loop  

                // Handle all frameUpdates
                while (frameUpdates.Count > 0) {

                    Bitmap bmp = frameUpdates.Dequeue();
                    DisplayOnWpfImageControl(new Bitmap(bmp), previewBitmap);
                    currentBitmap.SetBitmap(bmp);
                }

                // Draw all circles of positions on canvas
                while (pendingPositions[cameraIndex].Count > 0) {
                    ReadPosition pos = pendingPositions[cameraIndex].Dequeue();

                    pos.Circle = DrawCircleAtPosition(pos, drawingCanvas);

                    PositionsToReadAt.Add(pos);
                }

                // Read all colors from positions in readPositions
                for (int i = 0; i < PositionsToReadAt.Count; i++) {

                    if (PositionsToReadAt[i].CameraIndex == cameraIndex) {

                        PositionsToReadAt[i].Color = ReadColorAtPosition(PositionsToReadAt[i]);
                    }
                }

                // This block will be only executed by the primary webcamcontrol
                if (cameraIndex == 0) {

                    if (Helper.CurrentTimeMillis() - lastCubeGeneration > 10000) {
                        // Run this code only every 10 seconds

                        // If the whole cube is scanned, send the cube-configuration to the cube solver
                        if (CubeIsFullyScanned()) {

                            SortAndValidateColors();
                        }
                        lastCubeGeneration = Helper.CurrentTimeMillis();
                    }
                }

                // Code in while loop  

                while (Helper.CurrentTimeMillis() < loopEnd) {

                    Thread.Sleep(Convert.ToInt32(loopEnd - Helper.CurrentTimeMillis()));
                }
            }
        }

        private void ProcessCapturedFrame(object sender, EventArgs e) {

            Mat mat = new Mat();
            videoCapture.Read(mat);

            frameUpdates.Enqueue(new Bitmap(mat.Bitmap));
        }

        private Color ReadColorAtPosition(ReadPosition pos) {

            if (currentBitmap.BitmapIsValid()) {

                int absoluteX = Convert.ToInt32(pos.RelativeX * currentBitmap.Width);
                int absoluteY = Convert.ToInt32(pos.RelativeY * currentBitmap.Height);

                Color colorAtPos = currentBitmap.ReadPixel(absoluteX - ReadRadius, absoluteY - ReadRadius, ReadRadius * 2 + 1, ReadRadius * 2 + 1);

                return colorAtPos;
            }

            return Color.Empty;
        }

        #endregion

        #region Static Functions

        private static bool CubeIsFullyScanned() {

            foreach (ReadPosition tempPos in PositionsToReadAt) {

                if (tempPos.Color == Color.Empty) {
                    return false;
                }
            }

            return PositionsToReadAt.Count == MAXPOSITIONSTOREAD;
        }

        private static void SortAndValidateColors() {

            List<ReadPosition> unsorted = new List<ReadPosition>(PositionsToReadAt);

            // Add the middle-tiles, because they wont be read out with the camera
            unsorted.AddRange(middleTiles);

            List<ReadPosition> sorted = new List<ReadPosition>();

            // This for loop moves all colors from the unsorted list to the sorted list while sorting them ()
            for (int i = 0; i < 6; i++) {
                if (!(unsorted.Count >= 9)) {
                    Log.LogStuff("Cube Generation Failed");
                    return;
                }
                Move9Highest(colorIndex: i, ref unsorted, ref sorted);
            }

            /* the list "sorted" looks now like this
             *  indicies 0-8:   all positions, with orange tiles
             *  indicies 9-17:  all positions, with white tiles
             *  indicies 18-26: all positions, with green tiles
             *  indicies 27-35: all positions, with yellow tiles
             *  indicies 36-44: all positions, with red tiles
             *  indicies 45-53: all positions, with blue tiles
             */

            Cube cube = new Cube();

            for (int i = 0; i < sorted.Count; i++) {

                ReadPosition colorAtPos = sorted[i];

                /* "currentColorToSet" changes in switch 
                * -> the first 9 loop cycles assign all orange tiles to the cube
                * -> the next 9 loop cycles assign all white tiles
                * etc.
                */
                CubeColor currentColorToSet = CubeColor.NONE;

                switch (Math.Floor(i / 8d)) {

                    case (int)CubeColor.ORANGE:
                        currentColorToSet = CubeColor.ORANGE;
                        break;
                    case (int)CubeColor.WHITE:
                        currentColorToSet = CubeColor.WHITE;
                        break;
                    case (int)CubeColor.GREEN:
                        currentColorToSet = CubeColor.GREEN;
                        break;
                    case (int)CubeColor.YELLOW:
                        currentColorToSet = CubeColor.YELLOW;
                        break;
                    case (int)CubeColor.RED:
                        currentColorToSet = CubeColor.RED;
                        break;
                    case (int)CubeColor.BLUE:
                        currentColorToSet = CubeColor.BLUE;
                        break;
                }

                // Assign tiles to the cube
                cube.SetTile((CubeFace)colorAtPos.FaceIndex, colorAtPos.RowIndex * 3 + colorAtPos.ColIndex, currentColorToSet);

                Application.Current.Dispatcher.Invoke(() => {

                    if (colorAtPos.RowIndex != 2 && colorAtPos.ColIndex != 2) {
                        CircleByIndicies(colorAtPos.FaceIndex, colorAtPos.RowIndex, colorAtPos.ColIndex).Stroke = Helper.ColorBrush(currentColorToSet);
                    }
                });
            }
        }

        private static void Move9Highest(int colorIndex, ref List<ReadPosition> unsortedSource, ref List<ReadPosition> sortedDestination) {

            // Find indicies of 9 highest color-percentages of "colorIndex" in unsortedSource and move them to the sortedDestination
            for (int i = 0; i < 9; i++) {

                int maxIndex = ColorIdentification.MaxIndex(colorIndex, unsortedSource);
                sortedDestination.Add(unsortedSource[maxIndex]);
                unsortedSource.RemoveAt(maxIndex);
            }
        }

        public static void AddPosition(ReadPosition readPosition, int index) {

            pendingPositions[index].Enqueue(readPosition);
        }

        public static Ellipse CircleByIndicies(int faceIndex, int rowIndex, int colIndex) {

            foreach (ReadPosition pos in PositionsToReadAt) {
                if (pos.RowIndex == rowIndex && pos.ColIndex == colIndex && pos.FaceIndex == faceIndex) {
                    return pos.Circle;
                }
            }

            return null;
        }

        public static void SaveAllPositionsToXml() {

            // Setup XmlDocument
            XDocument docToSave = new XDocument(new XElement(XmlCameraReadPositions)) {
                Declaration = new XDeclaration("1.0", "UTF-8", null)
            };

            // Store Positions for the 4 cameras in "doc"
            for (int currentCameraIndex = 0; currentCameraIndex < 4; currentCameraIndex++) {

                XElement cameraElement = new XElement(XmlCamera, new XAttribute(XmlCameraIndex, currentCameraIndex));

                foreach (ReadPosition pos in PositionsToReadAt) {

                    if (pos.CameraIndex == currentCameraIndex) {

                        XElement readPositionElement = new XElement(XmlDesignations.XmlReadPosition);
                        readPositionElement.Add(new XAttribute(XmlRelativeX, pos.RelativeX));
                        readPositionElement.Add(new XAttribute(XmlRelativeY, pos.RelativeY));
                        readPositionElement.Add(new XAttribute(XmlFaceIndex, pos.FaceIndex));
                        readPositionElement.Add(new XAttribute(XmlRowIndex, pos.RowIndex));
                        readPositionElement.Add(new XAttribute(XmlColIndex, pos.ColIndex));

                        cameraElement.Add(readPositionElement);
                    }

                    // "doc.Root" is the XElement defined in the Constructor of "doc"
                    docToSave.Root.Add(cameraElement);
                }
            }

            if (File.Exists(PathToXml)) {

                File.Delete(PathToXml);
            }

            docToSave.Save(PathToXml);
        }

        public static void LoadAllPositionsFromXml() {

            if (!File.Exists(PathToXml)) {
                return;
            }

            XDocument loadedDoc = XDocument.Load(PathToXml);

            IEnumerable<XElement> cameraElements = loadedDoc.Root.Elements();

            foreach (XElement cameraElement in cameraElements) {

                foreach (XElement readPositionElement in cameraElement.Elements()) {

                    AddPosition(new ReadPosition(
                        double.Parse(readPositionElement.Attribute(XmlRelativeX).Value.Replace('.', ',')),
                        double.Parse(readPositionElement.Attribute(XmlRelativeY).Value.Replace('.', ',')),
                        int.Parse(readPositionElement.Attribute(XmlFaceIndex).Value),
                        int.Parse(readPositionElement.Attribute(XmlRowIndex).Value),
                        int.Parse(readPositionElement.Attribute(XmlColIndex).Value),
                        int.Parse(cameraElement.Attribute(XmlCameraIndex).Value)
                        ), int.Parse(cameraElement.Attribute(XmlCameraIndex).Value)
                    );
                }
            }
        }

        private static Ellipse DrawCircleAtPosition(ReadPosition pos, Canvas canvas) {

            Ellipse circle = null;

            Application.Current.Dispatcher.Invoke(() => {

                // Initialize circle, that should be drawn over camera stream
                circle = new Ellipse {
                    Width = ReadRadius * 2 + 1,
                    Height = ReadRadius * 2 + 1,
                    Stroke = Helper.ColorBrush(CubeColor.NONE), // Default color of circle will be black
                    StrokeThickness = ReadRadius / 2
                };

                // Add circle to the canvas over the camera stream
                canvas.Children.Add(circle);

                // Set position of circle on canvas
                Canvas.SetLeft(circle, pos.RelativeX * canvas.ActualWidth - ReadRadius);
                Canvas.SetTop(circle, pos.RelativeY * canvas.ActualHeight - ReadRadius);

            });

            // Wait until GUI-Thread has drawn circle
            while (circle == null) {
                Thread.Sleep(1);
            }

            return circle;
        }

        public static void RedrawAllCircles(Canvas[] canvases) {

            // Remove all children (circles) of each canvas
            for (int i = 0; i < canvases.Length; i++) {
                canvases[i].Children.Clear();
            }

            // Draw all circles
            for (int i = 0; i < canvases.Length; i++) {

                foreach (ReadPosition entry in PositionsToReadAt) {

                    if (entry.CameraIndex == i) {

                        DrawCircleAtPosition(entry, canvases[i]);
                    }
                }
            }
        }

        public static void DisplayOnWpfImageControl(Bitmap bitmap, WriteableBitmap writeableBitmap) {

            Application.Current.Dispatcher.Invoke(() => {
                // Reserve the backBuffer of writeableBitmap for updates
                writeableBitmap.Lock();
                unsafe {
                    BitmapData tempData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    // CopyMemory(destPointer, sourcePointer, byteLength to copy);
                    CopyMemory(writeableBitmap.BackBuffer, tempData.Scan0, writeableBitmap.BackBufferStride * bitmap.Height);

                    bitmap.UnlockBits(tempData);
                    tempData = null;
                }

                // Specify the area of the bitmap, that changed (in this case, the whole bitmap changed)
                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Dispose();

                // Release the backBuffer of writeableBitmap and make it available for display
                writeableBitmap.Unlock();
            });
        }

        #endregion
    }
}
