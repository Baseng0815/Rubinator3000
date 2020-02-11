using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RubinatorCore.CubeFace;
using static RubinatorCore.CubeColor;
using System.Threading;

namespace RubinatorCore.Solving {
    /// <summary>
    /// Ein <see cref="CubeSolver"/> zum Lösen des F2L
    /// </summary>
    public partial class F2LSolver : CubeSolver {
        /// <summary>
        /// Gibt an, ob das F2L gelöst ist
        /// </summary>
        public override bool Solved {
            get {
                for (int f = 0; f < 4; f++) {
                    int face = (int)MiddleLayerFaces[f];
                    CubeColor faceColor = MiddleLayerFaces[f].GetFaceColor();
                    for (int t = 0; t < 6; t++) {
                        if (!(cube.At(face, t) == faceColor)) {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Eine Auflistung der F2L-Paare
        /// </summary>
        private IEnumerable<F2LPair> pairs;

        /// <summary>
        /// Erstellt einen neuen <see cref="F2LSolver"/> mit dem eingegebenen Würfel
        /// </summary>
        /// <param name="cube">Der Würfel bei dem das F2L gelöst werden soll</param>
        public F2LSolver(Cube cube) : base(cube) {
            Func<EdgeStone, CornerStone, bool> edgeSelector = (e, c) => {
                var colors = c.GetColors().Except(new List<CubeColor>() { WHITE });
                return e.GetColors().All(color => colors.Contains(color));
            };

            // die F2L-Paare bestimmen
            pairs = from corner in cube.Corners
                    where corner.HasColor(WHITE)
                    select new F2LPair(corner,
                            cube.Edges.First(e => edgeSelector(e, corner)),
                            cube);
        }

        /// <summary>
        /// Löst das F2L
        /// </summary>
        public override void SolveCube() {
            while (pairs.Any(pair => !pair.Solved)) {
                // ungelöste Paare bestimmen
                F2LPair[] unsolvedPairs = pairs.Where(pair => !pair.Solved).ToArray();
                int pairIndex = -1;

                // das Paar mit den wenigsten Zügen lösen
                MoveCollection minMoves = null;
                for (int i = 0; i < unsolvedPairs.Length; i++) {
                    F2LMoveCalculator moveCalculator = new F2LMoveCalculator(unsolvedPairs[i], cube);
                    MoveCollection pairMoves = moveCalculator.CalcMoves();

                    if (minMoves == null || pairMoves.Count < minMoves.Count) {
                        minMoves = pairMoves;
                        pairIndex = i;
                    }
                }

                DoMoves(minMoves);
            }
        }

        /// <summary>
        /// Löst das F2L asynchron
        /// </summary>
        public override Task SolveCubeAsync() {
            return Task.Factory.StartNew(async () => {

                while (pairs.Any(pair => !pair.Solved)) {
                    // ungelöste Paare bestimmen
                    IEnumerable<F2LPair> unsolvedPairs = pairs.Where(pair => !pair.Solved);

                    // Aufgaben erstellen um alle Paare zu lösen
                    List<Task<MoveCollection>> tasks = new List<Task<MoveCollection>>();
                    foreach (var pair in unsolvedPairs) {
                        var task = new Task<MoveCollection>(() => {
                            F2LMoveCalculator moveCalculator = new F2LMoveCalculator(pair, cube);
                            return moveCalculator.CalcMoves();
                        });

                        tasks.Add(task);
                    }

                    // alle Aufgaben starten
                    tasks.ForEach(t => t.Start());

                    // das Paar mit den wenigsten Zügen lösen
                    MoveCollection minMoves = null;
                    while (tasks.Count > 0) {
                        var moves = await tasks.First();

                        if (minMoves == null || minMoves.Count > moves.Count) {
                            minMoves = moves;
                        }

                        tasks.RemoveAt(0);
                    }

                    DoMoves(minMoves);
                }

                Log.LogMessage(Solved ? "F2L gelöst" : "F2L nicht gelöst");
            }).Unwrap();
        }

        /// <summary>
        /// Überprüft, ob das weiße Kreuz gelöst ist
        /// </summary>
        /// <param name="cube">Der Würfel, der überprüft werden soll</param>
        /// <returns>Eine Wert, der angibt, ob das weiße Kreuz gelöst ist</returns>
        protected override bool CheckCube(Cube cube) {
            // überprüfen, ob die weiße Seite ein Kreuz aufweist
            for (int t = 1; t < 9; t += 2) {
                if (!(cube.At(UP, t) == WHITE)) {
                    return false;
                }
            }

            // überprüfen, ob die Kantensteine richtig sind
            foreach (var face in MiddleLayerFaces) {
                if (!(cube.At(face, 1) == Cube.GetFaceColor(face))) {
                    return false;
                }
            }

            return true;
        }

    }
}
