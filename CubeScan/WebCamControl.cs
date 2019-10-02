using Emgu.CV;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
                    Bitmap bitmap = new Bitmap(bmp);
                    DisplayOnWpfImageControl(bitmap, previewBitmap);
                    bitmap.Dispose();
                    currentBitmap.SetBitmap(bmp);
                }

                // Add all pendingPositions to PositionsToReadAt and draw their circles
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

            // Positions left to assign
            List<ReadPosition> positions = new List<ReadPosition>(PositionsToReadAt);

            // Assign color percentages to each color
            for (int i = 0; i < positions.Count; i++) {

                positions[i].Percentages = ColorIdentification.CalculateColorPercentages(positions[i].Color);
            }

            Cube cube = new Cube();

            for (int i = 0; i < 6; i++) {

                CubeColor currentCubeColor = (CubeColor)i;

                int[] maxIndicies = ColorIdentification.Max8Indicies(currentCubeColor, positions);

                List<ReadPosition> positionsToRemove = new List<ReadPosition>();

                for (int j = 0; j < maxIndicies.Length; j++) {

                    // Assign tiles to the cube
                    cube.SetTile((CubeFace)(positions[maxIndicies[j]].FaceIndex), positions[maxIndicies[j]].RowIndex * 3 + positions[maxIndicies[j]].ColIndex, currentCubeColor);
                    positions[maxIndicies[j]].AssumedCubeColor = currentCubeColor;

                    // Dye the circle of the readposition in the corresponding color
                    Application.Current.Dispatcher.Invoke(() => {
                        
                        // Change color of all the circles
                        CircleByIndicies(positions[maxIndicies[j]].FaceIndex, positions[maxIndicies[j]].RowIndex, positions[maxIndicies[j]].ColIndex).Stroke = Helper.ColorBrush(currentCubeColor);
                    });
                    positionsToRemove.Add(positions[j]);
                }

                // Sort maxIndicies -> highest value at index=0
                maxIndicies = maxIndicies.OrderByDescending(c => c).ToArray();

                // Remove all positions, that were assigned to the cube in this loop cycle
                for (int j = 0; j < maxIndicies.Length; j++) {

                    positions.RemoveAt(maxIndicies[j]);
                }
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

                        XElement readPositionElement = new XElement(XmlReadPosition);
                        readPositionElement.Add(new XAttribute(XmlRelativeX, pos.RelativeX));
                        readPositionElement.Add(new XAttribute(XmlRelativeY, pos.RelativeY));
                        readPositionElement.Add(new XAttribute(XmlFaceIndex, pos.FaceIndex));
                        readPositionElement.Add(new XAttribute(XmlRowIndex, pos.RowIndex));
                        readPositionElement.Add(new XAttribute(XmlColIndex, pos.ColIndex));

                        cameraElement.Add(readPositionElement);
                    }
                }

                // "doc.Root" is the XElement defined in the Constructor of "doc"
                docToSave.Root.Add(cameraElement);
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

                circle = GenerateCircle(pos.AssumedCubeColor);

                // Set position of circle on canvas
                Canvas.SetLeft(circle, pos.RelativeX * canvas.ActualWidth - ReadRadius);
                Canvas.SetTop(circle, pos.RelativeY * canvas.ActualHeight - ReadRadius);

                // Add circle to the canvas over the camera stream
                canvas.Children.Add(circle);
            });

            return circle;
        }

        private static Ellipse GenerateCircle(CubeColor cc) {

            // Initialize circle, that should be drawn over camera stream
            return new Ellipse {
                Width = ReadRadius * 2 + 1,
                Height = ReadRadius * 2 + 1,
                Stroke = Helper.ColorBrush(cc),
                StrokeThickness = ReadRadius / 1.5
            };
        }

        public static void DisplayOnWpfImageControl(Bitmap bitmap, WriteableBitmap writeableBitmap) {

            Application.Current.Dispatcher.Invoke(() => {
                // Reserve the backBuffer of writeableBitmap for updates
                writeableBitmap.Lock();
                unsafe {
                    BitmapData tempData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

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
