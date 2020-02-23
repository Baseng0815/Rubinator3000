using DirectShowLib;
using Rubinator3000.CubeScan.CameraControl;
using Rubinator3000.CubeScan.ColorIdentification;
using Rubinator3000.CubeScan.RelativeElements;
using RubinatorCore;
using RubinatorCore.CubeRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Rubinator3000.CubeScan {
    public class CubeScanner {

        public delegate void OnCubeScannedEventHandler(object sender, CubeScanEventArgs e);
        public static event OnCubeScannedEventHandler OnCubeScanned;

        public delegate void OnTileFoundEventHandler(object sender, TileFoundEventArgs e);
        public static event OnTileFoundEventHandler OnTileFound;

        private List<CameraDevice> systemCameras = new List<CameraDevice>();

        public readonly List<WebCamControl> webCamControls = new List<WebCamControl>();
        public readonly List<CameraPreview> cameraPreviews = new List<CameraPreview>();

        public List<ReadPosition> ReadPositions { get; private set; } = new List<ReadPosition>();

        private readonly Thread thread;
        private bool threadShouldStop = true;

        public CubeScanner(List<Image> previewImages, List<Canvas> previewCanvases) {

            Init(previewImages, previewCanvases);
            thread = new Thread(Run);
            thread.Start();
        }

        private CameraDevicesUpdate UpdateSystemCameras() {

            DsDevice[] fetchedSysCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            List<CameraDevice> newSysCamsList = new List<CameraDevice>();

            for (int i = 0; i < fetchedSysCams.Length; i++) {

                Resolution res = GetHighestAvailableResolution(fetchedSysCams[i]);

                if (res == null) { // If Camera is not a usb Camera

                    fetchedSysCams[i].Dispose();
                }
                else {
                    newSysCamsList.Add(new CameraDevice(fetchedSysCams[i], res));
                }
            }

            List<int> arrivedCamerasIndices = new List<int>();
            List<int> disconnectedCamerasIndices = new List<int>();

            if (newSysCamsList.Count > 0) {

                for (int i = 0; i < newSysCamsList.Count; i++) {

                    if (i < systemCameras.Count) {

                        if (newSysCamsList[i].DsDevice.DevicePath != systemCameras[i].DsDevice.DevicePath) { // If Camera at index "i" changed

                            // Check if DevicePath is in any other DsDevice in newSystemCameras
                            if (newSysCamsList.FindIndex(cameraDevice => cameraDevice.DsDevice.DevicePath == systemCameras[i].DsDevice.DevicePath) == -1) { // If there is no camera with that DevicePath

                                disconnectedCamerasIndices.Add(i);
                            }
                            else {
                                arrivedCamerasIndices.Add(i);
                            }
                        }
                    }
                    else {

                        arrivedCamerasIndices.Add(i);
                    }
                }
            }
            else {

                if (systemCameras.Count > 0) {

                    for (int i = 0; i < systemCameras.Count; i++) {

                        disconnectedCamerasIndices.Add(i);
                    }
                }
            }
            systemCameras = newSysCamsList;
            return new CameraDevicesUpdate(arrivedCamerasIndices, disconnectedCamerasIndices);
        }

        public void HighlightClosestTile(Image clickedImage, double relativeX, double relativeY) {

            int webCamControlIndex = -1;
            for (int i = 0; i < webCamControls.Count; i++) {

                if (webCamControls[i].GetCameraPreview().Image == clickedImage) {
                    webCamControlIndex = i;
                    break;
                }
            }
            WebCamControl targetWcc = webCamControls[webCamControlIndex];
            Contour closest = targetWcc.CubeScanFrame.FindClosestContour(relativeX, relativeY);
            if (closest != null) {

                targetWcc.GetCameraPreview().AddRelativeCanvasElement(Settings.HighlightName, closest.ToRelativeHighlightPolygon());
            }
        }

        public void HighlightContour(Contour contour) {

            int webCamControlIndex = -1;
            for (int i = 0; i < webCamControls.Count; i++) {

                if (webCamControls[i].CubeScanFrame.TileContours.Contains(contour)) {
                    webCamControlIndex = i;
                    break;
                }
            }
            WebCamControl targetWcc = webCamControls[webCamControlIndex];
            targetWcc.GetCameraPreview().AddRelativeCanvasElement(Settings.HighlightName, contour.ToRelativeHighlightPolygon());
        }

        public void HighlightAll(int cameraIndex) {

            for (int i = 0; i < webCamControls[cameraIndex].CubeScanFrame.TileContours.Count; i++) {

                HighlightContour(webCamControls[cameraIndex].CubeScanFrame.TileContours[i]);
            }
        }

        public void ClearTileHighlight() {

            for (int i = 0; i < webCamControls.Count; i++) {

                for (int j = 0; j < webCamControls[i].GetCameraPreview().Canvas.Children.Count; j++) {

                    if (webCamControls[i].GetCameraPreview().Canvas.Children[j] is Polygon) {

                        webCamControls[i].GetCameraPreview().Canvas.Children.RemoveAt(j);
                        webCamControls[i].GetCameraPreview().GetClonedRelativeCanvasChildren().Remove(Settings.HighlightName);
                    }
                }
            }
        }

        public void ReadCube() {

            if (ReadPositions.Count != Settings.RequiredReadPositionCount) {

                return;
            }

            for (int i = 0; i < ReadPositions.Count; i++) {

                ReadPositions[i].Color = webCamControls[ReadPositions[i].CameraIndex].ReadColorInsideContour(ReadPositions[i].Contour);
            }

            SortAndValidateColors();
        }

        private void SortAndValidateColors() {

            // Get a deep clone of "readPositions"
            List<ReadPosition> allPositions = ReadPositions.Select(item => (ReadPosition)item.Clone()).ToList();

            // Positions left to assign
            List<ReadPosition> positionsLeftToAssign = new List<ReadPosition>(allPositions);

            // Assign color percentages to each color
            for (int i = 0; i < positionsLeftToAssign.Count; i++) {

                positionsLeftToAssign[i].Percentages = ColorID.CalculateColorPercentages(positionsLeftToAssign[i]);
            }

            // scanData stores the data of the cube that was read out
            CubeColor[][] scanData = new CubeColor[6][];
            for (int i = 0; i < scanData.Length; i++) {
                scanData[i] = new CubeColor[9];
                scanData[i][4] = (CubeColor)i;
            }

            for (int i = 0; i < 6; i++) {

                CubeColor currentCubeColor = (CubeColor)i;

                int[] maxIndicies = ColorID.Max8Indicies(currentCubeColor, positionsLeftToAssign);

                for (int j = 0; j < maxIndicies.Length; j++) {

                    ReadPosition currentPosition = positionsLeftToAssign[maxIndicies[j]];

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

            OnCubeScanned.Invoke(this, new CubeScanEventArgs(scanData));
        }

        public void RedrawAllCanvasElements() {

            for (int i = 0; i < webCamControls.Count; i++) {

                webCamControls[i].GetCameraPreview().UpdateAllCanvasElements();
            }
        }

        private void Init(List<Image> previewImages, List<Canvas> previewCanvases) {

            for (int i = 0; i < previewImages.Count; i++) {

                cameraPreviews.Add(new CameraPreview(previewImages[i], previewCanvases[i], null));
            }

            if (previewImages.Count != previewCanvases.Count) {

                Log.LogMessage("Could not initialize CubeScanner: \"previewImages.Count != previewCanvases.Count\"");
                return;
            }

            for (int i = 0; i < previewImages.Count; i++) {

                WebCamControl wcc = new WebCamControl(this, previewImages[i], previewCanvases[i], i, i);
                webCamControls.Insert(i, wcc);
            }
        }

        public void Run() {

            for (int i = 0; i < 4; i++) {

                webCamControls[i].TryInitialize(new Resolution(640, 480), i);
                webCamControls[i].StartCamera();
            }

            threadShouldStop = false;
            while (!threadShouldStop) {

                long loopStart = ReadUtility.CurrentTimeMillis();
                long loopEnd = loopStart + 1000 / Settings.TicksPerSecond;

                // Code in thread loop

                /*
                CameraDevicesUpdate cdu = UpdateSystemCameras();
                
                // Handle all arrived cameras
                for (int i = 0; i < cdu.Arrived.Count; i++) {

                    if (webCamControls[cdu.Arrived[i]].TryInitialize(systemCameras[cdu.Arrived[i]].Resolution, cdu.Arrived[i])) {

                        webCamControls[cdu.Arrived[i]].StartCamera();
                    }
                }
                // Handle all disconnected cameras
                for (int i = 0; i < cdu.Disconnected.Count; i++) {

                    webCamControls[cdu.Disconnected[i]].TerminateCamera();
                }
                // If Cube Scan should be asynchronous
                if (Settings.ReadoutRequested > 0) {

                    ReadCube();
                    if (Settings.ReadoutRequested == ReadUtility.ReadoutRequested.SINGLE_READOUT) {

                        Settings.ReadoutRequested = ReadUtility.ReadoutRequested.DISABLED;
                    }
                }
                */
                // Code in thread loop

                while (ReadUtility.CurrentTimeMillis() < loopEnd) {

                    Thread.Sleep(Convert.ToInt32(loopEnd - ReadUtility.CurrentTimeMillis()));
                }
            }
        }

        private Resolution GetHighestAvailableResolution(DsDevice vidDev) {

            Resolution HighestResolution = new Resolution(1, 1);
            int hr;

            var m_FilterGraph2 = new FilterGraph() as IFilterGraph2;
            hr = m_FilterGraph2.AddSourceFilterForMoniker(vidDev.Mon, null, vidDev.Name, out IBaseFilter sourceFilter);
            IPin pRaw2 = DsFindPin.ByCategory(sourceFilter, PinCategory.Capture, 0);
            if (pRaw2 == null) {

                //Log.LogMessage(string.Format("\"{0}\" is not a valid camera", vidDev.Name));
                return HighestResolution;
            }
            int bitCount = 0;

            VideoInfoHeader v = new VideoInfoHeader();
            IEnumMediaTypes mediaTypeEnum;
            hr = pRaw2.EnumMediaTypes(out mediaTypeEnum);

            AMMediaType[] mediaTypes = new AMMediaType[1];
            IntPtr fetched = IntPtr.Zero;
            hr = mediaTypeEnum.Next(1, mediaTypes, fetched);

            while (fetched != null && mediaTypes[0] != null) {
                Marshal.PtrToStructure(mediaTypes[0].formatPtr, v);
                if (v.BmiHeader.Size != 0 && v.BmiHeader.BitCount != 0) {
                    if (v.BmiHeader.BitCount > bitCount) {
                        bitCount = v.BmiHeader.BitCount;
                    }
                    Resolution res = new Resolution(v.BmiHeader.Width, v.BmiHeader.Height);
                    if (HighestResolution.PixelCount() < res.PixelCount()) {
                        HighestResolution = res;
                    }
                }
                hr = mediaTypeEnum.Next(1, mediaTypes, fetched);
            }
            return HighestResolution;
        }

        public void AddReadPosition(ReadPosition readPosition) {

            ReadPosition existingPosition = PositionByIndices(readPosition.FaceIndex, readPosition.RowIndex, readPosition.ColIndex);
            if (existingPosition != null) {
                ReadPositions.Remove(existingPosition);
            }
            ReadPositions.Add(readPosition);
            Log.LogMessage(string.Format("Position [{0}, {1}, {2}] added", readPosition.FaceIndex, readPosition.RowIndex, readPosition.ColIndex));

        }

        public ReadPosition PositionByIndices(int fi, int ri, int ci) {

            for (int i = 0; i < ReadPositions.Count; i++) {

                if (ReadPositions[i].FaceIndex == fi && ReadPositions[i].RowIndex == ri && ReadPositions[i].ColIndex == ci) {

                    return ReadPositions[i];
                }
            }

            return null;
        }

        public void InvokeOnTileFound(object sender, TileFoundEventArgs e) {

            OnTileFound.Invoke(sender, e);
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
    public class TileFoundEventArgs : EventArgs {
        public Contour Contour { get; }

        public int CameraIndex { get; }

        public TileFoundEventArgs(Contour contour, int cameraIndex) {
            Contour = contour;
            CameraIndex = cameraIndex;
        }
    }

    #endregion
}
