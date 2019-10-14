using System;
using System.Collections.Generic;
using System.Linq;
using static Rubinator3000.CubeColor;
using static Rubinator3000.CubeFace;

namespace Rubinator3000.Solving {
    /// <summary>
    /// Eine Hilfsstruktur, um die Verbindung von einem Kantenstein der mittleren Ebene und eines weißen Ecksteins herzustellen
    /// </summary>
    public struct FTLPair {
        private Tuple<CubeColor, CubeColor> colors;
        private readonly Cube cube;

        /// <summary>
        /// Erstellt aus den angegebenen Farben einen neuen Slot.
        /// </summary>
        /// <param name="colors">Die Farben des Slots</param>
        /// <param name="cube">Der Würfel</param>
        public FTLPair(Tuple<CubeColor, CubeColor> colors, Cube cube) {
            if (!CubeSolver.MiddleLayerEdgesColors.Any(t => t.ValuesEqual(colors)))
                throw new ArgumentOutOfRangeException();

            this.colors = colors;
            this.cube = cube;
        }

        /// <summary>
        /// Erstellt aus dem Eckstein und Kantenstein einen neuen Slot.
        /// </summary>
        /// <param name="edge">Ein Kantenstein der mittleren Ebene</param>
        /// <param name="corner">Der Eckstein der weißen Seite, der zu dem Kantenstein gehört</param>
        public FTLPair(EdgeStone edge, CornerStone corner) {
            Tuple<CubeColor, CubeColor> edgeColors = edge.Colors;
            if (!CubeSolver.MiddleLayerEdgesColors.Any(c => c.ValuesEqual(edgeColors)))
                throw new ArgumentOutOfRangeException(nameof(edge), edge, "Der Kantenstein muss ein Kantenstein der mittleren Ebene sein");

            if (!(corner.HasColor(edgeColors.Item1) && corner.HasColor(edgeColors.Item2) && corner.HasColor(WHITE)))
                throw new ArgumentOutOfRangeException(nameof(corner), corner, "Der Eckstein muss die gleichen Farben aufweisen, wie der Kantenstein");

            this.colors = edgeColors;
            this.cube = edge.GetCube();
        }

        /// <summary>
        /// Gibt den Kantenstein des Slots zurück.
        /// </summary>
        public EdgeStone Edge {
            get {
                Tuple<CubeColor, CubeColor> colors = this.colors;
                return cube.Edges.First(e => e.Colors.ValuesEqual(colors));
            }
        }

        /// <summary>
        /// Gibt den Eckstein des Slots zurück.
        /// </summary>
        public CornerStone Corner {
            get {
                Tuple<CubeColor, CubeColor> colors = this.colors;
                return cube.Corners.Where(c => c.HasColor(colors.Item1) && c.HasColor(colors.Item2) && c.HasColor(WHITE)).First();
            }
        }

        /// <summary>
        /// Gibt an, ob beide Steine korrekt gepaart sind.
        /// </summary>
        public bool Paired {
            get => IsPaired(out bool colorsRight) && colorsRight;
        }

        /// <summary>
        /// Gibt zurück, ob beide Steine in dem richtigen Slot korrekt eingesetzt sind
        /// </summary>
        public bool Solved {
            get {
                return Corner.InRightPosition() && Edge.InRightPosition();
            }
        }

        /// <summary>
        /// Gibt zurück, ob sich beide Steine auf der gelben Seite befinden
        /// </summary>
        public bool OnDownLayer {
            get {
                Position whitePos = CornerWhitePosition;
                return CubeSolver.MiddleLayerFaces.Any(f => f == whitePos.Face);
            }
        }

        /// <summary>
        /// Gibt die Position der weißen Fläche des Ecksteins zurück.
        /// </summary>
        public Position CornerWhitePosition {
            get => Corner.GetColorPosition(WHITE);
        }

        /// <summary>
        /// Gibt zurück, ob sich beide Steine nebeneinander befinden.
        /// </summary>
        /// <returns>Einen Wert, der angibt, ob sich beide Steine nebeneinander befinden</returns>
        public bool IsPaired() {
            return IsPaired(out _);
        }

        /// <summary>
        /// Gibt zurück, ob der Kantenstein mit dem Eckstein verbunden ist und gibt zusätzlich aus, ob die beiden Steine farblich
        /// richtig verbunden sind.
        /// </summary>
        /// <param name="edgeRight">Gibt an, ob die beiden Steine farblich korrekt verbunden sind</param>
        /// <returns>Gibt zurück, ob sich beide Steine nebeneinander befinden</returns>
        public bool IsPaired(out bool edgeRight) {
            // get common edge and corner positions on same face            
            IEnumerable<(Position corner, Position edge)> commonFaces = from ePos in Edge.GetPositions()
                                                                        join cPos in Corner.GetPositions() on ePos.Face equals cPos.Face
                                                                        select (cPos, ePos);
            Cube cube = this.cube;
            edgeRight = false;
            if (commonFaces.Count() == 0)
                return false;

            // check if edge and corner position are side by side 
            if (commonFaces.All(t => {
                int d = Math.Abs(t.edge.Tile - t.corner.Tile);
                return d == 1 || d == 3;
            })) {
                // return if colors are equal on each face
                edgeRight = commonFaces.All(t => cube.At(t.edge) == cube.At(t.corner));
                return true;
            }

            return false;
        }                
    }
}