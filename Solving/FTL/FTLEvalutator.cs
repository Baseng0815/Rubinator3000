using System;
using System.Linq;
using static Rubinator3000.CubeColor;
using static Rubinator3000.CubeFace;

namespace Rubinator3000.Solving {
    internal class FTLEvaluator : FTLSolver {
        protected new Cube cube;

        public FTLEvaluator(Cube cube) : base(null) {
            UpdateCube(cube);
        }

        public void UpdateCube(Cube cube) {
            this.cube = (Cube)cube.Clone();
        }

        public new MoveCollection CalcMoves(FTLPair pair) {
            base.CalcPairMoves(pair);

            return base.moves;
        }
    }
}