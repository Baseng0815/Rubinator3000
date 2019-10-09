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

        /// <summary>
        /// Gibt den Unterschied der Farben zurück. Der Unterschied ist die Anzahl der Seite im Uhrzeigersinn, um die Farbe auf 
        /// die richtige Seite zu bringen. Er wird durch die Differenz von der Seitenfarbe und der Farbe bestimmt.
        /// </summary>
        /// <param name="color">Die Farbe</param>
        /// <param name="faceColor">Die Seite auf der sich die Farbe befindet</param>
        /// <param name="faceToRot">Die Seite, die gedreht werden soll, um die Farbe auf die richtige Seite zu bringen</param>
        /// <returns></returns>
        public static int GetDelta(this CubeColor color, CubeColor faceColor, CubeFace faceToRot) {
            CubeColor[] colors = layerColors[(int)faceToRot];

            if (Cube.IsOpponentColor(faceColor, Cube.GetFaceColor(faceToRot)) || faceColor == Cube.GetFaceColor(faceToRot))
                throw new ArgumentOutOfRangeException();

            if (!colors.Contains(faceColor))
                throw new ArgumentOutOfRangeException(nameof(faceColor));

            if (!colors.Contains(color))
                throw new ArgumentOutOfRangeException(nameof(faceToRot));

            int delta = Array.IndexOf(colors, faceColor) - Array.IndexOf(colors, color);
            return SolvingUtility.NormalizeCount(delta);
        }        

        public static int NormalizeCount(int count) {
            return NormalizeCount(count, 0);
        }

        public static int NormalizeCount(int count, int minCount) {
            while (count < minCount) count += 4;

            if(count < 0)
                return -(int)(Math.Abs(count) % 4);
            else 
                return count;
        }
    }
}
