using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using static Rubinator3000.CubeScan.XmlDesignations;

namespace Rubinator3000.CubeScan {

    public class WebCamControl {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        #region Static Member Variables

        // This list is to prevent, that multiple "WebCamControl"-Objects access the same usb camera
        private static List<int> cameraIndexesInUse = new List<int>();

        public static int ReadRadius = 6;

        public const int MAXPOSITIONSTOREAD = 48;
        public const int MAXPOSITIONSPERCAMERA = 20;

        // This list stores the rgb colors at all tiles of the cube. Its used to differentiate between the 6 different colors
        private static readonly ReadPosition[,] PositionsToReadAt = new ReadPosition[4, MAXPOSITIONSPERCAMERA];
        public static int TotalPositionCount = 0;
        private static readonly Queue<ReadPosition>[] pendingPositions = {
            new Queue<ReadPosition>(),
            new Queue<ReadPosition>(),
            new Queue<ReadPosition>(),
            new Queue<ReadPosition>()
        };

        private static long lastCubeGeneration = Helper.CurrentTimeMillis();

        private static string PathToXml => "./Resources/ReadPositions.xml";

        public delegate void OnCubeScannedEventHandler(object sender, CubeScanEventArgs e);
        public static event OnCubeScannedEventHandler OnCubeScanned;

        #endregion

        #region Member Variables
        // Capture-object to retrieve bitmaps from usb camera
        private VideoCapture videoCapture;

        // Index of usb camera in os-settings
        private readonly int cameraIndex;

        // This Bitmap caches the most recent bitmap from the videoCapture for color analysing
        private readonly FastAccessBitmap currentFABitmap = new FastAccessBitmap(null);

        // This WriteableBitmap points to the bitmap, that will be shown in the camera stream
        private readonly WriteableBitmap previewBitmap;

        // Points on the gui canvas, where the circles of the positions should be drawn on
        private readonly Canvas drawingCanvas;

        // The Thread of
        private Thread thread;
        private readonly int ticksPerSecond;
        private bool threadShouldStop = true;
        private bool threadStarted = false;
        public bool Initialized { get; set; } = false;

        // This Bitmap holds only the current update of the camera-stream for color identification, not for gui camera stream
        private Bitmap frameToDisplay;

        #endregion

        public WebCamControl(int cameraIndex, /*ref*/ Canvas drawingCanvas, ref WriteableBitmap previewBitmap, int ticksPerSecond = 1) {

            this.drawingCanvas = drawingCanvas;
            this.previewBitmap = previewBitmap;
            this.cameraIndex = cameraIndex;
            this.ticksPerSecond = ticksPerSecond;

            TryInitializeAndStart();
        }

        #region Member Functions

        private bool InitCameraCapture() {

            // Prevent 2 "WebCamControl"-objects from accessing the same usb-camera
            if (cameraIndexesInUse.Contains(cameraIndex)) {

                throw new Exception("Camera in use already");
            }
            else {
                Log.LogMessage(string.Format("Initialization of Camera {0} started", cameraIndex));

                // try to setup videoCapture
                videoCapture = new VideoCapture(cameraIndex);

                // if setup was unsuccessful (if no camera connected at "CameraIndex")
                if (!videoCapture.IsOpened) {

                    Log.LogMessage(string.Format("Initialization of Camera {0} failed - (No camera connected at index \"{0}\")", cameraIndex));
                }
                else {

                    cameraIndexesInUse.Add(cameraIndex);

                    // setup usb-camera-input handling
                    videoCapture.ImageGrabbed += ProcessCapturedFrame;

                    // start the video apture
                    videoCapture.Start();

                    Log.LogMessage(string.Format("Initialization of Camera {0} finished", cameraIndex));

                    return true;
                }
            }
            return false;
        }

        private void Run() {

            try {

                threadStarted = true;

                while (!threadShouldStop) {

                    long loopStart = Helper.CurrentTimeMillis();
                    long loopEnd = loopStart + 1000 / ticksPerSecond;

                    // Code in while loop  

                    // Handle all frameUpdates
                    if (frameToDisplay != null) {

                        currentFABitmap.SetBitmap(frameToDisplay);
                        frameToDisplay.Dispose();
                        frameToDisplay = null;
                    }

                    // Add all pendingPositions to PositionsToReadAt and draw their circles
                    while (pendingPositions[cameraIndex].Count > 0) {

                        ReadPosition pos = pendingPositions[cameraIndex].Dequeue();

                        for (int i = 0; i < PositionsToReadAt.GetLength(1); i++) {

                            if (PositionsToReadAt[cameraIndex, i] == null) {

                                pos.Circle = DrawCircleAtPosition(pos, drawingCanvas);
                                PositionsToReadAt[cameraIndex, i] = pos;
                                TotalPositionCount++;
                                break;
                            }
                        }
                    }

                    // Read all colors from positions in readPositions
                    for (int i = 0; i < PositionsToReadAt.GetLength(1); i++) {

                        if (PositionsToReadAt[cameraIndex, i] == null) {
                            continue;
                        }
                        PositionsToReadAt[cameraIndex, i].Color = ReadColorAtPosition(PositionsToReadAt[cameraIndex, i]);
                    }

                    // This block will be only executed by the primary webcamcontrol
                    if (cameraIndex == 0) {

                        // If the whole cube is scanned, send the cube-configuration to the cube solver
                        if (CubeIsFullyScanned()) {

                            SortAndValidateColors();
                        }
                        lastCubeGeneration = Helper.CurrentTimeMillis();
                    }

                    // Code in while loop  

                    // Garbage Collection
                    if (GC.GetTotalMemory(true) > 500 * Math.Pow(10, 6)) {

                        GC.Collect();
                    }

                    while (Helper.CurrentTimeMillis() < loopEnd) {

                        Thread.Sleep(Convert.ToInt32(loopEnd - Helper.CurrentTimeMillis()));
                    }
                }
            }
            catch (Exception) {

                Log.LogMessage(string.Format("Camera {0} crashed", cameraIndex));
            }
            threadStarted = false;
        }

        private void ProcessCapturedFrame(object sender, EventArgs e) {

            // VideoCapture-objects return frames as Mat-objects, which contain the bitmap

            // Retrieve the frame from "videoCapture" and store it in "readBitmap"
            Mat mat = new Mat();
            videoCapture.Read(mat);
            Bitmap readBitmap = mat.Bitmap;
            /*
            int threshold = 100;
            var contrast = Math.Pow((100.0 + threshold) / 100.0, 2);

            Bitmap contrasted = SetContrast(readBitmap, 120);
            */
            // Display the received frame-update on gui
            DisplayOnWpfImageControl(bitmapToDisplay: readBitmap, displayWriteableBitmap: previewBitmap);
            frameToDisplay = new Bitmap(readBitmap);
            readBitmap.Dispose();
        }
        /*
        private static Bitmap SetContrast(Bitmap bmp, int threshold) {

            Bitmap procBit = new Bitmap(bmp);
            BitmapData bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            for (int i = 0; i < y + deltaY; i++) {
                for (int j = 0; j < x + deltaX; j++) {
                    int offset = y * _bitmapData.Stride + x * _bytesPerPixel;
                    blue += ptrFirstPixel[offset];
                    green += ptrFirstPixel[offset + 1];
                    red += ptrFirstPixel[offset + 2];

                    pixelCount++;
                }
            }
        }
        */
        private Color ReadColorAtPosition(ReadPosition pos) {

            if (currentFABitmap.HasValidBitmap()) {

                int absoluteX = Convert.ToInt32(pos.RelativeX * currentFABitmap.Width);
                int absoluteY = Convert.ToInt32(pos.RelativeY * currentFABitmap.Height);

                Color colorAtPos = currentFABitmap.ReadPixel(absoluteX - ReadRadius, absoluteY - ReadRadius, ReadRadius * 2 + 1, ReadRadius * 2 + 1);

                return colorAtPos;
            }

            return Color.Empty;
        }

        private void StartThread() {

            thread = new Thread(new ThreadStart(Run));
            threadShouldStop = false;
            thread.Start();
        }

        public void TryInitializeAndStart() {

            // Make the task independent from the calling gui-thread, so it wont get blocked
            Task.Run(() => {

                StopThread();

                if (Initialized = InitCameraCapture()) {
                    StartThread();
                }
            });
        }

        public void StopThread() {

            threadShouldStop = true;

            // wait for thread to stop

            if (videoCapture != null) {
                // Disable video stream
                videoCapture.ImageGrabbed -= ProcessCapturedFrame;
            }

            Initialized = false;

            // Make cameraIndex use free for other cameras
            cameraIndexesInUse.Remove(cameraIndex);
        }

        #endregion

        #region Static Functions

        private static bool CubeIsFullyScanned() {

            List<ReadPosition> allPositions = AllReadPositions();

            foreach (ReadPosition tempPos in allPositions) {

                if (tempPos.Color == Color.Empty) {
                    return false;
                }
            }

            return allPositions.Count == MAXPOSITIONSTOREAD;
        }

        private static void SortAndValidateColors() {

            List<ReadPosition> allPositions = AllReadPositions();
            // Positions left to assign
            List<ReadPosition> positions = new List<ReadPosition>(allPositions);

            // Assign color percentages to each color
            for (int i = 0; i < positions.Count; i++) {

                positions[i].Percentages = ColorIdentification.CalculateColorPercentages(positions[i].Color);
            }

            CubeColor[,] scanData = new CubeColor[6, 9];

            for (int i = 0; i < 6; i++) {
                CubeColor currentCubeColor = (CubeColor)i;

                int[] maxIndicies = ColorIdentification.Max8Indicies(currentCubeColor, positions);

                for (int j = 0; j < maxIndicies.Length; j++) {

                    ReadPosition currentPosition = positions[maxIndicies[j]];

                    // Assign tiles to the cube
                    //MainWindow.cube.SetTile((CubeFace)(currentPosition.FaceIndex), currentPosition.RowIndex * 3 + currentPosition.ColIndex, currentCubeColor);
                    scanData[currentPosition.FaceIndex, currentPosition.RowIndex * 3 + currentPosition.ColIndex] = currentCubeColor;
                    currentPosition.AssumedCubeColor = currentCubeColor;

                    // Dye the circle of the readposition in the corresponding color
                    Application.Current.Dispatcher.Invoke(() => {

                        // Change circle color of the current position on the gui
                        CircleByIndices(currentPosition.FaceIndex, currentPosition.RowIndex, currentPosition.ColIndex).Fill = Helper.ColorBrush(currentCubeColor);

                        /* Notlösung

                        // @TODO: fix color recognition and change this back again
                        CircleByIndices(currentPosition.FaceIndex, currentPosition.RowIndex, currentPosition.ColIndex).Fill = Helper.ColorBrush((
                            (MainWindow)Application.Current.MainWindow).cube.At(new Position(((CubeFace)currentPosition.FaceIndex),
                            currentPosition.RowIndex*3+currentPosition.ColIndex)));

                        */
                    });
                }

                // Sort maxIndicies -> highest value at index=0
                maxIndicies = maxIndicies.OrderByDescending(c => c).ToArray();

                // Remove all positions, that were assigned to the cube in this loop cycle
                for (int j = 0; j < maxIndicies.Length; j++) {

                    positions.RemoveAt(maxIndicies[j]);
                }
            }

            OnCubeScanned.Invoke(null, new CubeScanEventArgs(scanData));
        }

        private static List<ReadPosition> AllReadPositions() {

            List<ReadPosition> returnList = new List<ReadPosition>();

            for (int i = 0; i < PositionsToReadAt.GetLength(0); i++) {

                for (int j = 0; j < PositionsToReadAt.GetLength(1); j++) {

                    if (PositionsToReadAt[i, j] != null) {

                        returnList.Add(PositionsToReadAt[i, j]);
                    }
                }
            }

            return returnList;
        }

        public static string AddPosition(ReadPosition readPosition, int cameraIndex) {

            List<ReadPosition> slice = GetSlice(cameraIndex);


            if (PositionExists(readPosition.FaceIndex, readPosition.RowIndex, readPosition.ColIndex)) {

                return "Position not added: Entered Tile is already being scanned";
            }
            else if (slice.Count >= MAXPOSITIONSPERCAMERA) {

                return string.Format("Position not added: Camera already has {0} Tiles to scan", MAXPOSITIONSPERCAMERA);
            }
            else {

                pendingPositions[cameraIndex].Enqueue(readPosition);
                return "Position added";
            }

        }

        private static void RemovePosition(int faceIndex, int rowIndex, int colIndex) {

            for (int i = 0; i < PositionsToReadAt.GetLength(0); i++) {

                for (int j = 0; j < PositionsToReadAt.GetLength(1); j++) {

                    if (PositionsToReadAt[i, j] != null && PositionsToReadAt[i, j].FaceIndex == faceIndex && PositionsToReadAt[i, j].RowIndex == rowIndex && PositionsToReadAt[i, j].ColIndex == colIndex) {

                        PositionsToReadAt[i, j] = null;
                        TotalPositionCount--;
                        return;
                    }
                }
            }
        }

        private static bool PositionExists(int faceIndex, int rowIndex, int colIndex) {

            for (int i = 0; i < PositionsToReadAt.GetLength(0); i++) {

                for (int j = 0; j < PositionsToReadAt.GetLength(1); j++) {

                    ReadPosition pos = PositionsToReadAt[i, j];
                    if (pos == null) {

                        continue;
                    }
                    else if (pos.FaceIndex == faceIndex && pos.RowIndex == rowIndex && pos.ColIndex == colIndex) {

                        return true;
                    }
                }
            }

            return false;
        }

        public static Ellipse CircleByIndices(int faceIndex, int rowIndex, int colIndex) {

            for (int i = 0; i < PositionsToReadAt.GetLength(0); i++) {

                for (int j = 0; j < PositionsToReadAt.GetLength(1); j++) {

                        ReadPosition tempPosition = PositionsToReadAt[i, j];
                    
                    if (tempPosition != null) {

                        if (tempPosition.RowIndex == rowIndex && tempPosition.ColIndex == colIndex && tempPosition.FaceIndex == faceIndex) {

                            return tempPosition.Circle;
                        }
                    }
                }
            }

            return null;
        }

        private static List<ReadPosition> GetSlice(int index) {

            // This method will return a list with all added positions, without any "null-positions"

            List<ReadPosition> returnList = new List<ReadPosition>();

            for (int i = 0; i < PositionsToReadAt.GetLength(1); i++) {

                if (PositionsToReadAt[index, i] != null) {

                    returnList.Add(PositionsToReadAt[index, i]);
                }
            }

            return returnList;
        }

        public static void SaveAllPositionsToXml() {
            
            List<ReadPosition> allPositions = new List<ReadPosition>();
            allPositions.AddRange(GetSlice(0));
            allPositions.AddRange(GetSlice(1));
            allPositions.AddRange(GetSlice(2));
            allPositions.AddRange(GetSlice(3));

            // Make sure that all positions, even the pendings are going to be saved
            for (int i = 0; i < pendingPositions.Length; i++) {

                while (pendingPositions[i].Count > 0) {

                    allPositions.Add(pendingPositions[i].Dequeue());
                }
            }

            // Setup XmlDocument
            XDocument docToSave = new XDocument(new XElement(XmlCameraReadPositions)) {
                Declaration = new XDeclaration("1.0", "UTF-8", null)
            };

            // Store Positions for the 4 cameras in "doc"
            for (int currentCameraIndex = 0; currentCameraIndex < 4; currentCameraIndex++) {

                XElement cameraElement = new XElement(XmlCamera, new XAttribute(XmlCameraIndex, currentCameraIndex));

                foreach (ReadPosition pos in allPositions) {

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
            Log.LogMessage(string.Format("All ReadPositions were stored at \"{0}\"", PathToXml));
        }

        public static void LoadAllPositionsFromXml() {

            if (!File.Exists(PathToXml)) {
                return;
            }

            XDocument loadedDoc = XDocument.Load(PathToXml);

            IEnumerable<XElement> cameraElements = loadedDoc.Root.Elements();

            foreach (XElement cameraElement in cameraElements) {

                foreach (XElement readPositionElement in cameraElement.Elements()) {

                    var d = double.Parse(readPositionElement.Attribute(XmlRelativeX).Value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-us"));

                    Log.LogMessage(
                        AddPosition(new ReadPosition(
                            double.Parse(readPositionElement.Attribute(XmlRelativeX).Value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-us")),
                            double.Parse(readPositionElement.Attribute(XmlRelativeY).Value, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-us")),
                            int.Parse(readPositionElement.Attribute(XmlFaceIndex).Value, CultureInfo.GetCultureInfo("en-us")),
                            int.Parse(readPositionElement.Attribute(XmlRowIndex).Value, CultureInfo.GetCultureInfo("en-us")),
                            int.Parse(readPositionElement.Attribute(XmlColIndex).Value, CultureInfo.GetCultureInfo("en-us")),
                            int.Parse(cameraElement.Attribute(XmlCameraIndex).Value, CultureInfo.GetCultureInfo("en-us"))
                            ),
                            int.Parse(cameraElement.Attribute(XmlCameraIndex).Value, CultureInfo.GetCultureInfo("en-us"))
                        )
                    );
                }
            }

            Log.LogMessage(string.Format("All ReadPositions were loaded from \"{0}\"", PathToXml));
        }

        private static Ellipse DrawCircleAtPosition(ReadPosition pos, Canvas canvas) {

            Ellipse circle = null;

            Application.Current.Dispatcher.Invoke(() => {

                //circle = GenerateCircle(pos.AssumedCubeColor);
                // @TODO fix color recognition and change this back again
                circle = GenerateCircle(((MainWindow)Application.Current.MainWindow).cube.At(new Position((CubeFace)pos.FaceIndex, pos.RowIndex*3+pos.ColIndex)));

                // When you hover over the circle on the gui, you can see, which position is being read out at this position
                circle.ToolTip = new PieChart(pos);
                circle.ToolTipOpening += Circle_Tooltip;

                // Add click-eventhandling for circle
                circle.MouseUp += Circle_MouseUp;

                // Set position of circle on canvas
                Canvas.SetLeft(circle, pos.RelativeX * canvas.ActualWidth - ReadRadius);
                Canvas.SetTop(circle, pos.RelativeY * canvas.ActualHeight - ReadRadius);

                // Add circle to the canvas over the camera stream
                canvas.Children.Add(circle);
            });

            return circle;
        }

        private static void Circle_Tooltip(object sender, ToolTipEventArgs e) {

            PieChart pieChart = (PieChart)((Ellipse)sender).ToolTip;
            pieChart.Invalidate();
        }

        private static Ellipse GenerateCircle(CubeColor cc) {

            // Initialize circle, that should be drawn over camera stream
            return new Ellipse {
                Width = ReadRadius * 2 + 1,
                Height = ReadRadius * 2 + 1,
                Fill = Helper.ColorBrush(cc),
            };
        }

        private static void Circle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            if (/*!MainWindow.PositionEditingAllowed || */e.ChangedButton != System.Windows.Input.MouseButton.Right) {

                // If left-clicked, or position-adding is disabled via the gui, dont proceed
                return;
            }

            Ellipse circle = (Ellipse)sender;
            ReadPosition pos = ((PieChart)circle.ToolTip).ReadPosition;

            // Tooltip of circle looks like that: "CubeColor[RowIndex, FaceIndex]"
            
            RemovePosition(pos.FaceIndex, pos.RowIndex, pos.ColIndex);
            ((Canvas)circle.Parent).Children.Remove(circle);
        }

        public static CubeColor CubeColorByString(string cubeColorString) {

            switch (cubeColorString) {
                case "ORANGE": return CubeColor.ORANGE;
                case "WHITE": return CubeColor.WHITE;
                case "GREEN": return CubeColor.GREEN;
                case "YELLOW": return CubeColor.YELLOW;
                case "RED": return CubeColor.RED;
                case "BLUE": return CubeColor.BLUE;
                default: return CubeColor.NONE;
            }
        }

        public static void DisplayOnWpfImageControl(Bitmap bitmapToDisplay, WriteableBitmap displayWriteableBitmap) {

            if (bitmapToDisplay == null) {
                return;
            }

            Application.Current.Dispatcher.Invoke(() => {

                // Reserve the backBuffer of writeableBitmap for updates
                displayWriteableBitmap.Lock();

                // Lock "bitmapToDisplay" to be able to fast-copy the bits to the gui-image-backbuffer
                BitmapData tempData = bitmapToDisplay.LockBits(new System.Drawing.Rectangle(0, 0, bitmapToDisplay.Width, bitmapToDisplay.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // CopyMemory(destPointer, sourcePointer, byteLength to copy);
                CopyMemory(displayWriteableBitmap.BackBuffer, tempData.Scan0, displayWriteableBitmap.BackBufferStride * bitmapToDisplay.Height);

                // Unlock bitmapToDisplay for further processing
                bitmapToDisplay.UnlockBits(tempData);
                tempData = null;

                // Specify the area of the bitmap, that changed (in this case, the whole bitmap)
                displayWriteableBitmap.AddDirtyRect(new Int32Rect(0, 0, bitmapToDisplay.Width, bitmapToDisplay.Height));

                // Release the backBuffer of writeableBitmap and make it available for display
                displayWriteableBitmap.Unlock();
            });
        }

        #endregion
    }

    public class CubeScanEventArgs : EventArgs {
        public CubeColor[,] ScanData { get; }

        public CubeScanEventArgs(CubeColor[,] scanData) {
            ScanData = scanData ?? throw new ArgumentNullException(nameof(scanData));

            if (scanData.GetLength(0) != 6 || scanData.GetLength(1) != 9)
                throw new ArgumentOutOfRangeException(nameof(scanData));
        }
    }
}
