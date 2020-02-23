using RubinatorCore.CubeRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using static RubinatorCore.CubeRepresentation.CubeColor;
using static RubinatorCore.CubeRepresentation.CubeFace;

namespace RubinatorCore.Solving {
    /// <summary>
    /// Ein <see cref="CubeSolver"/> zum Lösen des weißen Kreuzes
    /// </summary>
    public class CrossSolver : CubeSolver {
        #region CubeSolver
        /// <summary>
        /// Der Pivotstein
        /// </summary>
        protected EdgeStone pivotStone;
        /// <summary>
        /// Eine Auflistung der weißen Kantensteine
        /// </summary>
        protected IEnumerable<EdgeStone> whiteEdges;

        /// <summary>
        /// Gibt die Orientierung der weißen Seite zurück
        /// </summary>
        protected int WhiteFaceOrientation {
            get {
                // Pivotstein nicht gesetzt
                if (pivotStone.Colors == null)
                    return 0;

                // Pivot-Delta zurückgeben
                return GetDelta(pivotStone);
            }
        }

        /// <summary>
        /// Erstellt einen neuen <see cref="CrossSolver"/> mit dem eingegebenen Würfel
        /// </summary>
        /// <param name="cube">Der Würfel bei dem das weiße Kreuz gelöst werden soll</param>
        public CrossSolver(Cube cube) : base(cube) {
            // eine Abfrage erstellen, die die weißen Kantensteine zurückgibt
            whiteEdges = cube.Edges.Where(e => e.HasColor(WHITE));
        }

        /// <summary>
        /// Gibt zurück, ob das weiße Kreuz gelöst ist
        /// </summary>
        public override bool Solved {
            get {
                // die Flächen 1, 3, 5, 7 überprüfen
                for (int t = 1; t < 9; t += 2) {
                    // wenn nicht weiß, false zurückgeben
                    if (cube.At(UP, t) != WHITE)
                        return false;
                }

                // zurückgeben, ob die weißen Kantensteine an richtiger Position sind 
                return MiddleLayerFaces.All(f => cube.At(f, 1) == Cube.GetFaceColor(f));
            }
        }

        /// <summary>
        /// Löst das weiße Kreuz
        /// </summary>
        public override void SolveCube() {
            // überprüfen, ob das weiße Kreuz gelöst ist
            if (Solved)
                return;

            // das Pivotstein auswählen
            // überprüfen, ob eine weiße Fläche eines Kantensteins auf der weißen Seite ist
            if (whiteEdges.Any(e => e.GetColorPosition(WHITE).Face == UP)) {
                // den Kantenstein auf der weißen Seite als Pivotstein festlegen und in die richtige Position bringen
                int orientation = GetWhiteFaceOrientation();

                DoMove(new Move(UP, orientation));
                pivotStone = whiteEdges.First(e => e.InRightPosition());
            }
            else {
                // den Pivotstein durch das Steinrating auswählen und in die richtige Position bringen
                IEnumerable<(EdgeStone edge, int value)> ratings = whiteEdges.Select(e => (e, GetStoneRating(e)));
                EdgeStone edgeStone = ratings.OrderBy(r => r.value).First().edge;

                HandleStone(edgeStone);
                pivotStone = edgeStone;
            }

            // die restlichen weißen Kantensteine bewerten und in die richtige Position bringen
            while (!whiteEdges.All(e => IsEdgeRight(e))) {
                // die Steinbewertungen aktualisieren
                var stoneRating = from e in whiteEdges
                                  where !IsEdgeRight(e)
                                  group e by GetStoneRating(e) into edgeRating
                                  where edgeRating.Key > 0
                                  orderby edgeRating.Key
                                  select edgeRating;

                // den Kantenstein mit den wenigsten Zügen in die richtige Position bringen
                EdgeStone edgeToSolve = stoneRating.First().First();

                HandleStone(edgeToSolve);
            }

            // die weiße Seite in die richtige orientierung drehen
            DoMove(new Move(UP, -WhiteFaceOrientation));

            // ausgeben, ob das weiße Kreuz gelöst werden konnte
            Log.LogMessage(Solved ? "Weißes Kreuz gelöst" : "Weißes Kreuz nicht gelöst");
        }
        #endregion

        #region StoneHandling
        /// <summary>
        /// Bringt einen weißen Kantenstein in die richtige Position relativ zum Pivotstein
        /// </summary>
        /// <param name="edge">Der weiße Kantenstein, der in die richtige Position gebracht wird</param>
        protected void HandleStone(EdgeStone edge) {
            // überpüfen, ob der Kantenstein eine weiße Fläche hat
            if (!edge.HasColor(WHITE))
                throw new ArgumentException("Der Kantenstein muss eine weiße Fläche besitzen", nameof(edge));

            // die Seite bestimmen, auf der sich die weiße Seite des Kantentsteins befindet
            CubeFace whitePosFace = edge.GetColorPosition(WHITE).Face;

            // den Kantenstein lösen
            switch (whitePosFace) {
                case UP:
                    HandleStoneWhiteFace(edge);
                    break;
                case DOWN:
                    HandleStoneYellowFace(edge);
                    break;
                case LEFT:
                case FRONT:
                case RIGHT:
                case BACK:
                    int tile = edge.GetColorPosition(WHITE).Tile;
                    if (tile == 1 || tile == 7)
                        HandleFalseOrientatedStone(edge);
                    else
                        HandleStoneMiddleLayer(edge);
                    break;
            }
        }

        /// <summary>
        /// Bringt einen weißen Kantenstein auf der weißen Seite in die richtige Position relativ zum Pivotstein
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        protected void HandleStoneWhiteFace(EdgeStone edge) {
            // überprüfen, ob die weiße Seite des Kantensteins auf der weißen Seite ist
            if (edge.GetColorPosition(WHITE).Face != UP)
                throw new ArgumentException("Die weiße Fläche des Kantensteins muss sich auf der weißen Seite befinden", nameof(edge));

            // die Seite bestimmen, die gedreht werden muss
            CubeFace faceToRot = edge.GetColorPosition(c => c != WHITE).Face;

            // Stein auf die mittlere Ebene bringen
            DoMove(new Move(faceToRot));

            // den Stein lösen
            HandleStoneMiddleLayer(edge);
        }

        /// <summary>
        /// Bring einen weißen Kantenstein auf der mittleren Ebene in die richtige Position relativ zum Pivotstein.
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        protected void HandleStoneMiddleLayer(EdgeStone edge) {
            // überprüfen, ob der Kantenstein auf der mittleren Ebene ist
            if (Array.TrueForAll(MiddleLayerFaces, f => edge.GetColorPosition(WHITE).Face != f) || edge.GetColorPosition(WHITE).Tile == 1 || edge.GetColorPosition(WHITE).Tile == 7)
                throw new ArgumentException("Der weiße Kantenstein muss sich auf der mittleren Ebene befinden", nameof(edge));

            // bestimmen, wie oft die weiße Seite gedreht werden muss, um den Stein richtig einsetzen zu können
            int delta = SolvingUtility.GetDelta(GetSecondColor(edge), edge.GetColorPosition(c => c != WHITE).Face, UP);

            // die weiße Seite so drehen, dass der Stein eingesetzt werden kann
            DoMove(new Move(UP, delta - WhiteFaceOrientation));

            // den Stein einsetzen
            Position secondPos = edge.GetColorPosition(c => c != WHITE);
            DoMove(new Move(secondPos.Face, secondPos.Tile == 3 ? 1 : -1));
        }

        /// <summary>
        /// Bring einen weißen falsch orientierten Kantenstein auf der mittleren Ebene in die richtige Position relativ zum Pivotstein.
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        protected void HandleFalseOrientatedStone(EdgeStone edge) {
            // überprüfen, ob der Stein auf der oberen oder unteren Ebene ist und die weiße Fläche zur Seite zeigt
            if (Array.TrueForAll(MiddleLayerFaces, f => edge.GetColorPosition(WHITE).Face != f) || edge.GetColorPosition(WHITE).Tile == 3 || edge.GetColorPosition(WHITE).Tile == 5)
                throw new ArgumentOutOfRangeException("Der weiße Kantenstein muss sich in der oberen oder unteren Ebene befinden und die weiße Fläche muss auf einer der seitlichen Seiten (Orange, Grün, Rot, Blau) sein", nameof(edge));

            // die zweite Farbe und die Position der weißen Fläche bestimmen
            CubeColor secndColor = GetSecondColor(edge);
            Position whitePos = edge.GetColorPosition(WHITE);

            // überprüfen, wie der Stein am schnellsten zu lösen ist
            // die Seiten der mittleren Ebene bestimmen, die sich neben der Seite des weißen Steins befinden
            int middleLayerFaceID = Array.IndexOf(MiddleLayerFaces, whitePos.Face);

            CubeFace leftFace = MiddleLayerFaces[(middleLayerFaceID + 3) % 4];
            CubeFace rightFace = MiddleLayerFaces[(middleLayerFaceID + 1) % 4];

            // berechnen, wie oft die weiße Seite in beiden Fällen gedreht werden müsste um den Stein in die richtige Position zu bringen
            int leftDelta = Math.Abs(SolvingUtility.GetDelta(secndColor, leftFace, UP));
            int rightDelta = Math.Abs(SolvingUtility.GetDelta(secndColor, rightFace, UP));

            // weißen Kantenstein lösen
            // überprüfen, ob die weiße Seite nach dem Einsetzen des Steins zurückgedreht werden muss
            bool rotateBack = false;
            int previousOrientation = WhiteFaceOrientation;

            if (whitePos.Tile == 7) {
                int[] tiles = { 3, 7, 5, 1 };
                rotateBack = cube.At(UP, tiles[middleLayerFaceID]) == WHITE;
            }

            // den Stein auf die mittlere Ebene bringen
            int direction = leftDelta > rightDelta ? 1 : -1;
            if (whitePos.Tile == 1) {
                direction *= -1;
            }
            DoMove(new Move(whitePos.Face, direction));

            // Stein auf mittlere Ebene lösen
            HandleStoneMiddleLayer(edge);

            // die weiße Seite wieder zurückdrehen
            if (rotateBack) {
                DoMove(new Move(UP, WhiteFaceOrientation - previousOrientation));
                DoMove(new Move(whitePos.Face, -direction));
            }
        }

        /// <summary>
        /// Bring einen weißen Kantenstein auf der gelben Seite in die richtige Position relativ zum Pivotstein.
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        protected void HandleStoneYellowFace(EdgeStone edge) {
            // überprüfen, ob sich die weiße Fläche auf der unteren Seite befindet
            if (edge.GetColorPosition(WHITE).Face != DOWN)
                throw new ArgumentException("Die weiße Fläche des Kantensteins muss sich auf der gelben Seite befinden", nameof(edge));

            // das Delta des Steines bestimmen
            int delta = SolvingUtility.GetDelta(GetSecondColor(edge), edge.GetColorPosition(c => c != WHITE).Face, UP);

            // die weiße Seite in die richtige Position drehen, um den Stein richtig einsetzen zu können
            DoMove(new Move(UP, delta - WhiteFaceOrientation));

            // den Stein einsetzen
            CubeFace faceToRot = edge.GetColorPosition(c => c != WHITE).Face;
            DoMove(new Move(faceToRot, 2));
        }
        #endregion

        #region Utility
        /// <summary>
        /// Überprüft den Würfel, ob er die Vorraussetzungen erfüllt, damit das weiße Kreuz gelöst werden kann
        /// </summary>
        /// <param name="cube">Der zu lösende Würfel</param>
        /// <returns></returns>
        protected override bool CheckCube(Cube cube) {
            return true;
        }

        /// <summary>
        /// Gibt die Anzahl der Drehungen zurück, damit die meisten Kantensteine auf der weißen Seite in der richtigen Position sind
        /// </summary>
        /// <returns></returns>
        protected int GetWhiteFaceOrientation() {
            // die weiße Seite vier mal drehen und die richtigen Steine zählen
            int[] count = new int[4];
            for (int i = 0; i < 4; i++) {
                count[i] = whiteEdges.Count(e => e.InRightPosition());
                DoMove(new Move(UP));
            }

            // die Anzahl der Drehungen zurückgeben, in der die meisten Steine richtig sind
            int maxCount = count.Max();
            return Array.IndexOf(count, maxCount);
        }

        /// <summary>
        /// Gibt den Farbunterschied eines weißen Kantensteins zurück
        /// </summary>
        /// <param name="edge">Der Kantenstein, dessen Farbunterschied bestimmt werden soll</param>
        /// <returns></returns>
        protected int GetDelta(EdgeStone edge) {
            // die zweite Farbe des weißen Kantensteins und die Position dieser Farbe bestimmen
            CubeColor edgeColor = edge.GetColors().First(c => c != WHITE);
            CubeFace face = edge.GetColorPosition(edgeColor).Face;

            // das Delta berechnen
            return SolvingUtility.GetDelta(edgeColor, face, UP);
        }

        /// <summary>
        /// Bestimmt, ob der Kantenstein relativ zum Pivot in der richtigen Position ist
        /// </summary>
        /// <param name="edge">Der Kantenstein, der überprüft werden soll</param>
        /// <returns></returns>
        protected bool IsEdgeRight(EdgeStone edge) {
            // Pivotstein nicht festgelegt
            if (pivotStone.Colors == null) {
                return edge.InRightPosition();
            }

            // weiße Fläche nicht auf der weißen Seite
            if (edge.GetColorPosition(WHITE).Face != UP)
                return false;

            // Pivot-Delta und Stein-Delta bestimmen und vergleichen
            int pivotDelta = GetDelta(pivotStone);
            int stoneDelta = GetDelta(edge);

            return pivotDelta == stoneDelta;
        }

        /// <summary>
        /// Gibt die zweite Farbe eines weißen Kantensteins zurück
        /// </summary>
        /// <param name="edge">Der Kantenstein, dessen zwiete Farbe bestimmt werden soll</param>
        /// <returns></returns>
        protected CubeColor GetSecondColor(EdgeStone edge) {
            // die Farben des Steins bestimmen
            IEnumerable<CubeColor> colors = edge.GetColors();

            // überprüfen, ob der Stein eine weiße Fläche hat
            if (!colors.Contains(WHITE))
                throw new ArgumentException("Der Kantenstein muss die Farbe Weiß besitzen");

            // die Farbe zurückgeben, die nicht Weiß ist
            return colors.First(c => c != WHITE);
        }

        /// <summary>
        /// Gibt die Anzahl der Züge zurück, die benötigt wird, um den Kantenstein in die richtige Position zu bringen.
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        /// <returns></returns>
		protected int GetStoneRating(EdgeStone edge) {
            // Stein in der richtigen Position
            if (IsEdgeRight(edge))
                return 0;

            Position whitePosition = edge.GetColorPosition(WHITE);
            int edgeDelta;

            // weiße Fläche auf der oberen oder unteren Seite
            if (whitePosition.Face == UP || whitePosition.Face == DOWN) {
                // das Delta des Steins bestimmen       
                if (whitePosition.Face == DOWN) {
                    edgeDelta = SolvingUtility.GetDelta(
                        GetSecondColor(edge),                           // die zweite Farbe des Steins
                        edge.GetColorPosition(c => c != WHITE).Face,    // die Seite, auf der sich die zweite Fläche befindet
                        UP);                                            // die Seite, die gedreht werden soll
                }
                else {
                    edgeDelta = GetDelta(edge);
                }

                // die Anzahl der Züge zurückgeben
                int sortingMoves = Math.Abs(SolvingUtility.NormalizeCount(edgeDelta - WhiteFaceOrientation));
                return 2 + sortingMoves;
            }
            // auf einer der mittleren Seiten
            else {
                switch (whitePosition.Tile) {
                    // falsch orientiert in der oberen Ebene
                    case 1:
                        edgeDelta = SolvingUtility.GetDelta(
                            GetSecondColor(edge),   // zweite Farbe
                            edge.GetColorPosition(WHITE).Face,   // die Seite, auf der sich die zweite Fläche befindet
                            UP);    // die Seite, die gedreht werden soll
                        int edgePivotDelta = SolvingUtility.NormalizeCount(edgeDelta - WhiteFaceOrientation);

                        // die Anzahl der Züge zurückgeben
                        return (edgePivotDelta % 2) * -1 + 3;
                    // falsch orientiert auf der unteren Ebene
                    case 7:
                        // diese Konstellation kann immer mit genau 3 Zügen gelöst werden
                        return 3;
                    // auf der mittleren Ebene
                    default:
                        edgeDelta = SolvingUtility.GetDelta(
                            edge.GetColors().First(c => c != WHITE),        // die zweite Farbe des Steins
                            edge.GetColorPosition(c => c != WHITE).Face,    // die Seite, auf der sich die zweite Fläche befindet
                            UP);                                            // die Seite, die gedreht werden soll
                        // die Anzahl der Züge zurückgeben
                        int sortingMoves = Math.Abs(SolvingUtility.NormalizeCount(edgeDelta - WhiteFaceOrientation));
                        return 1 + sortingMoves;
                }

            }
        }
        #endregion
    }
}