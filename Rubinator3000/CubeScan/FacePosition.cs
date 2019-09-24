using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.CubeScan
{
    class FacePosition
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int FaceIndex { get; private set; }
        public int RowIndex { get; private set; }
        public int ColIndex { get; private set; }

        public FacePosition(int x, int y, int faceIndex, int rowIndex, int colIndex)
        {
            X = x;
            Y = y;
            FaceIndex = faceIndex;
            RowIndex = rowIndex;
            ColIndex = colIndex;
        }
    }
}
