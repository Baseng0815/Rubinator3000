using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RubinatorCore.CubeFace;
using static RubinatorCore.CubeColor;

namespace RubinatorCore.Solving {

    /// <summary>
    /// Stellt Methoden und Eigenschaften bereit, um Patterns anzuwenden und zu organisieren
    /// </summary>
    public interface IPattern {
        /// <summary>
        /// Die Nummer des Patterns
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Überprüft, ob das Pattern der aktuellen Konstellation des Würfels entspricht
        /// </summary>
        /// <param name="cube">Der zu überprüfende Würfel</param>
        /// <returns>Einen Wert, der angibt, ob das Pattern dem Würfel entspricht</returns>
        bool IsMatch(Cube cube);
    }

    /// <summary>
    /// Stellt ein OLL Pattern dar
    /// </summary>
    public struct OllPattern : IPattern {
        /// <summary>
        /// Die Konstellation der gelben Seite
        /// </summary>
        private bool[] face;

        /// <summary>
        /// Die Orientierung der gelben Flächen, die nicht auf der gelben Seite sind
        /// </summary>
        private bool[][] sides;

        /// <summary>
        /// Die Nummer des Patterns
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Erstellt ein neues OLL Pattern
        /// </summary>
        /// <param name="number">Die Nummer des Patterns</param>
        /// <param name="face">Die Konstellation der gelben Seite</param>
        /// <param name="sides">Die Orientierung der gelben Flächen, die nicht auf der gelben Seite sind</param>
        public OllPattern(int number, bool[] face, bool[][] sides) {
            // Die Länge der Arrays überprüfen
            if (face.Length != 9)
                throw new ArgumentOutOfRangeException(nameof(face));

            if (sides.Length != 4 || sides.Any(e => e.Length != 3))
                throw new ArgumentOutOfRangeException(nameof(sides));

            // die Felder initalisieren
            this.face = face;
            this.sides = sides;
            this.Number = number;
        }

        /// <summary>
        /// Überprüft, ob das Pattern der aktuellen Konstellation des Würfels entspricht
        /// </summary>
        /// <param name="cube">Der zu überprüfende Würfel</param>
        /// <returns>Einen Wert, der angibt, ob das Pattern dem Würfel entspricht</returns>
        public bool IsMatch(Cube cube) {
            // die gelbe Seite überprüfen
            for (int t = 0; t < 9; t++) {
                if ((cube.At(DOWN, t) == YELLOW) != face[t])
                    return false;
            }

            // die seitlichen Flächen überprüfen
            for (int f = 0; f < 4; f++) {
                var face = CubeSolver.MiddleLayerFaces[f];
                for (int i = 0; i < 3; i++) {
                    if ((cube.At(face, i + 6) == YELLOW) != sides[f][i])
                        return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Stellt ein PLL Pattern dar
    /// </summary>
    public struct PllPattern : IPattern {
        /// <summary>
        /// Die Nummer des Patterns
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Die Farbdifferenzen der Steine
        /// </summary>
        private byte[][] patternData;        

        /// <summary>
        /// Die Farben der mittleren Seiten
        /// </summary>
        private static readonly Dictionary<CubeColor, int> middleLayerColors;

        /// <summary>
        /// Ein statischer Konstruktor, der statische Felder intialisiert
        /// </summary>
        static PllPattern() {
            middleLayerColors = new Dictionary<CubeColor, int>();

            // die Farben der mittleren Seiten hinzufügen
            middleLayerColors.Add(ORANGE, 0);
            middleLayerColors.Add(GREEN, 1);
            middleLayerColors.Add(RED, 2);
            middleLayerColors.Add(BLUE, 3);
        }

        /// <summary>
        /// Erstellt ein neues PLL Pattern
        /// </summary>
        /// <param name="number">Die Nummer des Patterns</param>
        /// <param name="patternData">Die Farbdifferenzen der Steine</param>
        public PllPattern(int number, byte[][] patternData) {
            // die Länge des Arrays überprüfen
            if (patternData.Length != 4 || patternData.Any(e => e.Length != 3))
                throw new ArgumentOutOfRangeException(nameof(patternData));

            // die Felder initalisieren
            this.patternData = patternData;
            this.Number = number;
        }

        /// <summary>
        /// Überprüft, ob das Pattern der aktuellen Konstellation des Würfels entspricht
        /// </summary>
        /// <param name="cube">Der zu überprüfende Würfel</param>
        /// <returns>Einen Wert, der angibt, ob das Pattern dem Würfel entspricht</returns>
        public bool IsMatch(Cube cube) {
            // die seitlichen Seiten überprüfen
            for (int f = 0; f < 4; f++) {
                CubeFace face = CubeSolver.MiddleLayerFaces[f];

                // die Farbwerte der seitlichen Flächen auf der unteren Ebene bestimmen
                CubeColor[] tiles = {
                    cube.At(face, 6),
                    cube.At(face, 7),
                    cube.At(face, 8)
                };

                // die Farbdifferenzen berechnen
                int delta0 = (SolvingUtility.NormalizeCount(middleLayerColors[tiles[0]] - middleLayerColors[tiles[1]]) + 4) % 4;
                int delta1 = (SolvingUtility.NormalizeCount(middleLayerColors[tiles[0]] - middleLayerColors[tiles[2]]) + 4) % 4;
                int delta2 = (SolvingUtility.NormalizeCount(middleLayerColors[tiles[1]] - middleLayerColors[tiles[2]]) + 4) % 4;

                // die Farbdifferenzen mit den Differenzen des Patterns vergleichen
                if (delta0 != patternData[f][0] || delta1 != patternData[f][1] || delta2 != patternData[f][2])
                    return false;
            }

            return true;
        }
    }
}