using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {

    public class CrossSolver : CubeSolver {
        protected EdgeStone pivotStone;

        /// <summary>
        /// Gibt an, ob das weiße Kreuz gelöst ist
        /// </summary>
        public override bool Solved {
            get => whiteEdges.All(e => e.InRightPosition());
        }


        protected IEnumerable<EdgeStone> whiteEdges {
            get => cube.Edges.Where(e => e.HasColor(WHITE));
        }

        public CrossSolver(Cube cube) : base(cube) { }

        protected override void CalcMoves() {
            // die Steine in die richtige Orientierung bringen
            OrientateStones();

            // überprüfen, ob sich ein Kantenstein auf der weißen Seite befindet
            if (whiteEdges.Any(e => e.GetColorPosition(WHITE).Face == UP)) {
                int[] count = CountRightStones();

                int maxCount = count.Max();
                // die weiße Seite so drehen, dass die meisten Steine stimmen
                int rotCount = Array.IndexOf(count, maxCount);
                DoMove(UP, rotCount);

                // das Pivot Element festlegen
                pivotStone = whiteEdges.First(e => e.InRightPosition());
            }
            else {
                CubeFace faceToRot; int count;

                // das Pivot Element "zufällig" festlegen
                // und das Pivot Element auf die weiße Seite bringen

                // überprüfen, ob sich ein Stein auf der mittleren Ebene befindet                
                Func<EdgeStone, bool> predicate = e => MiddleLayerFaces.Contains(e.GetColorPosition(WHITE).Face);
                if (whiteEdges.Any(predicate)) {
                    pivotStone = whiteEdges.First(predicate);

                    faceToRot = pivotStone.GetColorPosition(c => c != WHITE).Face;
                    count = pivotStone.GetColorPosition(c => c != WHITE).Tile == 3 ? -1 : 1;
                }
                // alle weißen Steine auf der gelben Seite
                else {
                    pivotStone = whiteEdges.FirstOrDefault(e => e.GetColorPosition(c => c != WHITE).Face == Cube.GetFace(e.GetColors().First(c => c != WHITE)));

                    faceToRot = pivotStone.GetColorPosition(c => c != WHITE).Face;
                    count = 2;
                }

                DoMove(faceToRot, count);
            }

            MoveStonesToWhiteFace();

            SortStones();
        }


        /// <summary>
        /// Bringt die weißen Kantensteine in die richtige Orientierung
        /// </summary>
        protected void OrientateStones() {
            // alle 4 seitlichen Seiten überprüfen
            for (int f = 0; f < 4; f++) {
                CubeFace face = MiddleLayerFaces[f];

                // überprüfen, ob sich weiße Kantensteine in falscher Orientierung befinden
                if (cube.At(face, 1) == WHITE || cube.At(face, 7) == WHITE) {

                    // richtig orientierte Steine in Sicherheit bringen
                    while (cube.At(face, 3) == WHITE) {

                        // die von aktueller Seite linke Seite drehen
                        var faceToRot = MiddleLayerFaces[(f + 3) % 4];
                        DoMove(faceToRot);
                    }

                    while (cube.At(face, 5) == WHITE) {

                        // die von aktueller Seite rechte Seite drehen
                        var faceToRot = MiddleLayerFaces[(f + 1) % 4];
                        DoMove(faceToRot);
                    }

                    // die falsch orientierten Steine richtig orientieren
                    DoMove(face);
                }
            }
        }

        /// <summary>
        /// Dreht die weiße Seite 4 mal und zählt jeweils die richtigen weißen Kantensteine
        /// </summary>
        /// <returns>Ein Array mit den jeweiligen Anzahlen der richtigen Steine</returns>
        protected int[] CountRightStones() {
            int[] count = new int[4];
            for (int i = 0; i < 4; i++) {
                count[i] = whiteEdges.Count(e => e.InRightPosition());
                DoMove(UP);
            }

            return count;
        }

        /// <summary>
        /// Bringt die weißen Kantensteine in die richtige Position
        /// </summary>
        protected void MoveStonesToWhiteFace() {
            Func<EdgeStone, bool> predicate = e => e.GetColorPosition(WHITE).Face != UP;

            while (whiteEdges.Any(predicate)) {
                EdgeStone edge = whiteEdges.First(predicate);

                // die weiße Seite ricthig ausrichten
                int delta = GetDelta(edge);
                int pivotDelta = GetDelta(pivotStone);
                while (delta != pivotDelta)
                    DoMove(UP);

                // die Seite und die Anzahl der Drehungen bestimmen
                Position secndPosition = edge.GetColorPosition(c => c != WHITE);
                CubeFace faceToRot = secndPosition.Face;
                int count;

                // die zweite Positon des Kantensteins gibt die Drehrichtung vor
                switch (secndPosition.Tile) {
                    // links
                    case 3:
                        count = 1;
                        break;
                    // rechts
                    case 5:
                        count = -1;
                        break;
                    // unten
                    case 7:
                        count = 2;
                        break;
                    default:
                        throw new InvalidProgramException();
                }

                DoMove(faceToRot, count);
            }
        }        

        /// <summary>
        /// Sortiert die restlichen falschen Steine auf der weißen Seite
        /// </summary>
        protected void SortStones() {
            Func<EdgeStone, bool> predicate = e => IsWhiteStoneRight(e, in pivotStone);

            while (whiteEdges.Any(predicate)) {
                EdgeStone edge = whiteEdges.First(predicate);

                // den falschen Kantenstein von der weißen Seite bewegen
                CubeFace faceToRot = edge.GetColorPosition(c => c != WHITE).Face;
                DoMove(faceToRot);

                // den Stein an der richtigen Stelle einsetzen
                MoveStonesToWhiteFace();
            }
        }

        /// <summary>
        /// Bestimmt, ob sich der angegebene Stein in der richtigen Position relativ zum Pivot Element befindet
        /// </summary>
        /// <param name="edge">Der zu überprüfende weiße Kantenstein</param>
        /// <param name="pivot">Das Pivot Element</param>
        /// <returns>Den Wert der angibt, ob der weiße Kantenstein relativ zum Pivot Element in der richtigen Position ist</returns>
        protected static bool IsWhiteStoneRight(EdgeStone edge, in EdgeStone pivot) {
            if (pivot.GetColorPosition(WHITE).Face == UP)
                throw new InvalidProgramException();

            // die Verschiebung des Pivots relativ zum Würfel bestimmen            
            int pivotDelta = GetDelta(pivot);

            // die verschiebung des Kantensteins zum Würfel bestimmen
            int edgeDelta = GetDelta(edge);

            return edge.GetColorPosition(WHITE).Face == UP && pivotDelta == edgeDelta;
        }

        /// <summary>
        /// Gibt die Verschiebung einer Farbe relativ zur Würfelseite an, auf der sie sich befindet
        /// </summary>
        /// <param name="color">Die Farbe des Kantensteins</param>
        /// <param name="edge">Der Kantenstein, dessen Verschiebung bestimmt werden soll</param>
        /// <returns></returns>
        protected static int GetDelta(EdgeStone edge) {
            CubeColor color = edge.GetColors().FirstOrDefault(c => c != WHITE);
            if (color == WHITE || color == YELLOW) {
                throw new ArgumentOutOfRangeException(nameof(color));
            }

            CubeFace face = edge.GetColorPosition(color).Face;
            if (face == UP || face == DOWN)
                throw new InvalidOperationException();

            CubeFace colorFace = Cube.GetFace(color);

            int delta = Array.IndexOf(MiddleLayerFaces, face) - Array.IndexOf(MiddleLayerFaces, colorFace);
            // den Wert zwischen -1 und 2 bringen
            delta += 4;
            delta %= 4;
            return delta == 3 ? -1 : delta;
        }
    }
}
