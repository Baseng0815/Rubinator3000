using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubinatorCore.Solving {
    public static class SolvingUtility {
        /// <summary>
        /// Die Farben der angrenzenden Seiten
        /// </summary>
        private static readonly CubeColor[][] layerColors = new CubeColor[6][] {
            // Links
            new CubeColor[4] { CubeColor.WHITE, CubeColor.BLUE, CubeColor.YELLOW, CubeColor.GREEN },
            // Oben
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.GREEN, CubeColor.RED, CubeColor.BLUE },
            // Vorne
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.YELLOW, CubeColor.RED, CubeColor.WHITE },
            // Unten
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.BLUE, CubeColor.RED, CubeColor.GREEN },
            // Rechts
            new CubeColor[4] { CubeColor.WHITE, CubeColor.GREEN, CubeColor.YELLOW, CubeColor.BLUE },
            // Hinten
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.WHITE, CubeColor.RED, CubeColor.YELLOW }
        };

        /// <summary>
        /// Normiert den Wert zwischen 1 und 4
        /// </summary>
        /// <param name="count">Der zu normierende Wert</param>
        /// <returns>Den normierten Wert</returns>
        public static int NormalizeCount(int count) {
            while (count < -1) count += 4;

            return (count + 1) % 4 - 1;
        }

        #region Extension Methodes
        /// <summary>
        /// Gibt den Unterschied der Farben zurück. Der Unterschied ist die Anzahl der Dehungen der Seite im Uhrzeigersinn, um die Farbe auf 
        /// die richtige Seite zu bringen. Er wird durch die Differenz von der Seitenfarbe und der Farbe bestimmt.
        /// </summary>
        /// <param name="color">Die Farbe</param>
        /// <param name="face">Die Seite auf der sich die Farbe befindet</param>
        /// <param name="faceToRot">Die Seite, die gedreht werden soll, um die Farbe auf die richtige Seite zu bringen</param>
        /// <returns>Einen Wert, der den Unterschied der Farben darstellt</returns>
        public static int GetDelta(this CubeColor color, CubeFace face, CubeFace faceToRot) {
            // die Farben der angrenzenden Seiten und der Seite auf der sich die Farbe befindet bestimmen
            CubeColor[] colors = layerColors[(int)faceToRot];
            CubeColor faceColor = Cube.GetFaceColor(face);

            // die Parameter überprüfen
            if (Cube.IsOpponentColor(faceColor, Cube.GetFaceColor(faceToRot)) || faceColor == Cube.GetFaceColor(faceToRot))
                throw new ArgumentOutOfRangeException();

            if (!colors.Contains(faceColor))
                throw new ArgumentOutOfRangeException(nameof(faceColor));

            if (!colors.Contains(color))
                throw new ArgumentOutOfRangeException(nameof(faceToRot));

            // das Delta berechnen und zurückgeben
            int delta = Array.IndexOf(colors, color) - Array.IndexOf(colors, faceColor);
            return NormalizeCount(delta);
        }

        /// <summary>
        /// Gibt die Farbe einer Seite zurück
        /// </summary>
        /// <param name="face">Die Seite, deren Farbe bestimmt werden soll</param>
        /// <returns>Die Farbe der Seite</returns>
        public static CubeColor GetFaceColor(this CubeFace face) {
            return Cube.GetFaceColor(face);
        }

        /// <summary>
        /// Gibt die Seite einer Farbe zurück
        /// </summary>
        /// <param name="color">Die Farbe, deren Seite bestimmt werden soll</param>
        /// <returns>Die Seite der Farbe</returns>
        public static CubeFace GetFace(this CubeColor color) {
            return Cube.GetFace(color);
        }       
        #endregion
    }
}
