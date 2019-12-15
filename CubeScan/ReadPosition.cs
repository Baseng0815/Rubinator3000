
using System;
using System.Drawing;
using System.Windows.Shapes;

namespace Rubinator3000.CubeScan {
    class ReadPosition {

        // The Relative Coordinates are holding the percentages of the width/height and not the absolute values
        public double RelativeX { get; private set; }
        public double RelativeY { get; private set; }
        public int FaceIndex { get; private set; }
        public int RowIndex { get; private set; }
        public int ColIndex { get; private set; }
        public int CameraIndex { get; set; }
        public Color Color { get; set; } // Stores the Rgb-values of that are read out at the this position
        public Ellipse Circle { get; set; }
        public double[] Percentages { get; set; }
        public CubeColor AssumedCubeColor { get; set; }

        public ReadPosition(double relativeX, double relativeY, int faceIndex, int rowIndex, int colIndex, int cameraIndex, Color? color = null, Ellipse circle = null, double[] percentages = null, CubeColor cubeColor = CubeColor.NONE) {

            RelativeX = relativeX;
            RelativeY = relativeY;
            FaceIndex = faceIndex;
            RowIndex = rowIndex;
            ColIndex = colIndex;
            CameraIndex = cameraIndex;

            Color = color == null ? Color.Empty : color.Value;
            Circle = circle;
            Percentages = percentages;
            Percentages = percentages == null ? new double[6] : percentages;
            AssumedCubeColor = cubeColor;
        }
    }
}
