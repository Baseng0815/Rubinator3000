using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.Solving {
    public partial class CubeSolverFridrich : CubeSolver {
        public override bool Solved => GetCubeSolved();

        public CubeSolverFridrich(Cube cube) : base(cube) {

        }

        protected override void CalcMoves() {
            // Das weiße Kreuz lösen
            CrossSolver cross = new CrossSolver(cube);
            moves.AddRange(cross.GetMoves());

            try {               
                CalcFTL();

                CalcOLL();

                CalcPLL();

                bool solved = GetCubeSolved();

                if (!solved) {
                    moves.Clear();
                }
            }
            catch (Exception) {
                throw;
            }
        }
        
        partial void CalcFTL();
        partial void CalcOLL();
        partial void CalcPLL();        
    }
}
