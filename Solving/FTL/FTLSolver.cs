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
            while(pairs.Any(pair => !pair.Solved)) {
                var pairMoves = from pair in pairs
                                where !pair.Solved
                                select FTLMoveCalculator.CalcMoves(pair, cube);

                int minMoves = pairMoves.Min(m => m.Count);
                MoveCollection moves = pairMoves.First(e => e.Count == minMoves);

                cube.DoMoves(moves);
                this.moves.AddRange(moves);
            }

            movesCalculated = true;
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
        
    }
}
