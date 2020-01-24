using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Rubinator3000.CubeColor;
using static Rubinator3000.CubeFace;

namespace Rubinator3000.Solving {
    public abstract class CubeSolver {

        protected readonly Cube cube;

        public MoveCollection SolvingMoves { get; private set; }

        /// <summary>
        /// Erstellt einen neuen CubeSolver und eine Kopie des aktuellen Cubes
        /// </summary>
        /// <param name="cube">Der zu lösende Cube</param>
        public CubeSolver(Cube cube) {
            if (!CheckCube(cube)) {
                throw new ArgumentException();
            }

            this.cube = cube;
            SolvingMoves = new MoveCollection();            
        }        

        /// <summary>
        /// Brechnet in einer abgleiteten Klasse, die Züge zum lösen des Würfels
        /// </summary>
        public abstract void SolveCube();

        public virtual Task SolveCubeAsync() {
            return Task.Run(SolveCube);
        }

        protected abstract bool CheckCube(Cube cube);

        /// <summary>
        /// Gibt in einer abgeleiteten Klasse an, ob der Solver seine Arbeit erfolgreich vollendet hat
        /// </summary>
        public abstract bool Solved { get; }


        protected void DoMove(Move move) {
            if (move.Count == 0)
                return;

            cube.DoMove(move);
            SolvingMoves.Add(move);
        }

        protected void DoMoves(IEnumerable<Move> moves) {
            foreach (var move in moves) {
                DoMove(move);
            }
        }

        /// <summary>
        /// Gibt zurück, ob der Würfel sich im gelösten Zustand befindet
        /// </summary>
        /// <returns>Den Wert, der angibt, ob der Würfel gelöst ist</returns>
        public bool GetCubeSolved() {
            for (int face = 0; face < 6; face++) {
                CubeColor faceColor = cube.At(face, 4);
                for (int tile = 0; tile < 9; tile++) {
                    if (faceColor != cube.At(face, tile))
                        return false;
                }
            }

            return true;
        }

        public static readonly CubeFace[] MiddleLayerFaces = { CubeFace.LEFT, CubeFace.FRONT, CubeFace.RIGHT, CubeFace.BACK };
        public static readonly Tuple<CubeColor, CubeColor>[] MiddleLayerEdgesColors = new Tuple<CubeColor, CubeColor>[4] {
            new Tuple<CubeColor, CubeColor>(ORANGE, GREEN),
            new Tuple<CubeColor, CubeColor>(GREEN, RED),
            new Tuple<CubeColor, CubeColor>(RED, BLUE),
            new Tuple<CubeColor, CubeColor>(BLUE, ORANGE)
        };
    }
}
