using Emgu.CV;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using static Rubinator3000.CubeScan.XmlDesignations;

namespace Rubinator3000.CubeScan {

    class WebCamControl {

        #region Static Variables

        // This list is to prevent, that multiple "WebCamControl"-Objects access the same usb camera
        private static List<int> cameraIndexesInUse = new List<int>();

        public static int ReadRadius = 3;

        // This list stores the rgb colors at all tiles of the cube. Its used to differentiate between the 6 different colors
        private static readonly ConcurrentDictionary<ReadPosition, Ellipse> positionsToReadAt = new ConcurrentDictionary<ReadPosition, Ellipse>();
        private static readonly Queue<ReadPosition> pendingPositions = new Queue<ReadPosition>();

        private static string PathToXml => ".\\ReadPositions.xml";

        #endregion

        #region Member Variables
        // Capture-object to retrieve images from usb camera
        private VideoCapture videoCapture;

        private readonly int cameraIndex;

        // This Bitmap caches the most recent bitmap from the videoCapture
        private readonly FastAccessBitmap currentBitmap = new FastAccessBitmap(null);

        // This WriteableBitmap points to the bitmap, that is used by the camera stream
        private readonly WriteableBitmap previewBitmap;

        private readonly Thread thread;
        private bool threadShouldStop = true;

        // Points on the gui canvas, where the circles of the positions should be drawn on
        private readonly Canvas drawingCanvas;

        private readonly int ticksPerSecond;

        #endregion

        public WebCamControl(int cameraIndex, ref Canvas drawingCanvas, ref WriteableBitmap previewBitmap, int ticksPerSecond = 1) {

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

                if (!currentBitmap.IsNull()) {

                    // Draw all pending circles on the canvas
                    while (pendingPositions.Count > 0) {

                        if (pendingPositions.Peek().CameraIndex == cameraIndex) {
                            ReadPosition pos = pendingPositions.Dequeue();

                            Ellipse circle = DrawCircleAtPosition(pos);

                            bool success = false;
                            while (!success) {

                                // Add position and related circle (that is drawn on the screen) for later processing
                                success = positionsToReadAt.TryAdd(pos, circle);
                            }
                        }
                    }

                    // Read all colors from positions in readPositions
                    foreach (ReadPosition pos in positionsToReadAt.Keys) {

                        // Already stores the color in the position
                        ReadColorAtPosition(pos);
                    }

                    // This block will be only executed by the primary webcamcontrol
                    if (cameraIndex == 0) {

                        // If the whole cube is scanned, send the cube-configuration to the cube solver
                        if (CubeIsFullyScanned()) {

                            SortAndValidateColors();
                        }
                    }

                    // Code in while loop  

                    if (GC.GetTotalMemory(true) > (500 * 10 ^ 6)) {
                        // Force Gargabe Collection if 500 MB or more are estimated to be allocated
                        GC.Collect();
                    }

                    while (Helper.CurrentTimeMillis() < loopEnd) {

                        Thread.Sleep(Convert.ToInt32(loopEnd - Helper.CurrentTimeMillis()));
                    }
                }
            }
        }

        private void ProcessCapturedFrame(object sender, EventArgs e) {

            Mat mat = new Mat();
            videoCapture.Read(mat);
            currentBitmap.SetBitmap(mat.Bitmap);

            currentBitmap.DisplayOnWpfImageControl(previewBitmap);
        }

        private Ellipse DrawCircleAtPosition(ReadPosition pos) {

            Ellipse circle = null;

            Application.Current.Dispatcher.Invoke(() => {

                // Initialize circle, that should be drawn over camera stream
                circle = new Ellipse {
                    Width = ReadRadius * 2 + 1,
                    Height = ReadRadius * 2 + 1,
                    Stroke = Helper.ColorBrush(CubeColor.WHITE), // Default color of circle
                    StrokeThickness = ReadRadius / 2
                };

                // Add circle to the canvas over the camera stream
                drawingCanvas.Children.Add(circle);

                // Set position of circle on canvas
                Canvas.SetLeft(circle, pos.RelativeX * drawingCanvas.ActualWidth);
                Canvas.SetTop(circle, pos.RelativeY * drawingCanvas.ActualHeight);

            });

            // Wait until GUI-Thread has drawn circle
            while (circle == null) {
                Thread.Sleep(1);
            }

            return circle;
        }

        private void ReadColorAtPosition(ReadPosition pos) {

            int absoluteX = Convert.ToInt32(pos.RelativeX * currentBitmap.Width);
            int absoluteY = Convert.ToInt32(pos.RelativeY * currentBitmap.Height);

            Color colorAtPos = currentBitmap.ReadPixel(absoluteX - ReadRadius, absoluteY - ReadRadius, ReadRadius * 2 + 1, ReadRadius * 2 + 1);

            if (colorAtPos != Color.Empty) {
                pos.Color = colorAtPos;
            }
        }

        #endregion

        #region Static Functions

        private static bool CubeIsFullyScanned() {

            foreach (ReadPosition tempPos in positionsToReadAt.Keys) {

                if (tempPos.Color == Color.Empty) {
                    return false;
                }
            }

            return positionsToReadAt.Count > 0;
        }

        private static void SortAndValidateColors() {

            List<ReadPosition> unsorted = new List<ReadPosition>(positionsToReadAt.Keys);

            List<ReadPosition> sorted = new List<ReadPosition>();

            // This for loop moves all colors from the unsorted list to the sorted list while sorting them ()
            for (int i = 0; i < 6; i++) {
                Move9Highest(i, ref unsorted, ref sorted);
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

                switch (Math.Floor(i / 9d)) {

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

        public static void AddPosition(ReadPosition readPosition) {

            pendingPositions.Enqueue(readPosition);
        }

        public static void SaveAllPositionsToXml() {

            // Setup XmlDocument
            XDocument docToSave = new XDocument(new XElement(CameraReadPositions)) {
                Declaration = new XDeclaration("1.0", "UTF-8", null)
            };

            // Store Positions for the 4 cameras in "doc"
            for (int currentCameraIndex = 0; currentCameraIndex < 4; currentCameraIndex++) {

                XElement cameraElement = new XElement(Camera, new XAttribute(CameraIndex, currentCameraIndex));

                foreach (ReadPosition pos in positionsToReadAt.Keys) {

                    if (pos.CameraIndex == currentCameraIndex) {

                        XElement readPositionElement = new XElement(XmlDesignations.ReadPosition);
                        readPositionElement.Add(new XAttribute(RelativeX, pos.RelativeX));
                        readPositionElement.Add(new XAttribute(RelativeY, pos.RelativeY));
                        readPositionElement.Add(new XAttribute(FaceIndex, pos.FaceIndex));
                        readPositionElement.Add(new XAttribute(RowIndex, pos.RowIndex));
                        readPositionElement.Add(new XAttribute(ColIndex, pos.ColIndex));

                        cameraElement.Add(readPositionElement);
                    }

                    // "doc.Root" is the XElement defined in the Constructor of "doc"
                    docToSave.Root.Add(cameraElement);
                }
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
                        double.Parse(readPositionElement.Attribute(RelativeX).Value.Replace('.', ',')),
                        double.Parse(readPositionElement.Attribute(RelativeY).Value.Replace('.', ',')),
                        int.Parse(readPositionElement.Attribute(FaceIndex).Value),
                        int.Parse(readPositionElement.Attribute(RowIndex).Value),
                        int.Parse(readPositionElement.Attribute(ColIndex).Value),
                        int.Parse(cameraElement.Attribute(CameraIndex).Value)
                        )
                    );
                }
            }
        }

        #endregion
    }
}
