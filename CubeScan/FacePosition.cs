using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.CubeScan
{
    class FacePosition
    {
        // The Relative Coordinates are holding the percentages of the width/height and not the absolute values
        public double RelativeX { get; private set; }
        public double RelativeY { get; private set; }
        public int FaceIndex { get; private set; }
        public int RowIndex { get; private set; }
        public int ColIndex { get; private set; }

        public bool IsCircleDrawn { get; set; } = false;

        public FacePosition(double relativeX, double relativeY, int faceIndex, int rowIndex, int colIndex)
        {
            RelativeX = relativeX;
            RelativeY = relativeY;
            FaceIndex = faceIndex;
            RowIndex = rowIndex;
            ColIndex = colIndex;
        }
    }
}
