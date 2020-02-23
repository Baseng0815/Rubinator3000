using RubinatorCore.CubeRepresentation;

namespace RubinatorCore.Solving {
    public partial class CubeSolverFridrich : CubeSolver {
        public override bool Solved => GetCubeSolved();

        public CubeSolverFridrich(Cube cube) : base((Cube)cube.Clone()) {

        }

        public override void SolveCube() {
            Log.LogMessage("Solve white cross");
            // Das weiße Kreuz lösen
            CrossSolver cross = new CrossSolver(cube);
            cross.SolveCube();
            SolvingMoves.AddRange(cross.SolvingMoves);

            Log.LogMessage("Solve F2L");
            // F2L
            F2LSolver ftl = new F2LSolver(cube);
            ftl.SolveCube();
            SolvingMoves.AddRange(ftl.SolvingMoves);

            Log.LogMessage("Solve last Layer");

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
