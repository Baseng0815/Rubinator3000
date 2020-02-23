using RubinatorCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Rubinator3000.CubeScan {
    class ReadPosition2 : ICloneable {

        public Contour Contour { get; set; }

        public int FaceIndex { get; set; }

        public int RowIndex { get; set; }

        public int ColIndex { get; set; }

        public int CameraIndex { get; set; }

        public Color Color { get; set; } // Stores the Rgb-values of that are read out at the this position

        public RelativeCircle RelativeCircle { get; set; }

        public double[] Percentages { get; set; }

        public CubeColor AssumedCubeColor { get; set; }

        public ReadPosition2(Contour contour, int faceIndex, int rowIndex, int colIndex, int cameraIndex, Color? color = null, RelativeCircle circle = null, double[] percentages = null, CubeColor assumedCubeColor = CubeColor.NONE) {

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

            return new ReadPosition2((Contour)Contour.Clone(), FaceIndex, RowIndex, ColIndex, CameraIndex, Color, RelativeCircle, (double[])Percentages.Clone(), AssumedCubeColor);
        }
    }
}
