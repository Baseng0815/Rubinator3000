using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {
    public partial class FTLSolver : CubeSolver {
        public override bool Solved {
            get {
                for (int f = 0; f < 4; f++) {
                    CubeColor faceColor = MiddleLayerFaces[f].GetFaceColor();
                    for (int t = 0; t < 6; t++) {
                        if (!(cube.At(f, t) == faceColor)) {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        private IEnumerable<FTLPair> pairs;

        public FTLSolver(Cube cube) : base(cube) {
            Func<EdgeStone, CornerStone, bool> edgeSelector = (e, c) => {
                var colors = c.GetColors().Except(new List<CubeColor>() { WHITE });
                return e.GetColors().All(color => colors.Contains(color));
            };

            pairs = from corner in cube.Corners
                    where corner.HasColor(WHITE)
                    select new FTLPair(corner,
                            cube.Edges.First(e => edgeSelector(e, corner)),
                            cube);
        }

        protected override void CalcMoves() {

        }

        protected void CalcPairMoves(FTLPair pair, ref MoveCollection moves, bool addMoves = true) {
            CubeFace faceToRot;
            int direction;

            if (!addMoves && moves == null)
                throw new ArgumentNullException(nameof(moves));

            void DoMove(CubeFace face, int dir = 1) {
                this.DoMove(face, dir, addMoves);
                if (!addMoves) moves.Add(face, dir);
            }

            // corner in slot
            if (pair.CornerWhitePosition.Face == UP) {
                // in right slot
                if (pair.Corner.InRightPosition()) {
                    // edge in slot
                    if (pair.EdgeInSlot) {
                        // already paired and in right slot
                        if (pair.Edge.InRightPosition()) {
                            return;
                        }
                        // edge false in slot
                        else if (EdgeFalseInRightSlot(pair.Edge)) {
                            // move pair to yellow layer
                            var cornerPos = pair.Corner.GetPositions().First(p => p.Face != UP);
                            faceToRot = cornerPos.Face;
                            direction = cornerPos.Tile == 2 ? 1 : -1;

                            DoMove(faceToRot, direction);
                            DoMove(DOWN, direction);
                            DoMove(faceToRot, -direction);

                            // handle false paired yellow layer
                            // goto falsePairedDownLayer;
                        }
                        // edge in other slot
                        else {
                            // move edge on yellow layer
                            var edgePos = pair.Edge.Positions.Item1;
                            faceToRot = edgePos.Face;
                            direction = edgePos.Tile == 5 ? 1 : -1;

                            DoMove(faceToRot, direction);
                            DoMove(DOWN, direction);
                            DoMove(faceToRot, -direction);

                            // handle corner right edge yellow layer
                            //goto cornerRightEdgeDOWN;
                        }
                    }
                    // edge on yellow layer
                    else {
                        // move edge to right orientation
                        var edgeUpColorFace = Cube.GetFace(pair.Edge.GetColor(p => p.Face == DOWN));
                        CubeFace opponentFace = Cube.GetOpponentFace(edgeUpColorFace);

                        while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != opponentFace)
                            DoMove(DOWN);

                        // pair the stones
                        var cornerPos = pair.Corner.GetPosition(p => p.Face == edgeUpColorFace);
                        faceToRot = cornerPos.Face;
                        direction = cornerPos.Tile == 2 ? 1 : -1;

                        DoMove(faceToRot, direction);
                        DoMove(DOWN, -direction);
                        DoMove(faceToRot, -direction);

                        // handle right paired down layer
                        // handleRightPairedDownLayer(pair, ref moves, addMoves);
                    }
                }
                // in false slot
                else {
                    // edge in slot
                    if (pair.EdgeInSlot) {
                        IEnumerable<CubeFace> cornerSideFaces = from pos in pair.Corner.GetPositions()
                                                                where pos.Face != UP
                                                                select pos.Face;
                        // edge in same slot 
                        if (pair.Edge.GetPositions().All(p => cornerSideFaces.Contains(p.Face))) {
                            // right paired
                            if (pair.Paired) {

                            }
                        }
                    }
                    else {
                        // move edge to right orientation
                        var edgeUpColorFace = Cube.GetFace(pair.Edge.GetColor(p => p.Face == DOWN));
                        CubeFace opponentFace = Cube.GetOpponentFace(edgeUpColorFace);

                        while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != opponentFace)
                            DoMove(DOWN);

                        // pair the stones
                        var cornerPos = pair.Corner.GetPosition(p => p.Face == edgeUpColorFace);
                        faceToRot = cornerPos.Face;
                        direction = cornerPos.Tile == 2 ? 1 : -1;

                        DoMove(faceToRot, direction);
                        DoMove(DOWN, -direction);
                        DoMove(faceToRot, -direction);

                        // handle right paired down layer
                        // handleRightPairedDownLayer(pair, ref moves, addMoves);
                    }
                }
            }
        }

        protected void MoveEdgeUp(EdgeStone edge) {
            if (edge.GetPositions().All(p => MiddleLayerFaces.Contains(p.Face)))
                return;

            Position pos = edge.GetPositions().First();
            CubeFace faceToRot = pos.Face;
            int direction = pos.Tile == 5 ? 1 : -1;
            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);
        }

        protected bool EagleSolveable(FTLPair pair) {
            if (!pair.EdgeInSlot)
                return false;

            Position pos = pair.Edge.GetPositions().First(p => p.Tile == 3);
            CubeColor posColor = pair.Edge.GetColor(pos);

            Position cornerColorPos = pair.Corner.GetColorPosition(posColor);
            return cornerColorPos.Tile == 6;
        }

        protected override bool CheckCube(Cube cube) {
            for (int t = 0; t < 9; t++) {
                if (!(cube.At(UP, t) == WHITE)) {
                    return false;
                }
            }

            return true;
        }

        #region FTL Case Handling        
        protected void FTL_Insert_RightPaired(FTLPair pair) {
            if (!rightPairedCase(pair))
                throw new InvalidOperationException();

            // rotate pair in right position
            // 0-2 Moves
            (Position pos, CubeColor color) = pair.Corner.GetPositions().Select(p => (p, cube.At(p))).First(p => p.p.Face != UP && p.Item2 != WHITE);
            int delta = SolvingUtility.GetDelta(color, pos.Face, DOWN) - 1;

            DoMove(DOWN, delta);

            // insert the pair in right slot
            // 3 Moves
            var faceToRot = Cube.GetOpponentFace(pair.CornerWhitePosition.Face);
            int direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;
            DoMove(faceToRot, direction);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);

            // sum 3-5 Moves
        }

        protected void FTL_Insert_FalsePaired(FTLPair pair) {
            if (!falsePairedCase(pair))
                throw new InvalidOperationException();

            // move corner above right slot
            // 0-2 moves
            Position pos = pair.Corner.GetPositions().First(p => p.Face == DOWN);
            int delta = SolvingUtility.GetDelta(cube.At(pos), pair.CornerWhitePosition.Face, DOWN);
            DoMove(DOWN, delta);

            // do algorithm     7-10 moves            
            var faceToRot = Cube.GetFace(cube.At(pair.Corner.GetPositions().First(p => p.Face == DOWN)));

            FTLPair secondPair = pairs.First(p => p.Edge.HasColor(Cube.GetFaceColor(faceToRot)) && p != pair);
            bool secondPairSolved = secondPair.Solved;

            int direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;
            DoMove(faceToRot, direction);
            DoMove(DOWN, 2);
            DoMove(faceToRot, 2);
            DoMove(DOWN, -direction);
            DoMove(faceToRot - direction);

            if (secondPairSolved) {
                DoMove(faceToRot, -direction);
                DoMove(DOWN, -direction);
                DoMove(faceToRot, direction);
            }
        }

        protected void FTL_Tiger(FTLPair pair) {
            if (!eagleCase(pair))
                throw new InvalidOperationException("Die beiden Steine befinden sich nicht in \"Tiger\"-Position");

            Position cornerSidePos = pair.Corner.GetPositions().First(p => p.Face != DOWN && p != pair.CornerWhitePosition);
            CubeColor cornerSideColor = pair.Corner.GetColor(cornerSidePos);

            // rotate stones right
            // 0-2 moves
            int delta = SolvingUtility.GetDelta(cornerSideColor, cornerSidePos.Face, DOWN);
            DoMove(DOWN, delta);

            // pair and insert
            // 3 moves
            var faceToRot = pair.CornerWhitePosition.Face;
            int direction = pair.CornerWhitePosition.Tile == 8 ? 1 : -1;
            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);
        }

        protected void FTL_Crocodile(FTLPair pair) {
            if (!crocodileCase(pair))
                throw new InvalidOperationException("Die beiden Steine befinden sich nicht in \"Krokodil\"-Position");

            Position edgeSidePos = pair.Edge.GetPositions().First(p => p.Face != DOWN);
            CubeColor edgeSideColor = pair.Edge.GetColor(edgeSidePos);

            // rotate edge right
            // 0-2 moves
            int delta = SolvingUtility.GetDelta(edgeSideColor, edgeSidePos.Face, DOWN);
            DoMove(DOWN, delta);

            // pair and insert
            var faceToRot = pair.CornerWhitePosition.Face;
            int direction = pair.CornerWhitePosition.Tile == 2 ? 1 : -1;

        }
        #endregion

        private static bool EdgeFalseInRightSlot(EdgeStone edge) {
            // edge in any slot
            if (edge.GetPositions().All(p => p.Face != DOWN)) {
                var color1Face = Cube.GetFace(edge.Colors.Item1);
                var color2Face = Cube.GetFace(edge.Colors.Item2);

                return edge.GetColorPosition(edge.Colors.Item1).Face == color2Face
                    && edge.GetColorPosition(edge.Colors.Item2).Face == color1Face;
            }

            return false;
        }

        private static bool OnDownLayer(IStone stone) {
            return stone.GetPositions().Any(p => p.Face == DOWN);
        }
    }
}
