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
            throw new NotImplementedException();
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

        protected void FTL_Eagle(FTLPair pair) {            
            if (!eagleCase(pair))
                throw new InvalidOperationException("Die beiden Steine befinden sich nicht in \"Adler\"-Position");

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

        }
        #endregion

        private static bool OnDownLayer(IStone stone) {
            return stone.GetPositions().Any(p => p.Face == DOWN);
        }
    }
}
