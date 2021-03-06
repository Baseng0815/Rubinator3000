using System;
using System.Collections.Generic;
using System.Linq;
using static RubinatorCore.CubeColor;
using static RubinatorCore.CubeFace;

namespace RubinatorCore.Solving {
    /// <summary>
    /// Eine Hilfsstruktur, um die Verbindung von einem Kantenstein der mittleren Ebene und eines wei�en Ecksteins herzustellen
    /// </summary>
    public struct F2LPair {
        /// <summary>
        /// Die Steine des Paares
        /// </summary>
        private (EdgeStone edge, CornerStone corner) stones;
        /// <summary>
        /// Der W�rfel, zu dem die Steine geh�ren
        /// </summary>
        private readonly Cube cube;

        /// <summary>
        /// Erstellt aus den angegebenen Steinen einen neuen Slot.
        /// </summary>
        /// <param name="corner">Der Eckstein</param>
        /// <param name="edge">Der Kantenstein</param>        
        /// <param name="cube">Der W�rfel</param>
        public F2LPair(CornerStone corner, EdgeStone edge, Cube cube) {
            this.stones = (edge, corner);
            this.cube = cube;
        }

        /// <summary>
        /// Bestimmt ein F2L-Paar mithilfe eines Steins
        /// </summary>
        /// <param name="stone">Der Eck- oder Kantenstein</param>
        /// <param name="cube">Der W�rfe, zu dem der Stein geh�rt</param>
        /// <returns>Ein F2L-Paar mit den Farben des Steins</returns>
        public static F2LPair GetPair(IStone stone, Cube cube) {
            if (stone is CornerStone corner) {
                if (!corner.HasColor(WHITE))
                    throw new ArgumentException();

                var colors = corner.GetColors().Where(c => c != WHITE);
                EdgeStone edge = cube.Edges.First(e => e.GetColors().All(c => colors.Contains(c)));

                return new F2LPair(corner, edge, cube);
            }
            else if (stone is EdgeStone edge) {
                if (edge.HasColor(WHITE) || edge.HasColor(YELLOW))
                    throw new ArgumentException();

                var colors = edge.GetColors();
                corner = cube.Corners.First(cr => cr.GetColors().All(c => colors.Contains(c)) && cr.HasColor(WHITE));

                return new F2LPair(corner, edge, cube);
            }
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// Gibt den Kantenstein des Slots zur�ck.
        /// </summary>
        public EdgeStone Edge {
            get => stones.edge;
        }

        /// <summary>
        /// Gibt den Eckstein des Slots zur�ck.
        /// </summary>
        public CornerStone Corner {
            get => stones.corner;
        }

        /// <summary>
        /// Gibt an, ob beide Steine korrekt gepaart sind.
        /// </summary>
        public bool Paired {
            get => IsPaired(out bool colorsRight, out bool cornerRight) && colorsRight && cornerRight;
        }

        /// <summary>
        /// Gibt zur�ck, ob beide Steine in dem richtigen Slot korrekt eingesetzt sind
        /// </summary>
        public bool Solved {
            get {
                return Corner.InRightPosition() && Edge.InRightPosition();
            }
        }

        /// <summary>
        /// Gibt zur�ck, ob sich beide Steine auf der gelben Seite befinden
        /// </summary>
        public bool OnDownLayer {
            get {
                Position whitePos = CornerWhitePosition;
                return Edge.GetPositions().Any(p => p.Face == DOWN) && Corner.GetPositions().Any(p => p != whitePos && p.Face == DOWN);
            }
        }

        /// <summary>
        /// Gibt die Position der wei�en Fl�che des Ecksteins zur�ck.
        /// </summary>
        public Position CornerWhitePosition {
            get => Corner.GetColorPosition(WHITE);
        }

        /// <summary>
        /// Gibt an, ob sich der Kantenstein des Paares in einem Slot befindet
        /// </summary>
        public bool EdgeInSlot {
            get => stones.edge.GetPositions().All(p => p.Face != DOWN);
        }

        /// <summary>
        /// Gibt zur�ck, ob sich beide Steine nebeneinander befinden.
        /// </summary>
        /// <returns>Einen Wert, der angibt, ob sich beide Steine nebeneinander befinden</returns>
        public bool IsPaired() {
            return IsPaired(out _, out _);
        }

        /// <summary>
        /// Gibt zur�ck, ob der Kantenstein mit dem Eckstein verbunden ist und gibt zus�tzlich aus, ob die beiden Steine farblich
        /// richtig verbunden sind.
        /// </summary>
        /// <param name="edgeRight">Gibt an, ob die beiden Steine farblich korrekt verbunden sind</param>
        /// <returns>Gibt zur�ck, ob sich beide Steine nebeneinander befinden</returns>
        public bool IsPaired(out bool edgeRight, out bool cornerRight) {            
            // die Positionen auf den gemeinsamen Seiten bestimmen
            IEnumerable<(Position corner, Position edge)> commonFaces = from ePos in Edge.GetPositions()
                                                                        join cPos in Corner.GetPositions() on ePos.Face equals cPos.Face
                                                                        select (cPos, ePos);
            Cube cube = this.cube;
            edgeRight = false;
            cornerRight = false;
            if (commonFaces.Count() == 0 || commonFaces.Count() == 1)
                return false;

            // �berpr�fen ob die gemeinsamen Seitenpositionen nebeneinander liegen
            if (commonFaces.All(t => {
                int d = Math.Abs(t.edge.Tile - t.corner.Tile);
                return d == 1 || d == 3;
            })) {
                IStone Corner = this.Corner;
                // zur�ckgeben, ob die nebeneinander liegenden Farben gleich sind
                edgeRight = commonFaces.All(t => cube.At(t.edge) == cube.At(t.corner));
                cornerRight = !commonFaces.Any(t => t.corner == Corner.GetColorPosition(WHITE));
                return true;
            }

            return false;
        }        

        /// <summary>
        /// �berpr�ft F2L-Paare auf Gleichheit
        /// </summary>        
        /// <returns>Einen Wert, der angibt, ob die beiden Paare gleich sind</returns>
        public override bool Equals(object obj) {
            return obj is F2LPair pair &&
                   EqualityComparer<Cube>.Default.Equals(cube, pair.cube) &&
                   EqualityComparer<EdgeStone>.Default.Equals(Edge, pair.Edge) &&
                   EqualityComparer<CornerStone>.Default.Equals(Corner, pair.Corner);
        }

        public override int GetHashCode() {
            var hashCode = 198864972;
            hashCode = hashCode * -1521134295 + EqualityComparer<Cube>.Default.GetHashCode(cube);
            hashCode = hashCode * -1521134295 + EqualityComparer<EdgeStone>.Default.GetHashCode(Edge);
            hashCode = hashCode * -1521134295 + EqualityComparer<CornerStone>.Default.GetHashCode(Corner);
            return hashCode;
        }

        public override string ToString() {
            return Edge.ToString();
        }

        public static bool operator ==(F2LPair left, F2LPair right) {
            return left.Corner.GetColors().All(c => right.Corner.GetColors().Contains(c));
        }

        public static bool operator !=(F2LPair left, F2LPair right) {
            return !(left == right);
        }
    }
}