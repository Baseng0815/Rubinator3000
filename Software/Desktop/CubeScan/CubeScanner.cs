using DirectShowLib;
using Rubinator3000.CubeScan.CameraControl;
using Rubinator3000.CubeScan.ColorIdentification;
using RubinatorCore;
using RubinatorCore.CubeRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Rubinator3000.CubeScan {
    public class CubeScanner {

        public delegate void OnCubeScannedEventHandler(object sender, CubeScanEventArgs e);
        public static event OnCubeScannedEventHandler OnCubeScanned;

        private List<CameraDevice> systemCameras = new List<CameraDevice>();
        private readonly List<WebCamControl> webCamControls = new List<WebCamControl>();

        private readonly List<ReadPosition> readPositions = new List<ReadPosition>();

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

                if (webCamControls[i].CameraPreview.Image == clickedImage) {
                    webCamControlIndex = i;
                    break;
                }
            }
            WebCamControl targetWcc = webCamControls[webCamControlIndex];
            Contour closest = targetWcc.CubeScanFrame.FindClosestContour(relativeX, relativeY);
            targetWcc.CameraPreview.AddRelativeCanvasElement("TileHighlight", closest.ToRelativeHighlightPolygon(targetWcc.CameraPreview.Canvas.ActualWidth, targetWcc.CameraPreview.Canvas.ActualHeight));
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

        private void SortAndValidateColors() {

            // Get a deep clone of "readPositions"
            List<ReadPosition> allPositions = readPositions.Select(item => (ReadPosition)item.Clone()).ToList();

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

            OnCubeScanned.Invoke(null, new CubeScanEventArgs(scanData));
        }

        public void RedrawAllCanvasElements() {

            for (int i = 0; i < webCamControls.Count; i++) {

                webCamControls[i].CameraPreview.UpdateAllCanvasElements();
            }
        }

        private void Init(List<Image> previewImages, List<Canvas> previewCanvases) {

            if (previewImages.Count != previewCanvases.Count) {

                Log.LogMessage("Could not initialize CubeScanner: \"previewImages.Count != previewCanvases.Count\"");
                return;
            }

            for (int i = 0; i < previewImages.Count; i++) {

                WebCamControl wcc = new WebCamControl(previewImages[i], previewCanvases[i], i);
                webCamControls.Insert(i, wcc);
            }
        }

        public void Run() {

            threadShouldStop = false;
            while (!threadShouldStop) {

                long loopStart = ReadUtility.CurrentTimeMillis();
                long loopEnd = loopStart + 1000 / Settings.TicksPerSecond;

                // Code in thread loop

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

                // Code in thread loop

                while (ReadUtility.CurrentTimeMillis() < loopEnd) {

                    Thread.Sleep(Convert.ToInt32(loopEnd - ReadUtility.CurrentTimeMillis()));
                }
            }
        }

        private Resolution GetHighestAvailableResolution(DsDevice vidDev) {

            int hr;

            var m_FilterGraph2 = new FilterGraph() as IFilterGraph2;
            hr = m_FilterGraph2.AddSourceFilterForMoniker(vidDev.Mon, null, vidDev.Name, out IBaseFilter sourceFilter);
            IPin pRaw2 = DsFindPin.ByCategory(sourceFilter, PinCategory.Capture, 0);
            if (pRaw2 == null) {

                //Log.LogMessage(string.Format("\"{0}\" is not a valid camera", vidDev.Name));
                return null;
            }
            int bitCount = 0;
            Resolution HighestResolution = new Resolution(1, 1);

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
    #endregion
}
