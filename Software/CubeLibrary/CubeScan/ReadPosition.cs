
using System;
using System.Drawing;
using System.Windows.Shapes;

namespace CubeLibrary.CubeScan {
    public class ReadPosition {

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

        // Currently not in use
        public ReadPosition Clone() {

            // Creates a clean clone without any references to the old ReadPosition

            return new ReadPosition(
                Convert.ToDouble(RelativeX),
                Convert.ToDouble(RelativeY),
                Convert.ToInt32(FaceIndex),
                Convert.ToInt32(RowIndex),
                Convert.ToInt32(ColIndex),
                Convert.ToInt32(CameraIndex),
                Color.FromArgb(Convert.ToInt32(Color.R), Convert.ToInt32(Color.G), Convert.ToInt32(Color.B)),
                new Ellipse() {
                    Width = Convert.ToDouble(Circle.Width),
                    Height = Convert.ToDouble(Circle.Height),
                    Stroke = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(
                            Convert.ToByte(((System.Windows.Media.SolidColorBrush)Circle.Stroke).Color.A),
                            Convert.ToByte(((System.Windows.Media.SolidColorBrush)Circle.Stroke).Color.R),
                            Convert.ToByte(((System.Windows.Media.SolidColorBrush)Circle.Stroke).Color.G),
                            Convert.ToByte(((System.Windows.Media.SolidColorBrush)Circle.Stroke).Color.B))),
                    StrokeThickness = Convert.ToDouble(Circle.StrokeThickness)
                },
                (double[])Percentages.Clone(),
                AssumedCubeColor
            );
        }
    }
}
