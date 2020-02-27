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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Rubinator3000.CubeScan {
    public class CubeScanner {

        public delegate void OnCubeScannedEventHandler(object sender, CubeScanEventArgs e);
        public static event OnCubeScannedEventHandler OnCubeScanned;

        public delegate void OnTileFoundEventHandler(object sender, TileFoundEventArgs e);
        public static event OnTileFoundEventHandler OnTileFound;

        public CameraPreviewHandler PreviewHandler { get; private set; }

        private List<CameraDevice> systemCameras = new List<CameraDevice>();

        public List<WebCamControl> WebCamControls { get; } = new List<WebCamControl>();

        public ReadPosition[,] ReadPositions { get; set; } = new ReadPosition[6, 9];

        private readonly Thread thread;
        private bool threadShouldStop = true;

        public CubeScanner(List<(Image Image, Canvas Canvas)> outputs) {

            _ = Task.Run(Init);
            PreviewHandler = new CameraPreviewHandler(this, outputs);
            thread = new Thread(Run);
            thread.Start();
        }

        private CameraDevicesUpdate UpdateSystemCameras() {

            DsDevice[] fetchedSysCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            List<CameraDevice> newSysCamsList = new List<CameraDevice>();

            for (int i = 0; i < fetchedSysCams.Length; i++) {

                Resolution res = ReadUtility.GetHighestAvailableResolution(fetchedSysCams[i]);

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

        public void ReadCube() {

            // Check if any position is not read-out
            if (!AllPositionsAssigned()) {
                return;
            }

            for (int i = 0; i < ReadPositions.GetLength(0); i++) {
                for (int j = 0; j < ReadPositions.GetLength(1); j++) {

                    ReadPositions[i, j].Color = WebCamControls[ReadPositions[i, j].CameraIndex].ReadColorInsideContour(ReadPositions[i, j].Contour);
                }
            }

            SortAndValidateColors();
        }

        private void SortAndValidateColors() {

            // Get list of all positions
            List<ReadPosition> allPositions = new List<ReadPosition>();
            for (int i = 0; i < ReadPositions.GetLength(0); i++) for (int j = 0; j < ReadPositions.GetLength(1); j++) allPositions.Add(ReadPositions[i, j]);

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

        private void Init() {

            int i = 0;
            while (WebCamControls.Count < Settings.MaxWebCamControlCount) {

                WebCamControl wcc = new WebCamControl(this, i);
                WebCamControls.Insert(i, wcc);
                i++;
            }
        }

        public void Run() {

            bool done = false;

            threadShouldStop = false;
            while (!threadShouldStop) {

                long loopStart = ReadUtility.CurrentTimeMillis();
                long loopEnd = loopStart + 1000 / Settings.TicksPerSecond;

                // Code in thread loop

                if (!done) {

                    CameraDevicesUpdate cdu = UpdateSystemCameras();

                    // Handle all arrived cameras
                    for (int i = 0; i < cdu.Arrived.Count; i++) {

                        if (WebCamControls[cdu.Arrived[i]].TryInitialize(cdu.Arrived[i])) {

                            WebCamControls[cdu.Arrived[i]].StartCamera();
                            done = true;
                        }
                    }
                    // Handle all disconnected cameras
                    for (int i = 0; i < cdu.Disconnected.Count; i++) {

                        WebCamControls[cdu.Disconnected[i]].TerminateCamera();
                    }
                    /*// If Cube Scan should be asynchronous
                    if (Settings.ReadoutRequested > 0) {

                        ReadCube();
                        if (Settings.ReadoutRequested == ReadUtility.ReadoutRequested.SINGLE_READOUT) {

                            Settings.ReadoutRequested = ReadUtility.ReadoutRequested.DISABLED;
                        }
                    }*/
                }

                // Code in thread loop

                while (ReadUtility.CurrentTimeMillis() < loopEnd) {

                    Thread.Sleep(Convert.ToInt32(loopEnd - ReadUtility.CurrentTimeMillis()));
                }
            }
        }

        public void AddReadPosition(ReadPosition readPosition) {

            ReadPositions[readPosition.FaceIndex, readPosition.RowIndex * 3 + readPosition.ColIndex] = readPosition;
            Log.LogMessage(string.Format("Position [{0}, {1}, {2}] added", readPosition.FaceIndex, readPosition.RowIndex, readPosition.ColIndex));

        }

        public void RemoveReadPosition(ReadPosition readPosition) {

            ReadPositions[readPosition.FaceIndex, readPosition.RowIndex * 3 + readPosition.ColIndex] = null;
        }

        public ReadPosition ReadPositionByIndices(int fi, int ri, int ci) {

            return ReadPositions[fi, ri * 3 + ci];
        }

        public bool AllPositionsAssigned() {

            for (int i = 0; i < ReadPositions.GetLength(0); i++) {
                for (int j = 0; j < ReadPositions.GetLength(1); j++) {

                    if (ReadPositions[i, j] == null) {

                        return false;
                    }
                }
            }
            return true;
        }

        public void InvokeOnTileFound(object sender, TileFoundEventArgs e) {

            //OnTileFound.Invoke(sender, e);
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
