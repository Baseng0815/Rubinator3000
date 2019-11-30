using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CubeLibrary.CubeFace;
using static CubeLibrary.CubeColor;
using System.Threading;

namespace CubeLibrary.Solving {
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

        public override void SolveCube() {
            while (pairs.Any(pair => !pair.Solved)) {
                FTLPair[] unsolvedPairs = pairs.Where(pair => !pair.Solved).ToArray();
                int pairsCount = unsolvedPairs.Length;

                MoveCollection minMoves = null;
                for (int i = 0; i < pairsCount; i++) {
                    FTLMoveCalculator moveCalculator = new FTLMoveCalculator(unsolvedPairs[i], cube);
                    MoveCollection pairMoves = moveCalculator.CalcMoves();

                    if (minMoves == null || pairMoves.Count < minMoves.Count)
                        minMoves = pairMoves;
                }

                DoMoves(minMoves);                
            }            
        }

        public override Task SolveCubeAsync() {
            return Task.Factory.StartNew(async () => {

                while (pairs.Any(pair => !pair.Solved)) {
                    IEnumerable<FTLPair> unsolvedPairs = pairs.Where(pair => !pair.Solved);

                    List<Task<MoveCollection>> tasks = new List<Task<MoveCollection>>();
                    foreach (var pair in unsolvedPairs) {
                        var task = new Task<MoveCollection>(() => {
                            FTLMoveCalculator moveCalculator = new FTLMoveCalculator(pair, cube);
                            return moveCalculator.CalcMoves();
                        });

                        tasks.Add(task);
                    }

                    tasks.ForEach(t => t.Start());

                    MoveCollection minMoves = null;
                    while (tasks.Count > 0) {
                        var moves = await tasks.First();

                        if(minMoves == null || minMoves.Count > moves.Count) {
                            minMoves = moves;
                        }

                        tasks.RemoveAt(0);
                    }

                    DoMoves(minMoves);                    
                }                

            }).Unwrap();
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
