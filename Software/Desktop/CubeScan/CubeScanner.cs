using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DirectShowLib;
using Emgu.CV;
using RubinatorCore;

namespace Rubinator3000.CubeScan {
    class CubeScanner {

        private DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        private List<WebCamControl2> webCamControls = new List<WebCamControl2>();

        private Thread thread;
        private bool threadShouldStop = true;

        public CubeScanner() {


        }

        private void Init() {

            thread = new Thread(Run);
            thread.Start();
        }

        private void Run() {

            threadShouldStop = true;
            while (!threadShouldStop) {

            }
        }

        public void RequestStop() {

            threadShouldStop = true;
        }

        private static void SortAndValidateColors() {

            List<ReadPosition> allPositions = AllReadPositions();
            // Positions left to assign
            List<ReadPosition> positions = new List<ReadPosition>(allPositions);

            // Assign color percentages to each color
            for (int i = 0; i < positions.Count; i++) {

                positions[i].Percentages = ColorIdentification.CalculateColorPercentages(positions[i]);
            }

            CubeColor[][] scanData = new CubeColor[6][];
            for (int i = 0; i < scanData.Length; i++) {
                scanData[i] = new CubeColor[9];
                scanData[i][4] = (CubeColor)i;
            }

            for (int i = 0; i < 6; i++) {
                CubeColor currentCubeColor = (CubeColor)i;

                int[] maxIndicies = ColorIdentification.Max8Indicies(currentCubeColor, positions);

                for (int j = 0; j < maxIndicies.Length; j++) {

                    ReadPosition currentPosition = positions[maxIndicies[j]];

                    // Assign tiles to the cube
                    //MainWindow.cube.SetTile((CubeFace)(currentPosition.FaceIndex), currentPosition.RowIndex * 3 + currentPosition.ColIndex, currentCubeColor);
                    scanData[currentPosition.FaceIndex][currentPosition.RowIndex * 3 + currentPosition.ColIndex] = currentCubeColor;
                    currentPosition.AssumedCubeColor = currentCubeColor;

                    // Dye the circle of the readposition in the corresponding color
                    Application.Current.Dispatcher.Invoke(() => {

                        // Change circle color of the current position on the gui
                        CircleByIndices(currentPosition.FaceIndex, currentPosition.RowIndex, currentPosition.ColIndex).Fill = ReadUtility.ColorBrush(currentCubeColor);
                        CircleByIndices(currentPosition.FaceIndex, currentPosition.RowIndex, currentPosition.ColIndex).Stroke = ReadUtility.ColorBrush(currentCubeColor);
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
    }
}
