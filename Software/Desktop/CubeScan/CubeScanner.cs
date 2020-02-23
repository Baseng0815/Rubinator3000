using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DirectShowLib;
using Emgu.CV;
using RubinatorCore;

namespace Rubinator3000.CubeScan {
    public class CubeScanner {

        public delegate void OnCubeScannedEventHandler(object sender, CubeScanEventArgs e);
        public static event OnCubeScannedEventHandler OnCubeScanned;

        public delegate void OnCameraDisconnectedEventHandler(object sender, CameraDisconnectedEventArgs e);
        public static event OnCameraDisconnectedEventHandler OnCameraDisconnected;

        private readonly List<CameraPreview> cameraPreviews = new List<CameraPreview>();

        private DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        private readonly List<WebCamControl2> webCamControls = new List<WebCamControl2>();

        private readonly List<ReadPosition2> readPositions = new List<ReadPosition2>();

        // Which cameraPreview-index is linked to which webCamControl-index
        // Dictionary<cameraPreview-index, webCamControl-index>
        private readonly Dictionary<int, int> cpToWccLinkings = new Dictionary<int, int>();

        private Thread thread;
        private bool threadShouldStop = true;

        public CubeScanner(List<Image> previewImages, List<Canvas> previewCanvases) {

            if (previewImages.Count != previewCanvases.Count) {

                Log.LogMessage("Could not create CubeScanner: \"previewImages.Count != previewCanvases.Count\"");
                return;
            }

            const int width = 640;
            const int height = 480;

            for (int i = 0; i < systemCameras.Length; i++) {

                WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
                previewImages[i].Source = writeableBitmap;
                cameraPreviews.Insert(i, new CameraPreview(previewImages[i], previewCanvases[i], writeableBitmap));
                webCamControls.Insert(i, new WebCamControl2(cameraPreviews[i], i));
            }
        }

        public void HighlightClosestTile(Image clickedImage, double relativeX, double relativeY) {

            int cameraPreviewIndex = -1;
            for (int i = 0; i < cameraPreviews.Count; i++) {

                if (cameraPreviews[i].Image == clickedImage) {
                    cameraPreviewIndex = i;
                    break;
                }
            }
            Contour closest = webCamControls[cpToWccLinkings[cameraPreviewIndex]].CubeScanFrame.FindClosestContour(relativeX, relativeY);

        }

        public void ReadCube() {

            if (readPositions.Count != 48) {

                return;
            }

            for (int i = 0; i < readPositions.Count; i++) {

                readPositions[i].Color = webCamControls[readPositions[i].CameraIndex].ReadColorInsideContour(readPositions[i].Contour);
            }

            SortAndValidateColors();
        }

        public void Initialized(int cameraIndex) {

        }

        private void SortAndValidateColors() {

            // Get a deep clone of "readPositions"
            List<ReadPosition2> allPositions = readPositions.Select(item => (ReadPosition2)item.Clone()).ToList();

            // Positions left to assign
            List<ReadPosition2> positionsLeftToAssign = new List<ReadPosition2>(allPositions);

            // Assign color percentages to each color
            for (int i = 0; i < positionsLeftToAssign.Count; i++) {

                positionsLeftToAssign[i].Percentages = ColorIdentification.CalculateColorPercentages(positionsLeftToAssign[i]);
            }

            // scanData stores the data of the cube that was read out
            CubeColor[][] scanData = new CubeColor[6][];
            for (int i = 0; i < scanData.Length; i++) {
                scanData[i] = new CubeColor[9];
                scanData[i][4] = (CubeColor)i;
            }

            for (int i = 0; i < 6; i++) {

                CubeColor currentCubeColor = (CubeColor)i;

                int[] maxIndicies = ColorIdentification.Max8Indicies(currentCubeColor, positionsLeftToAssign);

                for (int j = 0; j < maxIndicies.Length; j++) {

                    ReadPosition2 currentPosition = positionsLeftToAssign[maxIndicies[j]];

                    // Assign tiles to the cube
                    //MainWindow.cube.SetTile((CubeFace)(currentPosition.FaceIndex), currentPosition.RowIndex * 3 + currentPosition.ColIndex, currentCubeColor);
                    scanData[currentPosition.FaceIndex][currentPosition.RowIndex * 3 + currentPosition.ColIndex] = currentCubeColor;
                    currentPosition.AssumedCubeColor = currentCubeColor;

                    // Dye the circle of the readposition in the corresponding color
                    Application.Current.Dispatcher.Invoke(() => {

                        // Change circle color of the current position on the gui
                        currentPosition.RelativeCircle.FillColor = ReadUtility.ColorBrush(currentCubeColor).Color;
                    });
                }

                // Sort maxIndicies -> highest value at index=0
                maxIndicies = maxIndicies.OrderByDescending(c => c).ToArray();

                // Remove all positions, that were assigned to the cube in this loop cycle
                for (int j = 0; j < maxIndicies.Length; j++) {

                    positionsLeftToAssign.RemoveAt(maxIndicies[j]);
                }
            }

            OnCubeScanned.Invoke(null, new CubeScanEventArgs(scanData));
        }

        public void RedrawAllCanvasElements() {

            for (int i = 0; i < cameraPreviews.Count; i++) {

                cameraPreviews[i].UpdateAllCanvasElements();
            }
        }

        public void Run() {

            threadShouldStop = false;
            while (!threadShouldStop) {

                long loopStart = ReadUtility.CurrentTimeMillis();
                long loopEnd = loopStart + 1000 / Settings.TicksPerSecond;

                // Code in thread loop

                DsDevice[] updatedSystemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
                int newCamerasCount = updatedSystemCameras.Length - systemCameras.Length;
                if (newCamerasCount > 0) {

                    for (int i = 0; i < newCamerasCount; i++) {

                        //webCamControls[systemCameras.Length - 1 + i].Reinitialize();
                    }
                }

                // Code in thread loop

                while (ReadUtility.CurrentTimeMillis() < loopEnd) {

                    Thread.Sleep(Convert.ToInt32(loopEnd - ReadUtility.CurrentTimeMillis()));
                }
            }
        }
    }

    #region Custom Events
    public class CubeScanEventArgs : EventArgs {
        public CubeColor[][] ScanData { get; }

        public CubeScanEventArgs(CubeColor[][] scanData) {
            ScanData = scanData ?? throw new ArgumentNullException(nameof(scanData));

            if (scanData.Length != 6 || scanData[0].Length != 9)
                throw new ArgumentOutOfRangeException(nameof(scanData));
        }
    }

    public class CameraDisconnectedEventArgs : EventArgs {
        public int CameraIndex { get; }

        public CameraDisconnectedEventArgs(int cameraIndex) {

            CameraIndex = cameraIndex;
        }
    }

    #endregion
}
