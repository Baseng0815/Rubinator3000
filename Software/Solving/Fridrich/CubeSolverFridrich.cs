using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.Solving {
    public partial class CubeSolverFridrich : CubeSolver {
        public override bool Solved => GetCubeSolved();

        public CubeSolverFridrich(Cube cube) : base((Cube)cube.Clone()) {
            
        }

        public override void SolveCube() {
            Log.LogMessage("Solve white cross");
#if DEBUG_MOVES
            Log.MoveLogLogging("Solve white cross");
#endif
            // Das weiße Kreuz lösen
            CrossSolver cross = new CrossSolver(cube);
            cross.SolveCube();
            SolvingMoves.AddRange(cross.SolvingMoves);

            Log.LogMessage("Solve F2L");
#if DEBUG_MOVES
            Log.MoveLogLogging("Solve F2L");
#endif
            // F2L
            FTLSolver ftl = new FTLSolver(cube);
            ftl.SolveCube();
            SolvingMoves.AddRange(ftl.SolvingMoves);

            Log.LogMessage("Solve last Layer");

#if DEBUG_MOVES
            Log.MoveLogLogging("Solve last Layer");
#endif
            // OLL and PLL
            LLSolver llSolver = new LLSolver(cube);
            llSolver.SolveCube();
            SolvingMoves.AddRange(llSolver.SolvingMoves);
        }

        protected override bool CheckCube(Cube cube) {
            return true;
        }
    }
}
