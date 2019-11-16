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

            // corner in slot and white face on white side
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
                            MoveSlotUp(pair.Corner);

                            // handle false paired yellow layer
                            FalsePairedDownLayer(pair);
                        }
                        // edge in other slot
                        else {
                            // move edge on yellow layer
                            MoveSlotUp(pair.Edge);

                            // handle corner right and edge yellow layer
                            CornerInSlot_WhiteUp_EdgeDown(pair);
                        }
                    }
                    // edge on yellow layer
                    else {
                        CornerInSlot_WhiteUp_EdgeDown(pair);
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
                                MoveSlotUp(pair.Corner);

                                // handle as right paired
                                RightPairedDownLayer(pair);
                            }
                            else {
                                // move edge on yellow layer
                                MoveSlotUp(pair.Edge);

                                // handle as false paired
                                FalsePairedDownLayer(pair);
                            }
                        }
                        // edge in other slot
                        else {
                            // move edge up
                            MoveSlotUp(pair.Edge);

                            // handle corner in slot and edge on yellow layer
                            CornerInSlot_WhiteUp_EdgeDown(pair);
                        }
                    }
                    else {
                        // handle corner in slot and edge on yellow layer
                        CornerInSlot_WhiteUp_EdgeDown(pair);
                    }
                }
            }
            // corner in slot and white face to side 
            else if (pair.Corner.GetPositions().Any(p => p.Face == UP)) {
                // get the color on side which is not white
                Position cornerSidePos = pair.Corner.GetPosition(p => p.Face != UP && p != pair.CornerWhitePosition);
                CubeColor cornerSideColor = pair.Corner.GetColor(cornerSidePos);

                // edge in slot
                if (pair.EdgeInSlot) {
                    // edge in same slot as corner
                    if (pair.IsPaired()) {
                        // move pair to up layer and try to calc the moves again
                        MoveSlotUp(pair.Edge);

                        CalcPairMoves(pair);
                    }
                    // edge in other slot
                    else {
                        // move edge to yellow layer and try to calc the moves new
                        MoveSlotUp(pair.Edge);

                        CalcPairMoves(pair);
                    }
                }
                // edge on top
                else {
                    // edge color on top
                    CubeColor edgeDownColor = pair.Edge.GetColor(p => p.Face == DOWN);

                    // edge right
                    if (cornerSideColor == edgeDownColor) {
                        // handle crocodile
                        CornerInSlot_WhiteSide_EdgeRightDown(pair);
                    }
                    else {
                        CornerInSlot_WhiteSide_EdgeFalseDown(pair);
                    }
                }
            }
            // corner on down layer and white face on yellow side
            else if (pair.CornerWhitePosition.Face == DOWN) {
                // edge in slot
                if (pair.EdgeInSlot) {
                    CubeColor color = pair.Corner.GetColors().First(c => c != WHITE);
                    int cornerTile = pair.Corner.GetColorPosition(color).Tile;
                    int edgeTile = pair.Edge.GetColorPosition(color).Tile;

                    // edge right orientated
                    if (edgeTile + 3 == cornerTile) {
                        EaglePosition(pair);
                    }
                    // edge false orientated
                    else {
                        CornerDown_YellowSide_EdgeFalse(pair);
                    }
                }
                // edge on down layer
                else {
                    CornerDown_YellowSide_EdgeDown(pair);
                }
            }
            // corner on down layer and white face to side
            else {
                // edge in slot
                if (pair.EdgeInSlot) {
                    // transform to tiger position

                }
            }
        }

        protected override bool CheckCube(Cube cube) {
            for (int t = 1; t < 9; t += 2) {
                if (!(cube.At(UP, t) == WHITE)) {
                    return false;
                }
            }

            foreach (var face in MiddleLayerFaces) {
                if (!(cube.At(face, 1) == Cube.GetFaceColor(face))) {
                    return false;
                }
            }

            return true;
        }

        protected static bool EdgeFalseInRightSlot(EdgeStone edge) {
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
