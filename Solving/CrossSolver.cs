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
        protected readonly IEnumerable<EdgeStone> whiteEdges;
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

            // bring rest of the stones in right position

        }

        #region StoneHandling
        protected void HandleStone(EdgeStone edge) {
            CubeFace whitePosFace = edge.GetColorPosition(WHITE).Face;

        }

        protected void HandleStoneWhiteFace(EdgeStone edge) {
            CubeFace faceToRot = edge.GetColorPosition(c => c != WHITE).Face;

            // bring stone to middle Layer
            DoMove(faceToRot);

            // handle stone on middle Layer
            HandleStoneMiddleLayer(edge);
        }

        protected void HandleStoneMiddleLayer(EdgeStone edge) {
            int delta = SolvingUtility.GetDelta(GetSecondColor(edge), edge.GetColorPosition(c => c != WHITE).Face, UP);

            // rotate the white face to insert the stone right to pivot
            DoMove(UP, delta - WhiteFaceOrientation);

            Position secondPos = edge.GetColorPosition(c => c != WHITE);
            DoMove(secondPos.Face, secondPos.Tile == 3 ? 1 : -1);
        }

        protected void HandleFalseOrientatedStone(EdgeStone edge) {
            CubeColor secndColor = GetSecondColor(edge);
            Position whitePos = edge.GetColorPosition(WHITE);

            // on up layer
            if(whitePos.Tile == 1) {
                // get face of white pos
                int middleLayerFaceID = Array.IndexOf(MiddleLayerFaces, whitePos.Face);
                // face in anti clockwise rotation of white position face
                CubeFace leftFace = MiddleLayerFaces[(middleLayerFaceID + 3) % 4];
                // face in clockwise roation of white position face
                CubeFace rightFace = MiddleLayerFaces[(middleLayerFaceID + 1) % 4];

                int leftDelta = Math.Abs(SolvingUtility.NormalizeCount(SolvingUtility.GetDelta(secndColor, leftFace, UP), -1));
                int rightDelta = Math.Abs(SolvingUtility.NormalizeCount(SolvingUtility.GetDelta(secndColor, rightFace, UP), -1));

                // move edge to middle layer
                if (leftDelta < rightDelta) {
                    DoMove(whitePos.Face, -1);
                }
                else {
                    DoMove(whitePos.Face);
                }
            }
            else {

            }
        }
        #endregion

        protected override bool CheckCube(Cube cube) {
            return true;
        }

        protected int GetWhiteFaceOrientation() {
            int[] count = new int[4];
            for (int i = 0; i < 4; i++) {
                count[i] = whiteEdges.Count(e => e.InRightPosition());
                DoMove(UP, addMove: false);
            }

            int maxCount = count.Max();
            return Array.IndexOf(count, maxCount);
        }

        protected int GetDelta(EdgeStone edge) {
            if (!edge.HasColor(WHITE) || edge.GetColorPosition(WHITE).Face != UP)
                throw new ArgumentException("Der Kantenstein muss eine weiße Fläche haben und diese muss sich auf der weißen Seite befinden");

            CubeColor edgeColor = edge.GetColors().First(c => c != WHITE);
            CubeFace face = edge.GetColorPosition(c => c != WHITE).Face;

            return SolvingUtility.GetDelta(edgeColor, face, UP);
        }

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
    }
}
