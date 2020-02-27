using Rubinator3000.CubeScan.CameraControl;
using Rubinator3000.CubeScan.ColorIdentification;
using Rubinator3000.CubeScan.RelativeElements;
using RubinatorCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Rubinator3000.CubeScan {

    public class CameraPreviewHandler {

        public CubeScanner ParentCubeScanner { get; set; }

        public ReadOnlyCollection<CameraPreview> CameraPreviews { get; private set; } = new ReadOnlyCollection<CameraPreview>(new List<CameraPreview>());

        private readonly List<Tunnel> _tunnels = new List<Tunnel>();

        public CameraPreviewHandler(CubeScanner parentCubeScanner, List<(Image Image, Canvas canvas)> outputs = null) {

            ParentCubeScanner = parentCubeScanner;
            Init(outputs);
        }

        private void Init(List<(Image Image, Canvas Canvas)> outputs) {

            if (outputs == null) {
                return;
            }

            for (int i = 0; i < outputs.Count; i++) {

                AddCameraPreview(new CameraPreview(outputs[i].Image, outputs[i].Canvas));
            }
        }

        public void AddCameraPreview(params CameraPreview[] cameraPreviews) {

            List<CameraPreview> list = CameraPreviews.ToList();
            for (int i = 0; i < cameraPreviews.Length; i++) {

                list.Insert(Convert.ToInt32(char.GetNumericValue(cameraPreviews[i].Image.Name.Last())), cameraPreviews[i]);
            }
            CameraPreviews = list.AsReadOnly();
        }

        public bool Display(WebCamControl wcc, System.Drawing.Bitmap bitmapToDisplay) {

            Tunnel tunnel = _tunnels.Where(o => o.Input == wcc.CameraIndex).FirstOrDefault();
            if (tunnel == null) {

                // Occupied Camera Preview Indices
                List<int> occupiedIndices = OccupiedIndices();

                int requestedIndex = 0;

                if (_tunnels.Count > 0) {

                    while (occupiedIndices.Contains(_tunnels[requestedIndex].Output)) {
                        requestedIndex++;
                    }
                }
                if (CameraPreviews.Count == requestedIndex) { // Check that requested index is not out of bound of "CameraPreviews"

                    return false;
                }
                else {

                    _tunnels.Add(new Tunnel(wcc.CameraIndex, requestedIndex));
                    CameraPreviews[requestedIndex].Occupant = wcc;
                    return Display(wcc, bitmapToDisplay);
                }
            }

            else {
                CameraPreviews[tunnel.Output].DisplayFrame(bitmapToDisplay);
                return true;
            }
        }

        private List<int> OccupiedIndices() {

            return _tunnels.Select(o => o.Output).ToList();
        }

        public int RequestPreviewOccupy(WebCamControl webCamControl, int index = -1) {

            if (index != -1) {

                if (CameraPreviews[index].Occupant == null) {

                    CameraPreviews[index].Occupant = webCamControl;
                    return index;
                }
                else {

                    Log.LogMessage(string.Format("Could not occupy Camera-Preview {0} - (Camera-Preview {0} occupied already)", index));
                }
            }
            else {

                for (int i = 0; i < CameraPreviews.Count; i++) {

                    if (CameraPreviews[i].Occupant == null) {

                        CameraPreviews[i].Occupant = webCamControl;
                        return i;
                    }
                }
            }

            return -1;
        }

        public void Unoccupy(WebCamControl webCamControl) {

            for (int i = 0; i < CameraPreviews.Count; i++) {

                if (CameraPreviews[i].Occupant == webCamControl) {

                    CameraPreviews[i].Occupant = null;
                    return;
                }
            }
        }

        public void SwitchPreviews(CameraPreview preview1, CameraPreview preview2) {

            if (preview1 == preview2) {

                return;
            }

            int cpi1 = CameraPreviews.IndexOf(preview1);
            int cpi2 = CameraPreviews.IndexOf(preview2);

            int tCpi1 = _tunnels.IndexOf(_tunnels.Where(o => o.Output == cpi1).FirstOrDefault());
            int tCpi2 = _tunnels.IndexOf(_tunnels.Where(o => o.Output == cpi2).FirstOrDefault());
            if (tCpi1 == -1) tCpi1 = 0;
            if (tCpi2 == -1) tCpi2 = 0;

            _tunnels[tCpi1].Output = cpi2;
            _tunnels[tCpi2].Output = cpi1;

            // Transfer Relative Canvas Elements
            List<RelativeCanvasElement> rcesCloned1 = preview1.GetRelativeCanvasChildren(true);
            preview1.SetRelativeCanvasChildren(preview2.GetRelativeCanvasChildren());
            preview2.SetRelativeCanvasChildren(rcesCloned1);
        }

        public CameraPreview PreviewByCanvas(Canvas canvas) {

            return CameraPreviews.Where(o => o.Canvas == canvas).FirstOrDefault();
        }

        #region Highlighting Tiles

        public void HighlightClosestTile(Canvas clickedCanvas, double relativeX, double relativeY) {

            CameraPreview targetCp = CameraPreviews.Where(o => o.Canvas == clickedCanvas).FirstOrDefault();
            if (targetCp != null && targetCp.Occupant != null) {

                Contour closest = targetCp.Occupant.CubeScanFrame.FindClosestContour(relativeX, relativeY);
                if (closest != null) {

                    HighlightContour(closest);
                    targetCp.AddRelativeCanvasElement(closest.ToRelativeHighlightPolygon());
                }
            }
        }

        public void HighlightContour(Contour contour) {

            // WebCamControl, whose contours-collection contains "contour"
            WebCamControl webCamControl = ParentCubeScanner.WebCamControls.Where(wcc => wcc.CubeScanFrame.TileContours.Contains(contour)).FirstOrDefault();
            // Find cameraPreview of webCamControl
            CameraPreview cameraPreview = CameraPreviews.Where(o => o.Occupant == webCamControl).FirstOrDefault();
            if (cameraPreview != null) {

                cameraPreview.AddRelativeCanvasElement(contour.ToRelativeHighlightPolygon());
            }
            else {
                Log.LogMessage("Could not highlight tile - (WebCamControl has no CameraPreview to display)");
            }
        }

        public void HighlightAll(int cameraIndex) {

            for (int i = 0; i < ParentCubeScanner.WebCamControls[cameraIndex].CubeScanFrame.TileContours.Count; i++) {

                HighlightContour(ParentCubeScanner.WebCamControls[cameraIndex].CubeScanFrame.TileContours[i]);
            }
        }

        public void ClearHighlightedTiles() {

            int index = 0;

            for (int i = 0; i < CameraPreviews.Count; i++) {
                ;

                // Remove all Polygons from canvas
                foreach (System.Windows.UIElement uiElement in CameraPreviews[i].Canvas.Children.OfType<System.Windows.Shapes.Polygon>()) {

                    CameraPreviews[i].Canvas.Children.Remove(uiElement);
                }

                // Remove all Polygons from relativeCanvasChildren-List
                CameraPreviews[i].GetRelativeCanvasChildren().RemoveAll(o => o.CanvasElement is System.Windows.Shapes.Polygon);
            }
        }

        #endregion

        public void RedrawAllCanvasElements() {

            for (int i = 0; i < CameraPreviews.Count; i++) {

                CameraPreviews[i].UpdateAllCanvasElements();
            }
        }
    }
}
