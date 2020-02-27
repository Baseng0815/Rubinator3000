using Rubinator3000.CubeScan.RelativeElements;
using RubinatorCore.CubeRepresentation;
using System;
using System.Drawing;

namespace Rubinator3000.CubeScan.ColorIdentification {
    public class ReadPosition : ICloneable {

        public int FaceIndex { get; set; }

        public int RowIndex { get; set; }

        public int ColIndex { get; set; }

        public int CameraIndex { get; set; }

        public Contour Contour { get; set; }

        public Color Color { get; set; } // Stores the Rgb-values of that are read out at the this position

        public RelativeCircle RelativeCircle { get; set; }

        public double[] Percentages { get; set; }

        public CubeColor AssumedCubeColor { get; set; }

        public ReadPosition(int faceIndex, int rowIndex, int colIndex, int cameraIndex, Contour contour, RelativeCircle circle = null, Color? color = null, double[] percentages = null, CubeColor assumedCubeColor = CubeColor.NONE) {

            Contour = contour;
            FaceIndex = faceIndex;
            RowIndex = rowIndex;
            ColIndex = colIndex;
            CameraIndex = cameraIndex;
            Color = color == null ? Color.Empty : color.Value;
            RelativeCircle = circle;
            Percentages = percentages;
            AssumedCubeColor = assumedCubeColor;
        }

        public object Clone() {

            return new ReadPosition(
                FaceIndex,
                RowIndex,
                ColIndex,
                CameraIndex,
                (Contour)Contour.Clone(),
                RelativeCircle,
                Color,
                Percentages == null ? null : (double[])Percentages.Clone(),
                AssumedCubeColor);
        }
    }
}
