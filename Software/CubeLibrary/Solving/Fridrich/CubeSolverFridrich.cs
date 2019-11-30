using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeLibrary.Solving {
    public partial class CubeSolverFridrich : CubeSolver {
        public override bool Solved => GetCubeSolved();

        public CubeSolverFridrich(Cube cube) : base(cube) {

        }

        public override void SolveCube() {
            Log.LogStuff("Solve white cross");
            // Das weiße Kreuz lösen
            CrossSolver cross = new CrossSolver(cube);
            cross.SolveCube();

            Log.LogStuff("Solve F2L");
            // F2L
            FTLSolver ftl = new FTLSolver(cube);
            ftl.SolveCube();

            Log.LogStuff("Solve last Layer");
            // OLL and PLL
            LLSolver llSolver = new LLSolver(cube);
            llSolver.SolveCube();
        }

        public override Task SolveCubeAsync() {
            return Task.Factory.StartNew(async () => {
                // Das weiße Kreuz lösen
                CrossSolver cross = new CrossSolver(cube);
                cross.SolveCube();

                // F2L
                FTLSolver ftl = new FTLSolver(cube);
                await ftl.SolveCubeAsync();
                //moves.AddRange(ftl.GetMoves());

                // OLL and PLL
                LLSolver llSolver = new LLSolver(cube);
                llSolver.SolveCube();
            }).Unwrap();
        }

        protected override bool CheckCube(Cube cube) {
            return true;
        }
    }
}
