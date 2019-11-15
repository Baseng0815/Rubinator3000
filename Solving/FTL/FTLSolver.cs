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

        protected void CalcPairMoves(FTLPair pair) {
            CubeFace faceToRot;
            int direction;

            Action<CubeFace, int> doMove = (f, d) => DoMove(f, d);

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
                            MoveSlotUp(pair.Corner, doMove);

                            // handle false paired yellow layer
                            FalsePairedDownLayer(pair, doMove);
                        }
                        // edge in other slot
                        else {
                            // move edge on yellow layer
                            MoveSlotUp(pair.Edge, doMove);

                            // handle corner right edge yellow layer
                            //goto cornerRightEdgeDOWN;
                        }
                    }
                    // edge on yellow layer
                    else {
                        CornerInSlotEdgeDown(pair, doMove);
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
                                // move on yellow layer
                                MoveSlotUp(pair.Corner, doMove);

                                // handle as right paired
                                RightPairedDownLayer(pair, doMove);
                            }
                            else {
                                // move edge on yellow layer
                                MoveSlotUp(pair.Edge, doMove);

                                // handle as false paired
                                FalsePairedDownLayer(pair, doMove);
                            }
                        }
                        // edge in other slot
                        else {
                            // move edge up
                            MoveSlotUp(pair.Edge, doMove);

                            // handle corner in slot and edge on yellow layer
                            CornerInSlotEdgeDown(pair, doMove);
                        }
                    }
                    else {
                        // handle corner in slot and edge on yellow layer
                        CornerInSlotEdgeDown(pair, doMove);
                    }
                }
            }
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

        protected void FTL_Tiger(FTLPair pair) {            

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
