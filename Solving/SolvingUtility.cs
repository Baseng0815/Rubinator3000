using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.Solving {
    public static class SolvingUtility {
        private static readonly CubeColor[][] layerColors = new CubeColor[6][] {
            // LEFT
            new CubeColor[4] { CubeColor.WHITE, CubeColor.BLUE, CubeColor.YELLOW, CubeColor.GREEN },
            // UP
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.GREEN, CubeColor.RED, CubeColor.BLUE },
            // FRONT
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.YELLOW, CubeColor.RED, CubeColor.WHITE },
            // DOWN
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.BLUE, CubeColor.RED, CubeColor.GREEN },
            // RIGHT
            new CubeColor[4] { CubeColor.WHITE, CubeColor.GREEN, CubeColor.YELLOW, CubeColor.BLUE },
            // BACK
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.WHITE, CubeColor.RED, CubeColor.YELLOW }
        };

        public static int GetDelta(this CubeColor color, CubeColor faceColor, CubeFace faceToRot) {
            CubeColor[] colors = layerColors[(int)faceToRot];

            if (Cube.IsOpponentColor(faceColor, Cube.GetFaceColor(faceToRot)) || faceColor == Cube.GetFaceColor(faceToRot))
                throw new ArgumentOutOfRangeException();

            if (!colors.Contains(faceColor))
                throw new ArgumentOutOfRangeException(nameof(faceColor));

            if (!colors.Contains(color))
                throw new ArgumentOutOfRangeException(nameof(faceToRot));

            int delta = Array.IndexOf(colors, faceColor) - Array.IndexOf(colors, color);
            return delta.NomalizeCount();
        }

        /// <summary>
        /// Bringt den Wert in den Bereich von 0 bis 3
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int NomalizeCount(this int count) {
            while (count < 0) count += 4;
            return count % 4;
        }
    }
}
