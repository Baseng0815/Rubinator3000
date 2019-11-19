using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {

    public class CrossSolver : CubeSolver {
        #region CubeSolvered
        protected EdgeStone pivotStone;
        protected IEnumerable<EdgeStone> whiteEdges;
        protected int WhiteFaceOrientation {
            get {
                // pivot stone not set
                if (pivotStone.Colors == null)
                    return 0;

                // return pivot stone delta
                return GetDelta(pivotStone);
            }
        }

        public CrossSolver(Cube cube) : base(cube) {
            whiteEdges = cube.Edges.Where(e => e.HasColor(WHITE));
            cube.OnMoveDone += Cube_OnMoveDone;
        }

        private void Cube_OnMoveDone(object sender, MoveEventArgs e) {
            whiteEdges = cube.Edges.Where(edge => edge.HasColor(WHITE));
        }

        public override bool Solved {
            get {
                for (int t = 1; t < 9; t += 2) {
                    if (cube.At(UP, t) != WHITE)
                        return false;
                }

                return MiddleLayerFaces.All(f => cube.At(f, 1) == Cube.GetFaceColor(f));
            }
        }

        protected override void CalcMoves() {
            // check white cross solved
            if (Solved)
                return;

            // check if any white edge is on white face
            if (whiteEdges.Any(e => e.GetColorPosition(WHITE).Face == UP)) {
                // select pivot stone
                int orientation = GetWhiteFaceOrientation();

                DoMove(UP, orientation);
                pivotStone = whiteEdges.First(e => e.InRightPosition());
            }
            else {
                // select pivot by stone rating
                IEnumerable<(EdgeStone edge, int value)> ratings = whiteEdges.Select(e => (e, GetStoneRating(e)));
                EdgeStone edgeStone = ratings.OrderBy(r => r.value).First().edge;

                // bring stone in right position
                HandleStone(edgeStone);
                pivotStone = edgeStone;
            }

            int count = 0;

            // bring rest of the stones in right position            
            while (!whiteEdges.All(e => IsEdgeRight(e))) {
                // update rating
                var stoneRating = from e in whiteEdges
                                  where !IsEdgeRight(e)
                                  group e by GetStoneRating(e) into edgeRating
                                  where edgeRating.Key > 0
                                  orderby edgeRating.Key
                                  select edgeRating;

                EdgeStone edgeToSolve = stoneRating.First().First();

                HandleStone(edgeToSolve);
            }

            DoMove(UP, -WhiteFaceOrientation);
        }
        #endregion

        #region StoneHandling
        /// <summary>
        /// Bringt einen weißen Kantenstein in die richtige Position relativ zum Pivot
        /// </summary>
        /// <param name="edge">Der weiße Kantenstein, der in die richtige Position gebracht wird</param>
        protected void HandleStone(EdgeStone edge) {
            if (!edge.HasColor(WHITE))
                throw new ArgumentException("Der Kantenstein muss eine weiße Fläche besitzen", nameof(edge));
            CubeFace whitePosFace = edge.GetColorPosition(WHITE).Face;

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
        /// Bringt einen weißen Kantenstein auf der weißen Seite in die richtige Position relativ zum Pivotstein.
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        protected void HandleStoneWhiteFace(EdgeStone edge) {
            if (edge.GetColorPosition(WHITE).Face != UP)
                throw new ArgumentException("Die weiße Fläche des Kantensteins muss sich auf der weißen Seite befinden", nameof(edge));

#if DEBUG
            Log.LogStuff("Handle stone white face\r\n\t" + edge.ToString());
#endif

            CubeFace faceToRot = edge.GetColorPosition(c => c != WHITE).Face;

            // bring stone to middle Layer
            DoMove(faceToRot);

            // handle stone on middle Layer
            HandleStoneMiddleLayer(edge);
        }

        /// <summary>
        /// Bring einen weißen Kantenstein auf der mittleren Ebene in die richtige Position relativ zum Pivotstein.
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        protected void HandleStoneMiddleLayer(EdgeStone edge) {
            if (Array.TrueForAll(MiddleLayerFaces, f => edge.GetColorPosition(WHITE).Face != f) || edge.GetColorPosition(WHITE).Tile == 1 || edge.GetColorPosition(WHITE).Tile == 7)
                throw new ArgumentException("Der weiße Kantenstein muss sich auf der mittleren Ebene befinden", nameof(edge));

#if DEBUG
            Log.LogStuff("Handle stone middle layer\r\n\t" + edge.ToString());
#endif

            int delta = SolvingUtility.GetDelta(GetSecondColor(edge), edge.GetColorPosition(c => c != WHITE).Face, UP);

            // rotate the white face to insert the stone right to pivot
            DoMove(UP, delta - WhiteFaceOrientation);

            Position secondPos = edge.GetColorPosition(c => c != WHITE);
            DoMove(secondPos.Face, secondPos.Tile == 3 ? 1 : -1);
        }

        /// <summary>
        /// Bring einen weißen falsch orientierten Kantenstein auf der mittleren Ebene in die richtige Position relativ zum Pivotstein.
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        protected void HandleFalseOrientatedStone(EdgeStone edge) {
            if (Array.TrueForAll(MiddleLayerFaces, f => edge.GetColorPosition(WHITE).Face != f) || edge.GetColorPosition(WHITE).Tile == 3 || edge.GetColorPosition(WHITE).Tile == 5)
                throw new ArgumentOutOfRangeException("Der weiße Kantenstein muss sich in der oberen oder unteren Ebene befinden und die weiße Fläche muss auf einer der seitlichen Seiten (Orange, Grün, Rot, Blau) sein", nameof(edge));

#if DEBUG
            Log.LogStuff("Handle false orientated stone\r\n\t" + edge.ToString());
#endif

            // get edge information
            CubeColor secndColor = GetSecondColor(edge);
            Position whitePos = edge.GetColorPosition(WHITE);

            // get face of white pos
            int middleLayerFaceID = Array.IndexOf(MiddleLayerFaces, whitePos.Face);
            // face in anti clockwise rotation of white position face
            CubeFace leftFace = MiddleLayerFaces[(middleLayerFaceID + 3) % 4];
            // face in clockwise roation of white position face
            CubeFace rightFace = MiddleLayerFaces[(middleLayerFaceID + 1) % 4];

            int leftDelta = Math.Abs(SolvingUtility.NormalizeCount(SolvingUtility.GetDelta(secndColor, leftFace, UP), -1));
            int rightDelta = Math.Abs(SolvingUtility.NormalizeCount(SolvingUtility.GetDelta(secndColor, rightFace, UP), -1));

            // on up layer
            if (whitePos.Tile == 1) {
                // move edge to middle layer
                if (leftDelta < rightDelta) {
                    DoMove(whitePos.Face, -1);
                }
                else {
                    DoMove(whitePos.Face);
                }
                HandleStoneMiddleLayer(edge);
            }
            else {
                // check if the face must rotated back after stone handling
                int[] tiles = { 3, 7, 5, 1 };
                bool rotBack = cube.At(UP, tiles[middleLayerFaceID]) == WHITE;

                // move edge to middle layer
                int direction = leftDelta > rightDelta ? 1 : -1;
                DoMove(whitePos.Face, direction);

                HandleStoneMiddleLayer(edge);

                if (rotBack)
                    DoMove(whitePos.Face, -direction);
            }
        }

        /// <summary>
        /// Bring einen weißen Kantenstein auf der gelben Seite in die richtige Position relativ zum Pivotstein.
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        protected void HandleStoneYellowFace(EdgeStone edge) {
            if (edge.GetColorPosition(WHITE).Face != DOWN)
                throw new ArgumentException("Die weiße Fläche des Kantensteins muss sich auf der gelben Seite befinden", nameof(edge));

#if DEBUG
            Log.LogStuff("Handle stone yellow face\r\n\t" + edge.ToString());
#endif

            int delta = SolvingUtility.GetDelta(GetSecondColor(edge), edge.GetColorPosition(c => c != WHITE).Face, UP);

            // rotate the white face to insert the stone right to pivot
            DoMove(UP, delta - WhiteFaceOrientation);

            CubeFace faceToRot = edge.GetColorPosition(c => c != WHITE).Face;
            DoMove(faceToRot, 2);
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
            int[] count = new int[4];
            for (int i = 0; i < 4; i++) {
                count[i] = whiteEdges.Count(e => e.InRightPosition());
                DoMove(UP, addMove: false);
            }

            int maxCount = count.Max();
            return Array.IndexOf(count, maxCount);
        }

        /// <summary>
        /// Gibt den Farbunterschied eines weißen Kantensteins zurück
        /// </summary>
        /// <param name="edge">Der Kantenstein, dessen Farbunterschied bestimmt werden soll</param>
        /// <returns></returns>
        protected int GetDelta(EdgeStone edge) {
            CubeColor edgeColor = edge.GetColors().First(c => c != WHITE);
            CubeFace face = edge.GetColorPosition(c => c != WHITE).Face;

            return SolvingUtility.GetDelta(edgeColor, face, UP);
        }

        /// <summary>
        /// Bestimmt, ob der Kantenstein relativ zum Pivot in der richtigen Position ist
        /// </summary>
        /// <param name="edge">Der Kantenstein, der überprüft werden soll</param>
        /// <returns></returns>
        protected bool IsEdgeRight(EdgeStone edge) {
            // pivot stone not set
            if (pivotStone.Colors == null) {
                return edge.InRightPosition();
            }

            // white side of edge not on white face
            if (edge.GetColorPosition(WHITE).Face != UP)
                return false;

            // get pivot delta and stone delta to compare
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
            IEnumerable<CubeColor> colors = edge.GetColors();

            if (!colors.Contains(WHITE))
                throw new ArgumentException("Der Kantenstein muss die Farbe Weiß besitzen");

            return colors.First(c => c != WHITE);
        }

        /// <summary>
        /// Gibt die Anzahl der Züge zurück, die benötigt wird, um den Kantenstein in die richtige Position zu bringen.
        /// </summary>
        /// <param name="edge">Der Kantenstein, der in die richtige Position gebracht werden soll</param>
        /// <returns></returns>
		protected int GetStoneRating(EdgeStone edge) {
            // on white face and same delta as pivot stone
            if (IsEdgeRight(edge))
                return 0;

            Position whitePosition = edge.GetColorPosition(WHITE);
            int edgeDelta;
            // on white face and not same delta as pivot stone or on yellow face
            if (whitePosition.Face == UP || whitePosition.Face == DOWN) {
                // get delta of edge                
                if (whitePosition.Face == DOWN) {
                    edgeDelta = SolvingUtility.GetDelta(
                        GetSecondColor(edge),    // second color
                        edge.GetColorPosition(c => c != WHITE).Face, // second color face color
                        UP); // face to rotate                   
                }
                else {
                    edgeDelta = GetDelta(edge);
                }

                // return 2 + moves white face count                                
                int sortingMoves = Math.Abs(SolvingUtility.NormalizeCount(edgeDelta - WhiteFaceOrientation, -1));
                return 2 + sortingMoves;
            }
            // on middle layer
            else {
                switch (whitePosition.Tile) {
                    // false orientated on white face
                    case 1:
                        edgeDelta = SolvingUtility.GetDelta(
                            GetSecondColor(edge),   //second color
                            edge.GetColorPosition(WHITE).Face,   // middle layer face with white side of edge
                            UP);    //face to rotate
                        int edgePivotDelta = SolvingUtility.NormalizeCount(edgeDelta - WhiteFaceOrientation);

                        // return number of moves
                        return (edgePivotDelta % 2) * -1 + 3;
                    // false orientated on yellow face
                    case 7:
                        // this position can always solved in 3 moves
                        return 3;
                    // on middle layer
                    default:
                        edgeDelta = SolvingUtility.GetDelta(
                            edge.GetColors().First(c => c != WHITE),    // second color
                            edge.GetColorPosition(c => c != WHITE).Face,    // second color face color
                            UP);    // face to rotate
                        int sortingMoves = Math.Abs(SolvingUtility.NormalizeCount(edgeDelta - WhiteFaceOrientation, -1));
                        return 1 + sortingMoves;
                }

            }
        }
        #endregion
    }
}